//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.OData.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="HttpContext"/>.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Return the <see cref="IODataFeature"/> from the <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance to extend.</param>
    /// <returns>The <see cref="IODataFeature"/>.</returns>
    public static IODataFeature ODataFeature(this HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        IODataFeature odataFeature = httpContext.Features.Get<IODataFeature>();
        if (odataFeature == null)
        {
            odataFeature = new ODataFeature();
            httpContext.Features.Set(odataFeature);
        }

        return odataFeature;
    }

    /// <summary>
    /// Return the <see cref="IODataBatchFeature"/> from the <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance to extend.</param>
    /// <returns>The <see cref="IODataBatchFeature"/>.</returns>
    public static IODataBatchFeature ODataBatchFeature(this HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        IODataBatchFeature odataBatchFeature = httpContext.Features.Get<IODataBatchFeature>();
        if (odataBatchFeature == null)
        {
            odataBatchFeature = new ODataBatchFeature();
            httpContext.Features.Set(odataBatchFeature);
        }

        return odataBatchFeature;
    }

    /// <summary>
    /// Returns the <see cref="ODataOptions"/> instance from the DI container.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> instance to extend.</param>
    /// <returns>The <see cref="ODataOptions"/> instance from the DI container.</returns>
    public static ODataOptions ODataOptions(this HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        return httpContext.RequestServices?.GetService<IOptions<ODataOptions>>()?.Value;
    }

    internal static IEdmModel GetOrCreateEdmModel(this HttpContext httpContext, Type clrType)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        // P1. Get model for the request if it's configured/cached, used it.
        IODataFeature odataFeature = httpContext.ODataFeature();
        IEdmModel model = odataFeature.Model;
        if (model is not null)
        {
            return model;
        }

        // P2. Retrieve it from metadata if 'WithODataModel' called.
        var endpoint = httpContext.GetEndpoint();
        var odataMiniMetadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        model = odataMiniMetadata?.Model;
        if (model is not null)
        {
            odataFeature.Model = model;
            return model;
        }

        // P3. Let's retrieve the model from the global cache
        IODataEndpointModelMapper endpointModelMapper = httpContext.RequestServices.GetService<IODataEndpointModelMapper>();
        if (odataMiniMetadata is null && endpointModelMapper is null)
        {
            throw new ODataException($"Please call 'AddOData()' or register to 'IODataEndpointModelMapper' service.");
        }

        model = endpointModelMapper?.GetModel(httpContext);
        if (model is not null)
        {
            odataFeature.Model = model;
            return model;
        }

        // 4.Ok, we don't have the model configured, let's build the model on the fly
        IAssemblyResolver resolver = httpContext.RequestServices.GetService<IAssemblyResolver>() ?? new DefaultAssemblyResolver();
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder(resolver, isQueryCompositionMode: true);

        EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(clrType);
        builder.AddEntitySet(clrType.Name, entityTypeConfiguration);

        // Do the model configuration if the configuration service is registered.
        var modelConfig = httpContext.RequestServices.GetService<IODataModelConfiguration>();
        if (modelConfig is not null)
        {
            modelConfig.Apply(httpContext, builder, clrType);
        }

        model = builder.GetEdmModel();

        // Add the model into the cache
        if (odataMiniMetadata is not null)
        {
            // make sure the 'ServiceProvider' is built after the model configuration.
            odataMiniMetadata.Model = model;
        }
        else
        {
            // if using metadata, don't catch it into global
            endpointModelMapper.RegisterModel(httpContext, model);
        }

        // Cached it into the ODataFeature()
        odataFeature.Model = model;
        return model;
    }

    internal static ODataPath GetOrCreateODataPath(this HttpContext httpContext, Type clrType)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        // 1. Get model for the request if it's configured/cached, used it.
        IODataFeature odataFeature = httpContext.ODataFeature();
        if (odataFeature.Path is not null)
        {
            return odataFeature.Path;
        }

        IEdmModel model = httpContext.GetOrCreateEdmModel(clrType);

        // 2. Retrieve it from metadata?
        var endpoint = httpContext.GetEndpoint();
        var odataMiniMetadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        var pathFactory = odataMiniMetadata?.PathFactory ?? ODataMiniMetadata.DefaultPathFactory;

        var path = pathFactory.Invoke(httpContext, clrType);
        odataFeature.Path = path;

        return path;
    }

    internal static IServiceProvider GetOrCreateServiceProvider(this HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        // 1. Get service provider for the request if it's configured/cached, used it.
        IODataFeature odataFeature = httpContext.ODataFeature();
        if (odataFeature.Services is not null)
        {
            return odataFeature.Services;
        }

        // 2. Retrieve it from metadata?
        var endpoint = httpContext.GetEndpoint();
        var odataMiniMetadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        if (odataMiniMetadata is not null)
        {
            odataFeature.Services = odataMiniMetadata.ServiceProvider;
            return odataFeature.Services;
        }

        // 3.
        IODataEndpointModelMapper endpointModelMapper = httpContext.RequestServices.GetService<IODataEndpointModelMapper>();
        if (endpointModelMapper is not null)
        {
            odataFeature.Services = endpointModelMapper.GetServiceProvider(httpContext);
        }

        return odataFeature.Services;
    }

    internal static IServiceProvider BuildDefaultServiceProvider(this HttpContext httpContext, IEdmModel model)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(model);

        ODataMiniOptions miniOptions = httpContext.RequestServices.GetService<IOptions<ODataMiniOptions>>()?.Value
            ?? new ODataMiniOptions();

        IServiceCollection services = new ServiceCollection();

        // Inject the core odata services.
        services.AddDefaultODataServices(miniOptions.Version);

        // Inject the default query configuration from this options.
        services.AddSingleton(sp => miniOptions.QueryConfigurations);

        // Inject the default Web API OData services.
        services.AddDefaultWebApiServices();

        // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
        services.AddSingleton<ODataUriResolver>(sp =>
            new UnqualifiedODataUriResolver
            {
                EnableCaseInsensitive = miniOptions.EnableCaseInsensitive, // by default to enable case insensitive
                EnableNoDollarQueryOptions = miniOptions.EnableNoDollarQueryOptions // retrieve it from global setting
            });

        // Inject the Edm model.
        // From Current ODL implement, such injection only be used in reader and writer if the input
        // model is null.
        // How about the model is null?
        services.AddSingleton(sp => model);

        return services.BuildServiceProvider();
    }
}

