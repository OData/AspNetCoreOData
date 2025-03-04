//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointConventionBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Extension methods for adding <see cref="IODataQueryEndpointFilter"/> to a route handler.
/// </summary>
public static class ODataEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData metadata.
    /// It uses the Request.Header.ContentType or $format to identify whether it's CSDL-XML or CSDL-JSON.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataMetadata(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model)
        => endpoints.MapODataMetadata(pattern, model, new ODataMetadataHandler());

    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData metadata based on <see cref="IODataMetadataHandler"/>.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataMetadata(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model,
        IODataMetadataHandler metadataHandler)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(metadataHandler);

        // Let's focus on the metadata CSDL only now.
        return endpoints.MapGet(pattern, async httpContext => await metadataHandler.InvokeAsync(httpContext, model));
    }

    // It's better to add ODataQueryFilter as early as possible,
    // So, we can do the validation as early as possible,
    // but will do the applyTo as later as possbile.
    public static TBuilder AddODataQueryEndpointFilter<TBuilder>(this TBuilder builder, IODataQueryEndpointFilter queryFilter) where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(queryFilter);

    public static TBuilder AddODataQueryEndpointFilter<TBuilder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        where TFilterType : IODataQueryEndpointFilter =>
        builder.AddEndpointFilter<TBuilder, TFilterType>();

    /// <summary>
    /// Registers the default OData query filter onto the route handler.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="defaultConfigSetup">The action to configure default  settings.</param>
    /// <param name="validationSetup">The action to configure validataion settings.</param>
    /// <param name="querySetup">The action to configure query settings.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder,
        Action<DefaultQueryConfigurations> defaultConfigSetup = default,
        Action<ODataValidationSettings> validationSetup = default,
        Action<ODataQuerySettings> querySetup = default)
        => builder.AddODataQueryEndpointFilterInternal(new ODataQueryEndpointFilter(), defaultConfigSetup, validationSetup, querySetup);

    /// <summary>
    /// Registers the default OData query filter onto the route group.
    /// </summary>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <param name="validationSetup">The action to configure validataion settings.</param>
    /// <param name="querySetup">The action to configure query settings.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteGroupBuilder AddODataQueryEndpointFilter(this RouteGroupBuilder builder,
        Action<DefaultQueryConfigurations> defaultConfigSetup = default,
        Action<ODataValidationSettings> validationSetup = default,
        Action<ODataQuerySettings> querySetup = default) =>
        builder.AddODataQueryEndpointFilterInternal(new ODataQueryEndpointFilter(), defaultConfigSetup, validationSetup, querySetup);

    private static TBuilder AddODataQueryEndpointFilterInternal<TBuilder>(this TBuilder builder, ODataQueryEndpointFilter queryFilter,
        Action<DefaultQueryConfigurations> defaultConfigSetup,
        Action<ODataValidationSettings> validationSetup,
        Action<ODataQuerySettings> querySetup)
        where TBuilder : IEndpointConventionBuilder
    {
        validationSetup?.Invoke(queryFilter.ValidationSettings);
        querySetup?.Invoke(queryFilter.QuerySettings);
        builder.AddEndpointFilter(queryFilter);
        return builder;
    }

    /// <summary>
    /// Registers an OData query filter onto the route handler.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="queryFilter">The <see cref="IODataQueryEndpointFilter"/>.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder, IODataQueryEndpointFilter queryFilter)=>
        builder.AddEndpointFilter(queryFilter);

    /// <summary>
    /// Registers an OData query filter onto the route group.
    /// </summary>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <param name="queryFilter">The <see cref="IODataQueryEndpointFilter"/>.</param>
    /// <returns>A <see cref="RouteGroupBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteGroupBuilder AddODataQueryEndpointFilter(this RouteGroupBuilder builder, IODataQueryEndpointFilter queryFilter) =>
        builder.AddEndpointFilter(queryFilter);

    /// <summary>
    /// Registers an OData query filter of type <typeparamref name="TFilterType"/> onto the route handler.
    /// </summary>
    /// <typeparam name="TFilterType">The type of the <see cref="IODataQueryEndpointFilter"/> to register.</typeparam>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteHandlerBuilder AddODataQueryEndpointFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this RouteHandlerBuilder builder)
        where TFilterType : IODataQueryEndpointFilter =>
        builder.AddEndpointFilter<TFilterType>();

    /// <summary>
    /// Registers an OData query filter of type <typeparamref name="TFilterType"/> onto the route group.
    /// </summary>
    /// <typeparam name="TFilterType">The type of the <see cref="IODataQueryEndpointFilter"/> to register.</typeparam>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <returns>A <see cref="RouteGroupBuilder"/> that can be used to further customize the route handler.</returns>
    public static RouteGroupBuilder AddODataQueryEndpointFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this RouteGroupBuilder builder)
        where TFilterType : IODataQueryEndpointFilter =>
        builder.AddEndpointFilter<TFilterType>();


    public static TBuilder AddODataQueryEndpointFilterFactory<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddODataQueryEndpointFilterFactory(new ODataQueryEndpointFilter());
    }

    public static TBuilder AddODataQueryEndpointFilterFactory<TBuilder>(this TBuilder builder, IODataQueryEndpointFilter queryFilter) where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddEndpointFilterFactory((filterFactoryContext, next) =>
        {
            MethodInfo methodInfo = filterFactoryContext.MethodInfo;

            return async invocationContext =>
            {
                ILoggerFactory loggerFactory = invocationContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger("ODataQuery");
                logger.LogInformation("Starting OdataQuery. ..");

                var odataFilterContext = new ODataQueryFilterInvocationContext { MethodInfo = methodInfo, InvocationContext = invocationContext };

                await queryFilter.OnFilterExecutingAsync(odataFilterContext);

                var result = await next(invocationContext);

                logger.LogInformation("Ending OdataQuery. ..");

                return await queryFilter.OnFilterExecutedAsync(result, odataFilterContext);
            };
        });
    }

    /// <summary>
    /// Adds an OData Edm model metadata to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// This method typically is used in Minimal API scenarios.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="model">The Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithModel<TBuilder>(this TBuilder builder, IEdmModel model) where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new EdmModelMetadata(model));
}
