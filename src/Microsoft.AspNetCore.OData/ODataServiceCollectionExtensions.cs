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
using Microsoft.AspNetCore.OData.Common;
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
    public static IServiceCollection AddOData1(this IServiceCollection services, Action<DefaultQueryConfigurations> setupAction)
    {

        services.AddScoped(sp => new ODataMessageWriterSettings
        {
            EnableMessageStreamDisposal = false,
            MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
        });


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

        // QueryValidators.
        services.AddSingleton<ICountQueryValidator, CountQueryValidator>();

        services.AddSingleton<IFilterQueryValidator, FilterQueryValidator>();
        services.AddSingleton<IODataQueryValidator, ODataQueryValidator>();
        services.AddSingleton<IOrderByQueryValidator, OrderByQueryValidator>();
        services.AddSingleton<ISelectExpandQueryValidator, SelectExpandQueryValidator>();
        services.AddSingleton<ISkipQueryValidator, SkipQueryValidator>();
        services.AddSingleton<ISkipTokenQueryValidator, SkipTokenQueryValidator>();
        services.AddSingleton<ITopQueryValidator, TopQueryValidator>();
        services.AddSingleton<IComputeQueryValidator, ComputeQueryValidator>();
        services.AddSingleton<SkipTokenHandler, DefaultSkipTokenHandler>();

        // SerializerProvider and DeserializerProvider.
        services.AddSingleton<IODataSerializerProvider, ODataSerializerProvider>();
        services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

        // Deserializers.
        services.AddSingleton<ODataResourceDeserializer>();
        services.AddSingleton<ODataEnumDeserializer>();
        services.AddSingleton<ODataPrimitiveDeserializer>();
        services.AddSingleton<ODataResourceSetDeserializer>();
        services.AddSingleton<ODataCollectionDeserializer>();
        services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
        services.AddSingleton<ODataActionPayloadDeserializer>();
        services.AddSingleton<ODataDeltaResourceSetDeserializer>();

        // Serializers.
        services.AddSingleton<ODataEnumSerializer>();
        services.AddSingleton<ODataPrimitiveSerializer>();
        services.AddSingleton<ODataDeltaResourceSetSerializer>();
        services.AddSingleton<ODataResourceSetSerializer>();
        services.AddSingleton<ODataCollectionSerializer>();
        services.AddSingleton<ODataResourceSerializer>();
        services.AddSingleton<ODataServiceDocumentSerializer>();
        services.AddSingleton<ODataEntityReferenceLinkSerializer>();
        services.AddSingleton<ODataEntityReferenceLinksSerializer>();
        services.AddSingleton<ODataErrorSerializer>();
        services.AddSingleton<ODataMetadataSerializer>();
        services.AddSingleton<ODataRawValueSerializer>();

        // Query Binders
        services.AddSingleton<IFilterBinder, FilterBinder>();
        services.AddSingleton<IOrderByBinder, OrderByBinder>();
        services.AddSingleton<ISelectExpandBinder, SelectExpandBinder>();

        // MiniAPI needs this to do the query validation.
        DefaultQueryConfigurations queryConfiguration = new DefaultQueryConfigurations();
        setupAction?.Invoke(queryConfiguration);

        // Inject the default query configuration from this options.
        services.AddSingleton(sp => queryConfiguration);

        // ODL query option parser needs this, but it seems there's no way to set up since the properties are internal. For example: FilterLimit is internal
        services.AddScoped(sp => new ODataUriParserSettings());

        // ODL query option parser needs this.
        services.AddSingleton<ODataUriResolver>(sp =>
            new UnqualifiedODataUriResolver
            {
                EnableCaseInsensitive = true, // by default to enable case insensitive
                EnableNoDollarQueryOptions = true // by default to enable no dollar sign.
            });

        //services.AddDefaultODataServices(ODataVersion.V4);

        return services;
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


public class ODataMiniOptions
{
    /// <summary>
    /// Gets the query configurations.
    /// </summary>
    private DefaultQueryConfigurations _queryConfigurations = new DefaultQueryConfigurations();

    public DefaultQueryConfigurations QueryConfigurations { get => _queryConfigurations; }

    public ODataVersion Version { get; set; } = ODataVersionConstraint.DefaultODataVersion;

    /// <summary>
    /// Gets or sets whether or not the OData system query options should be prefixed with '$'.
    /// </summary>
    public bool EnableNoDollarQueryOptions { get; set; } = true;

    public bool EnableCaseInsensitive { get; set; } = true;

    public ODataMiniOptions EnableAll(int? maxTopValue = null)
    {
        _queryConfigurations.EnableExpand = true;
        _queryConfigurations.EnableSelect = true;
        _queryConfigurations.EnableFilter = true;
        _queryConfigurations.EnableOrderBy = true;
        _queryConfigurations.EnableCount = true;
        _queryConfigurations.EnableSkipToken = true;
        SetMaxTop(maxTopValue);
        return this;
    }

    internal void Update(ODataMiniOptions otherOptions)
    {
        this._queryConfigurations.UpdateAll(otherOptions.QueryConfigurations);
        this.EnableNoDollarQueryOptions = otherOptions.EnableNoDollarQueryOptions;
        this.EnableCaseInsensitive = otherOptions.EnableCaseInsensitive;
    }

    internal ODataMiniOptions Clone()
    {
        return new ODataMiniOptions();
    }

    /// <summary>
    /// Enable $expand query options.
    /// </summary>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions Expand()
    {
        _queryConfigurations.EnableExpand = true;
        return this;
    }

    /// <summary>
    /// Enable $select query options.
    /// </summary>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions Select()
    {
        _queryConfigurations.EnableSelect = true;
        return this;
    }

    /// <summary>
    /// Enable $filter query options.
    /// </summary>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions Filter()
    {
        _queryConfigurations.EnableFilter = true;
        return this;
    }

    /// <summary>
    /// Enable $orderby query options.
    /// </summary>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions OrderBy()
    {
        _queryConfigurations.EnableOrderBy = true;
        return this;
    }

    /// <summary>
    /// Enable $count query options.
    /// </summary>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions Count()
    {
        _queryConfigurations.EnableCount = true;
        return this;
    }

    /// <summary>
    /// Enable $skiptoken query option.
    /// </summary>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions SkipToken()
    {
        _queryConfigurations.EnableSkipToken = true;
        return this;
    }

    /// <summary>
    ///Sets the maximum value of $top that a client can request.
    /// </summary>
    /// <param name="maxTopValue">The maximum value of $top that a client can request.</param>
    /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
    public ODataMiniOptions SetMaxTop(int? maxTopValue)
    {
        if (maxTopValue.HasValue && maxTopValue.Value < 0)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxTopValue), maxTopValue, 0);
        }

        _queryConfigurations.MaxTop = maxTopValue;
        return this;
    }
}