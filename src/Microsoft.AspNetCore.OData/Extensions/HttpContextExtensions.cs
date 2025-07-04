//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

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

    internal static bool IsMinimalEndpoint(this HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        // Check if the endpoint is a minimal endpoint.
        var endpoint = httpContext.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<ODataMiniMetadata>() != null;
    }

    internal static IEdmModel GetOrCreateEdmModel(this HttpContext httpContext, Type clrType, ParameterInfo parameter = null)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        // P1. Get model from the request if it's configured/cached, used it.
        IODataFeature odataFeature = httpContext.ODataFeature();
        IEdmModel model = odataFeature.Model;
        if (model is not null)
        {
            return model;
        }

        // P2. Retrieve it from metadata if 'WithODataModel(model)' called/cached.
        var endpoint = httpContext.GetEndpoint();
        var odataMiniMetadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        model = odataMiniMetadata?.Model;
        if (model is not null)
        {
            odataFeature.Model = model;
            return model;
        }

        // 3.Ok, we don't have the model configured, let's build the model on the fly
        bool isQueryCompositionMode = parameter != null ? true : false;

        IAssemblyResolver resolver = httpContext.RequestServices.GetService<IAssemblyResolver>() ?? new DefaultAssemblyResolver();
        ODataModelBuilder builder = new ODataConventionModelBuilder(resolver, isQueryCompositionMode);

        EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(clrType);
        builder.AddEntitySet(clrType.Name, entityTypeConfiguration);

        // Do the model configuration if the configuration service is registered.
        // First, let's check the configuration on the parameter as attribute (provided using parameterInfo)
        var modelConfig = parameter?.GetCustomAttributes()
            .FirstOrDefault(c => c is IODataModelConfiguration) as IODataModelConfiguration;

        // Then, check the configuration on the globle
        modelConfig = modelConfig ?? httpContext.RequestServices.GetService<IODataModelConfiguration>();
        if (modelConfig is not null)
        {
            builder = modelConfig.Apply(httpContext, builder, clrType);
        }

        model = builder.GetEdmModel();

        // Add the model into the cache
        if (odataMiniMetadata is not null)
        {
            // make sure the 'ServiceProvider' is built after the model configuration.
            odataMiniMetadata.Model = model;
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

        return null;
    }

    internal static async ValueTask<T> BindODataParameterAsync<T>(this HttpContext httpContext, ParameterInfo parameter)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        if (metadata is null || metadata.Model is null || metadata.PathFactory is null)
        {
            throw new ODataException(SRResources.ODataMustBeSetOnMinimalAPIEndpoint);
        }

        Type parameterType = parameter.ParameterType;

        IEdmModel model = metadata.Model;

        IODataFeature oDataFeature = httpContext.ODataFeature();
        oDataFeature.Model = model;
        oDataFeature.Services = httpContext.GetOrCreateServiceProvider();
        oDataFeature.Path = metadata.PathFactory(httpContext, parameterType);
        HttpRequest request = httpContext.Request;
        IList<IDisposable> toDispose = new List<IDisposable>();
        Uri baseAddress = httpContext.GetInputBaseAddress(metadata);

        ODataVersion version = ODataResult.GetODataVersion(request, metadata);

        object result = null;
        try
        {
            result = await ODataInputFormatter.ReadFromStreamAsync(
                parameterType,
                defaultValue: null,
                baseAddress,
                version,
                request,
                toDispose).ConfigureAwait(false);

            foreach (IDisposable obj in toDispose)
            {
                obj.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw new ODataException(Error.Format(SRResources.BindParameterFailedOnMinimalAPIEndpoint, parameter.Name, ex.Message));
        }

        return result as T;
    }

    internal static Uri GetInputBaseAddress(this HttpContext httpContext, ODataMiniMetadata options)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        if (options.BaseAddressFactory is not null)
        {
            return options.BaseAddressFactory(httpContext);
        }
        else
        {
            return ODataInputFormatter.GetDefaultBaseAddress(httpContext.Request);
        }
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
