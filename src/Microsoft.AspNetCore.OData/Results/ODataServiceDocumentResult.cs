//-----------------------------------------------------------------------------
// <copyright file="ODataServiceDocumentResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Results;

/// <summary>
/// Defines a contract that represents the result of OData service document.
/// </summary>
internal class ODataServiceDocumentResult : IResult
{
    /// <summary>
    /// Gets the static instance since we don't need the instance of it.
    /// </summary>
    public static ODataServiceDocumentResult Instance = new ODataServiceDocumentResult();

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var endpoint = httpContext.GetEndpoint();
        ODataMiniMetadata metadata = endpoint.Metadata.GetMetadata<ODataMiniMetadata>();
        IServiceProvider sp = metadata.ServiceProvider;
        IEdmModel model = metadata.Model;

        ODataServiceDocument serviceDocument = model.GenerateServiceDocument();

        ODataServiceDocumentSerializer serializer = sp.GetService<ODataServiceDocumentSerializer>() ?? new ODataServiceDocumentSerializer();

        ODataMessageWriterSettings writerSettings = sp.GetService<ODataMessageWriterSettings>() ?? new ODataMessageWriterSettings();
        writerSettings.BaseUri = GetBaseAddress(httpContext, metadata);

        writerSettings.ODataUri = new ODataUri
        {
            ServiceRoot = writerSettings.BaseUri,
        };

        HttpResponse response = httpContext.Response;

        ODataVersion version = ODataResult.GetODataVersion(httpContext.Request, metadata);
        httpContext.Response.ContentType = "application/json";

        // Add version header.
        httpContext.Response.Headers["OData-Version"] = ODataUtils.ODataVersionToString(version);

        IODataResponseMessageAsync responseMessage = ODataMessageWrapperHelper.Create(response.Body, response.Headers, sp);

        await using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
        {
            ODataSerializerContext writeContext = new ODataSerializerContext();
            await serializer.WriteObjectAsync(serviceDocument, typeof(ODataServiceDocument), messageWriter, writeContext).ConfigureAwait(false);
        }
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

