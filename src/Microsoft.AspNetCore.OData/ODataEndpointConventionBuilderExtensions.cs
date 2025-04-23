//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointConventionBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// Extension methods for adding <see cref="IODataQueryEndpointFilter"/> to a route handler.
/// </summary>
public static class ODataEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="RouteEndpoint"/> to the <see cref="IEndpointRouteBuilder"/> that matches HTTP GET requests to get the OData service document.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="model">The related Edm model used to generate the service document.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapODataServiceDocument(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEdmModel model)
        => endpoints.MapGet(pattern, () => ODataServiceDocumentResult.Instance).WithODataModel(model);

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
        => endpoints.MapGet(pattern, () => ODataMetadataResult.Instance).WithODataModel(model);

    /// <summary>
    /// Registers the default OData query filter onto the route handler.
    /// </summary>
    /// <param name="builder">The <see cref="RouteHandlerBuilder"/>.</param>
    /// <param name="validationSetup">The action to configure validataion settings.</param>
    /// <param name="querySetup">The action to configure query settings.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    [SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>")]
    public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder,
        Action<ODataValidationSettings> validationSetup = default,
        Action<ODataQuerySettings> querySetup = default)
        => builder.AddODataQueryEndpointFilterInternal(new ODataQueryEndpointFilter(), validationSetup, querySetup);

    /// <summary>
    /// Registers the default OData query filter onto the route group.
    /// </summary>
    /// <param name="builder">The <see cref="RouteGroupBuilder"/>.</param>
    /// <param name="validationSetup">The action to configure validataion settings.</param>
    /// <param name="querySetup">The action to configure query settings.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the route handler.</returns>
    [SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "<Pending>")]
    public static RouteGroupBuilder AddODataQueryEndpointFilter(this RouteGroupBuilder builder,
        Action<ODataValidationSettings> validationSetup = default,
        Action<ODataQuerySettings> querySetup = default) =>
        builder.AddODataQueryEndpointFilterInternal(new ODataQueryEndpointFilter(), validationSetup, querySetup);

    private static TBuilder AddODataQueryEndpointFilterInternal<TBuilder>(this TBuilder builder, ODataQueryEndpointFilter queryFilter,
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

    /// <summary>
    /// Enables OData response annotation to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithODataResult<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            object result = await next(invocationContext);

            // If it's null or if it's already the ODataResult, simply do nothing
            if (result is null || result is ODataResult)
            {
                return result;
            }

            // Maybe we have a scenario like:
            // First, enable OData result in app.MapGroup(...).WithODataResult(),
            // Then, disable OData result for a certain Routehandler by cusomizing the metadaga
            var endpoint = invocationContext.HttpContext.GetEndpoint();
            ODataMiniMetadata odataMetadata = endpoint?.Metadata?.GetMetadata<ODataMiniMetadata>();
            if (odataMetadata is not null && odataMetadata.IsODataFormat)
            {
                // Now, Let's focus on the POCO data result.
                // We can figure out more types later, for example, TypedResult<T>, Results<T>, etc.
                return new ODataResult(result/*, odataMetadata*/);
            }

            return result;
        });

        builder.Add(b => ConfigureODataMetadata(b, m => m.IsODataFormat = true));
        return builder;
    }

    /// <summary>
    /// Enables OData options to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="setupAction">The OData minimal options setup.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithODataOptions<TBuilder>(this TBuilder builder, Action<ODataMiniOptions> setupAction) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        builder.Add(b => ConfigureODataMetadata(b, m => setupAction.Invoke(m.Options)));
        return builder;
    }

    ///// <summary>
    ///// Enables OData batch to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    ///// </summary>
    ///// <typeparam name="TBuilder"></typeparam>
    ///// <param name="builder"></param>
    ///// <param name="services"></param>
    ///// <returns></returns>
    //public static TBuilder WithODataBatch<TBuilder>(this TBuilder builder, ODataBatchHandler handler) where TBuilder : IEndpointConventionBuilder
    //{
    //   // builder.Add(b => ConfigureODataMetadata(b, m => m.Services = services));
    //    return builder;
    //}

    /// <summary>
    /// Customizes the services used for OData to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="services">The services.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithODataServices<TBuilder>(this TBuilder builder, Action<IServiceCollection> services) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        builder.Add(b => ConfigureODataMetadata(b, m => m.Services = services));

        //builder.Finally(b => { });
        return builder;
    }

    /// <summary>
    /// Adds the OData model to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="model">The Edm model.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithODataModel<TBuilder>(this TBuilder builder, IEdmModel model) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        builder.Add(b => ConfigureODataMetadata(b, m => m.Model = model));
        return builder;
    }

    /// <summary>
    /// Adds the base address factory to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="baseAddressFactory">The base address factory which is used to calculate the base address for OData payload.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithODataBaseAddressFactory<TBuilder>(this TBuilder builder, Func<HttpContext, Uri> baseAddressFactory) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(baseAddressFactory, nameof(baseAddressFactory));

        builder.Add(b => ConfigureODataMetadata(b, m => m.BaseAddressFactory = baseAddressFactory));
        return builder;
    }

    /// <summary>
    /// Configures the OData version to <see cref="Endpoint.Metadata" /> associated with the current endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="version">The OData version.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static TBuilder WithODataVersion<TBuilder>(this TBuilder builder, ODataVersion version) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(b => ConfigureODataMetadata(b, m => m.Version = version));
        return builder;
    }

    // Will update the odata metadata if existed.
    // Otherwise, will create a new metadata and insert into endpoint.
    internal static void ConfigureODataMetadata(EndpointBuilder endpointBuilder, Action<ODataMiniMetadata> setupAction)
    {
        var metadata = endpointBuilder.Metadata.OfType<ODataMiniMetadata>().FirstOrDefault();
        if (metadata is null)
        {
            metadata = new ODataMiniMetadata();

            // retrieve the global minimal API OData configuration
            ODataMiniOptions options = endpointBuilder.ApplicationServices.GetService<IOptions<ODataMiniOptions>>()?.Value;
            if (options is not null)
            {
                metadata.Options.UpdateFrom(options);
            }

            endpointBuilder.Metadata.Add(metadata);
        }

        setupAction?.Invoke(metadata);
    }
}
