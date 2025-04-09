//-----------------------------------------------------------------------------
// <copyright file="ODataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Results;

static class ODataResultsExtensions
{
    public static IResult OData(this IResultExtensions resultExtensions, object value)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new ODataResult(value);
    }
}

// Supports the inheritance scenarios
// for example: 
// The Action result type is IList<Customer>
// But the real value is IList<VipCustomer>?
internal class ODataResult<T> : ODataResult
{
    public ODataResult(object value) : base(value)
    {
    }
}

public interface IODataResult
{
    object Value { get; }
}

internal class ODataMetadataResult : IResult
{
    internal static ODataMetadataResult Instance = new ODataMetadataResult();

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        IEdmModel model = metadata.Model;

        IServiceProvider sp = GetServiceProvider(httpContext);
        ODataMetadataSerializer serializer = sp.GetService<ODataMetadataSerializer>() ?? new ODataMetadataSerializer();

        ODataMessageWriterSettings writerSettings = sp.GetService<ODataMessageWriterSettings>() ?? new ODataMessageWriterSettings();
        writerSettings.BaseUri = ODataOutputFormatter.GetDefaultBaseAddress(httpContext.Request);

        writerSettings.ODataUri = new ODataUri
        {
            ServiceRoot = writerSettings.BaseUri,
        };
        writerSettings.Version = metadata.Version;

        HttpResponse response = httpContext.Response;

        SetResponseHeader(httpContext, metadata);

        IODataResponseMessageAsync responseMessage = ODataMessageWrapperHelper.Create(response.Body, response.Headers, sp);

