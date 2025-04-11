//-----------------------------------------------------------------------------
// <copyright file="ODataServiceCollectionExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Provides extension methods to add OData services.
/// </summary>
public static class ODataServiceCollectionExtensions
{
    /// <summary>
    /// Adds essential OData services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>A <see cref="IServiceCollection"/> that can be used to further configure the OData services.</returns>
    public static IServiceCollection AddOData(this IServiceCollection services)
    {
        return services.AddOData(opt => { });
    }

    /// <summary>
    /// Adds essential OData services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="setupAction">The OData options to configure the services with,
    /// including access to a service provider which you can resolve services from.</param>
    /// <returns>A <see cref="IServiceCollection"/> that can be used to further configure the OData services.</returns>
    public static IServiceCollection AddOData(this IServiceCollection services, Action<ODataMiniOptions> setupAction)
    {
        if (services == null)
        {
            throw Error.ArgumentNull(nameof(services));
        }

        if (setupAction == null)
        {
            throw Error.ArgumentNull(nameof(setupAction));
        }

        services.Configure(setupAction);

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.PropertyNamingPolicy = null;
            options.SerializerOptions.WriteIndented = true;
            options.SerializerOptions.Converters.Add(new TruncatedCollectionValueConverter());
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new SelectExpandWrapperConverter());
            options.SerializerOptions.Converters.Add(new PageResultValueConverter());
            options.SerializerOptions.Converters.Add(new DynamicTypeWrapperConverter());
            options.SerializerOptions.Converters.Add(new SingleResultValueConverter());
            options.SerializerOptions.Converters.Add(new TruncatedCollectionValueConverter());
        });

        return services;
    }

    /// <summary>
    /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
    /// type. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddODataQueryFilter(this IServiceCollection services)
    {
        return AddODataQueryFilter(services, new EnableQueryAttribute());
    }

    /// <summary>
    /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
    /// type. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="queryFilter">The action filter that executes the query.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddODataQueryFilter(this IServiceCollection services, IActionFilter queryFilter)
    {
        if (services == null)
        {
            throw Error.ArgumentNull(nameof(services));
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IFilterProvider>(new QueryFilterProvider(queryFilter)));
        return services;
    }

    /// <summary>
    /// Adds the core OData services required for OData requests.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    internal static IServiceCollection AddODataCore(this IServiceCollection services)
    {
        if (services == null)
        {
            throw Error.ArgumentNull(nameof(services));
        }

        //
        // Options
        //
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());

        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ODataMvcOptionsSetup>());

        // For Minimal API, we should call 'ConfigureHttpJsonOptions' to config the JsonConverter,
        // But, this extension has been introduced since .NET 7
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<JsonOptions>, ODataJsonOptionsSetup>());

        //
        // Parser & Resolver & Provider
        //
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IODataQueryRequestParser, DefaultODataQueryRequestParser>());

        services.TryAddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

        //
        // Routing
        //
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelProvider>());

        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ODataRoutingMatcherPolicy>());

        services.TryAddSingleton<IODataTemplateTranslator, DefaultODataTemplateTranslator>();

        services.TryAddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();

        return services;
    }
}