internal class DefaultODataEndpointModelMapper1 : IODataEndpointModelMapper1
{
    public ConcurrentDictionary<Endpoint, (IEdmModel, IServiceProvider)> Maps { get; } = new ConcurrentDictionary<Endpoint, (IEdmModel, IServiceProvider)>();
}

internal interface IODataEndpointModelMapper1
{
    /// <summary>
    /// Gets the map between <see cref="Endpoint"/> and <see cref="IEdmModel"/>
    /// </summary>
    ConcurrentDictionary<Endpoint, (IEdmModel, IServiceProvider)> Maps { get; }
}

internal class MapProviderWrapper
{
    public IEdmModel Model { get; init; }
    public IServiceProvider ServiceProvider { get; init; }
}

public interface IODataEndpointModelMapper
{
    /// <summary>
    /// Gets the map between <see cref="Endpoint"/> and <see cref="IEdmModel"/>
    /// To provide the 'HttpContext' to acess the global ServiceProvider
    /// </summary>
    void RegisterModel(HttpContext context, IEdmModel model);

    IEdmModel GetModel(HttpContext context);

    IServiceProvider GetServiceProvider(HttpContext context);
}

internal class DefaultODataEndpointModelMapper : IODataEndpointModelMapper
{
    private ConcurrentDictionary<Endpoint, MapProviderWrapper> Maps { get; } = new ConcurrentDictionary<Endpoint, MapProviderWrapper>();

    public void RegisterModel(HttpContext context, IEdmModel model)
    {
        Endpoint endpoint = context.GetEndpoint();

        ODataMiniOptions miniOptions = context.RequestServices.GetService<IOptions<ODataMiniOptions>>()?.Value
            ?? new ODataMiniOptions();

        IServiceProvider serviceProvider = BuildServiceProvider(model, miniOptions);

        Maps[endpoint] = new MapProviderWrapper { Model = model, ServiceProvider = serviceProvider };
    }

    public IEdmModel GetModel(HttpContext context)
    {
        Endpoint endpoint = context.GetEndpoint();

        if (Maps.TryGetValue(endpoint, out var model))
        {
            return model.Model;
        }

        return null;
    }

    public IServiceProvider GetServiceProvider(HttpContext context)
    {
        Endpoint endpoint = context.GetEndpoint();

        if (Maps.TryGetValue(endpoint, out var model))
        {
            return model.ServiceProvider;
        }

        return null;
    }

    internal static IServiceProvider BuildServiceProvider(IEdmModel model, ODataMiniOptions miniOptions, Action<IServiceCollection> setupConfig = null)
    {
        IServiceCollection services = new ServiceCollection();

        // Inject the core odata services.
        services.AddDefaultODataServices(miniOptions.Version);

        // Inject the default query configuration from this options.
        services.AddSingleton(sp => miniOptions.QueryConfigurations);

        // Inject the default Web API OData services.
        services.AddDefaultWebApiServices();

        // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
        services.AddSingleton<ODataUriResolver>(sp =>
            new UnqualifiedODataUriResolver
            {
                EnableCaseInsensitive = miniOptions.EnableCaseInsensitive, // by default to enable case insensitive
                EnableNoDollarQueryOptions = miniOptions.EnableNoDollarQueryOptions // retrieve it from global setting
            });

        // Inject the Edm model.
        // From Current ODL implement, such injection only be used in reader and writer if the input
        // model is null.
        // How about the model is null?
        services.AddSingleton(sp => model);

        // Inject the customized services.
        setupConfig?.Invoke(services);

        return services.BuildServiceProvider();
    }
}

