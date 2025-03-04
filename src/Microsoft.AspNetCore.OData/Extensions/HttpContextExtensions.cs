//-----------------------------------------------------------------------------
// <copyright file="HttpContextExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Linq;

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

    internal static IEdmModel GetEdmModel(this HttpContext httpContext, Type clrType)
    {
        if (httpContext == null)
        {
            throw Error.ArgumentNull(nameof(httpContext));
        }

        // 1. Get model for the request if it's configured/cached, used it.
        IODataFeature odataFeature = httpContext.ODataFeature();
        IEdmModel model = odataFeature.Model;
        if (model is not null)
        {
            return model;
        }

        // 2. Retrieve it from metadata?
        var endpoint = httpContext.GetEndpoint();
        IEdmModelMetadata modelMetadata = endpoint.Metadata.GetMetadata<IEdmModelMetadata>();
        if (modelMetadata is not null)
        {
            // Cached it into the ODataFeature()
            odataFeature.Model = modelMetadata.Model;
            return modelMetadata.Model;
        }

        // Ok, we don't have the model configured, let's build the model on the fly
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

        // Cached it into the ODataFeature()
        odataFeature.Model = model;
        return model;
    }
}