        await using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
        {
            ODataSerializerContext writeContext = new ODataSerializerContext();
            await serializer.WriteObjectAsync(model, typeof(IEdmModel), messageWriter, writeContext).ConfigureAwait(false);
        }
    }

    private IServiceProvider GetServiceProvider(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        return metadata.ServiceProvider;
    }

    private static void SetResponseHeader(HttpContext httpContext, ODataMiniMetadata metadata)
    {
        ODataVersion version = ODataResult.GetODataVersion(httpContext.Request, metadata);

        // Add version header.
        httpContext.Response.Headers["OData-Version"] = ODataUtils.ODataVersionToString(version);

        if (IsJson(httpContext))
        {
            httpContext.Response.ContentType = "application/json";
        }
        else
        {
            httpContext.Response.ContentType = "application/xml";
        }
    }

    internal static bool IsJson(HttpContext context)
    {
        var acceptHeaders = context.Request.Headers.Accept;
        if (acceptHeaders.Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase)))
        {
            // If Accept header set on Request, we use it.
            return true;
        }
        else if (acceptHeaders.Any(h => h.Contains("application/xml", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        StringValues formatValues;
        bool dollarFormat = context.Request.Query.TryGetValue("$format", out formatValues) || context.Request.Query.TryGetValue("format", out formatValues);
        if (dollarFormat)
        {
            if (formatValues.Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else if (formatValues.Any(h => h.Contains("application/xml", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return false;
    }
}

internal class ODataServiceDocumentResult : ODataResult
{
    public IEdmModel Model { get; set; }

    public ODataServiceDocumentResult(IEdmModel model) : base(model)
    {
        Model = model;
    }

    public override async Task ExecuteAsync(HttpContext httpContext)
    {
        ODataServiceDocument serviceDocument = Model.GenerateServiceDocument();

        IServiceProvider sp = GetServiceProvider(httpContext);
        ODataServiceDocumentSerializer serializer = sp.GetService<ODataServiceDocumentSerializer>() ?? new ODataServiceDocumentSerializer();

        ODataMessageWriterSettings writerSettings = sp.GetService<ODataMessageWriterSettings>() ?? new ODataMessageWriterSettings();
        writerSettings.BaseUri = ODataOutputFormatter.GetDefaultBaseAddress(httpContext.Request);

        writerSettings.ODataUri = new ODataUri
        {
            ServiceRoot = writerSettings.BaseUri,
        };

        HttpResponse response = httpContext.Response;
        IODataResponseMessageAsync responseMessage = ODataMessageWrapperHelper.Create(response.Body, response.Headers, sp);

        await using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, Model))
        {
            ODataSerializerContext writeContext = new ODataSerializerContext();
            await serializer.WriteObjectAsync(serviceDocument, typeof(ODataServiceDocument), messageWriter, writeContext).ConfigureAwait(false);
        }
    }

    private IServiceProvider GetServiceProvider(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        return metadata.ServiceProvider;
    }
}

/// <summary>
/// Defines an implementation that represents the result of an OData format result.
/// It's used for minimal API.
/// </summary>
internal class ODataResult : IResult, IODataResult, IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataResult"/> class
    /// </summary>
    /// <param name="value">The wrappered real value.</param>
    public ODataResult(object value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the wrapper value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/> and <see cref="MethodInfo"/>.
    /// </summary>
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ODataEndpointConventionBuilderExtensions.ConfigureODataMetadata(builder, null);
    }

    /// <summary>
    /// Write an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public virtual async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        Type type = Value.GetType();
        if (type == null)
        {
            throw Error.ArgumentNull(nameof(type));
        }
        type = TypeHelper.GetTaskInnerTypeOrSelf(type);

        if (!TypeHelper.IsCollection(type, out Type elementType))
        {
            elementType = type;
        }

        HttpRequest request = httpContext.Request;
        if (request == null)
        {
            throw Error.InvalidOperation(SRResources.WriteToResponseAsyncMustHaveRequest);
        }

        IEdmModel model = httpContext.GetOrCreateEdmModel(elementType);

        if (elementType.IsSelectExpandWrapper(out Type elementType1))
        {
            elementType = elementType1;
        }

        IEdmType edmType = model.GetEdmType(elementType);

        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();

        var pathFactory = metadata?.PathFactory ?? ODataMiniMetadata.DefaultPathFactory;

        IODataFeature odataFeature = httpContext.ODataFeature();
        odataFeature.Path = pathFactory(httpContext, elementType);

        HttpResponse response = httpContext.Response;
        Uri baseAddress = GetBaseAddress(httpContext, metadata);
        MediaTypeHeaderValue contentType = GetContentType(response.Headers[HeaderNames.ContentType].FirstOrDefault());

        if (odataFeature.Services is null)
        {
            odataFeature.Services = metadata.ServiceProvider;
        }

        IODataSerializerProvider serializerProvider = odataFeature.Services.GetRequiredService<IODataSerializerProvider>();

        ODataVersion version = GetODataVersion(request, metadata);

        // Add version header.
        response.Headers["OData-Version"] = ODataUtils.ODataVersionToString(version);

        await ODataOutputFormatterHelper.WriteToStreamAsync(
            type,
            Value,
            model,
            version,
            baseAddress,
            contentType,
            request,
            request.Headers,
            serializerProvider).ConfigureAwait(false);
    }

    private Uri GetBaseAddress(HttpContext httpContext, ODataMiniMetadata options)
    {
        if (options.BaseAddressFactory is not null)
        {
            return options.BaseAddressFactory(httpContext);
        }

        return ODataOutputFormatter.GetDefaultBaseAddress(httpContext.Request);
    }

    internal static ODataVersion GetODataVersion(HttpRequest request, ODataMiniMetadata options)
    {
        ODataVersion? version = request.ODataMaxServiceVersion() ??
            request.ODataMinServiceVersion() ??
            request.ODataServiceVersion();

        if (version is not null)
        {
            return version.Value;
        }

        if (options is not null)
        {
            return options.Version;
        }

        return ODataVersionConstraint.DefaultODataVersion;
    }

    private MediaTypeHeaderValue GetContentType(string contentTypeValue)
    {
        MediaTypeHeaderValue contentType = null;
        if (!string.IsNullOrEmpty(contentTypeValue))
        {
            MediaTypeHeaderValue.TryParse(contentTypeValue, out contentType);
        }

        return contentType;
    }
}

