//-----------------------------------------------------------------------------
// <copyright file="ODataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Results;

/// <summary>
/// Defines an implementation that represents the result of an OData format result.
/// It's used for minimal API.
/// </summary>
internal class ODataResult : IODataResult, IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataResult"/> class.
    /// </summary>
    /// <param name="value">The wrappered real value.</param>
    public ODataResult(object value)
        : this(value, value?.GetType())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataResult"/> class.
    /// </summary>
    /// <param name="value">The wrappered real value.</param>
    /// <param name="expectedType">The expected type.</param>
    public ODataResult(object value, Type expectedType)
    {
        Value = value;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Gets the expected type.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
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
        WriteResponseHeaders(httpContext, metadata, version);

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

    private void WriteResponseHeaders(HttpContext context, ODataMiniMetadata metadata, ODataVersion version)
    {
        context.Response.Headers[HeaderNames.ContentType] = "application/json";
        context.Response.Headers["OData-Version"] = ODataUtils.ODataVersionToString(version);
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

