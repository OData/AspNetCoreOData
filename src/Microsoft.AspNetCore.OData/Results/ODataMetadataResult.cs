//-----------------------------------------------------------------------------
// <copyright file="ODataMetadataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Results;

/// <summary>
/// Defines a contract that represents the result of OData metadata.
/// </summary>
internal class ODataMetadataResult : IResult
{
    /// <summary>
    /// Gets the static instance since we don't need the instance of it.
    /// </summary>
    public static ODataMetadataResult Instance = new ODataMetadataResult();

    /// <summary>
    /// Write an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        IEdmModel model = metadata.Model;

        IServiceProvider sp = GetServiceProvider(httpContext);
        ODataMetadataSerializer serializer = sp.GetService<ODataMetadataSerializer>() ?? new ODataMetadataSerializer();

        ODataMessageWriterSettings writerSettings = sp.GetService<ODataMessageWriterSettings>() ?? new ODataMessageWriterSettings();
        writerSettings.BaseUri = GetBaseAddress(httpContext, metadata);

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

    private Uri GetBaseAddress(HttpContext httpContext, ODataMiniMetadata metadata)
    {
        if (metadata.BaseAddressFactory is not null)
        {
            return metadata.BaseAddressFactory(httpContext);
        }

        return ODataOutputFormatter.GetDefaultBaseAddress(httpContext.Request);
    }
}

