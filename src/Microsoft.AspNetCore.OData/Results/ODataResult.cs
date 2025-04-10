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

