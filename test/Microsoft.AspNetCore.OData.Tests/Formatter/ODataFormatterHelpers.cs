//-----------------------------------------------------------------------------
// <copyright file="ODataFormatterHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Formatter;

/// <summary>
/// A factory for creating <see cref="IODataSerializerProvider"/>.
/// </summary>
public static class ODataFormatterHelpers
{
    private static IServiceProvider _serviceProvider = BuildServiceProvider();

    internal static ODataOutputFormatter GetOutputFormatter(ODataPayloadKind[] payload, string mediaType = null)
    {
        // request is not needed on AspNetCore.
        ODataOutputFormatter formatter;
        formatter = new ODataOutputFormatter(payload);
        formatter.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        formatter.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

        if (mediaType != null)
        {
            formatter.SupportedMediaTypes.Add(mediaType);
        }
        else
        {
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
            formatter.SupportedMediaTypes.Add(ODataMediaTypes.ApplicationXml);
        }

        return formatter;
    }

    internal static ObjectResult GetContent<T>(T content, ODataOutputFormatter formatter, string mediaType)
    {
        ObjectResult objectResult = new ObjectResult(content);
        objectResult.Formatters.Add(formatter);
        objectResult.ContentTypes.Add(mediaType);

        return objectResult;
    }

    internal static async Task<string> GetContentResult(ObjectResult content, HttpRequest request)
    {
        var objectType = content.DeclaredType;
        if (objectType == null || objectType == typeof(object))
        {
            objectType = content.Value?.GetType();
        }

        MemoryStream ms = new MemoryStream();
        request.HttpContext.Response.Body = ms;

        var formatterContext = new OutputFormatterWriteContext(
            request.HttpContext,
            CreateWriter,
            objectType,
            content.Value);

        await content.Formatters[0].WriteAsync(formatterContext);

        ms.Flush();
        ms.Position = 0;
        StreamReader reader = new StreamReader(request.HttpContext.Response.Body);
        return reader.ReadToEnd();
    }

    private static TextWriter CreateWriter(Stream stream, Encoding encoding)
    {
        const int DefaultBufferSize = 16 * 1024;
        return new HttpResponseStreamWriter(stream, encoding, DefaultBufferSize);
    }

    internal static IHeaderDictionary GetContentHeaders(string contentType = null)
    {
        IHeaderDictionary headers = RequestFactory.Create().Headers;
        if (!string.IsNullOrEmpty(contentType))
        {
            headers["Content-Type"] = contentType;
        }

        return headers;
    }

    /// <summary>
    /// Gets an <see cref="IODataSerializerProvider"/>.
    /// </summary>
    /// <returns>An ODataSerializerProvider.</returns>
    public static IODataSerializerProvider GetSerializerProvider()
    {
        return _serviceProvider.GetRequiredService<IODataSerializerProvider>();
    }

    /// <summary>
    /// Gets an <see cref="IODataDeserializerProvider"/>.
    /// </summary>
    /// <returns>An ODataDeserializerProvider.</returns>
    public static IODataDeserializerProvider GetDeserializerProvider()
    {
        return _serviceProvider.GetRequiredService<IODataDeserializerProvider>();
    }

    public static ODataMessageWriter GetMockODataMessageWriter()
    {
        MockODataRequestMessage requestMessage = new MockODataRequestMessage();
        return new ODataMessageWriter(requestMessage);
    }

    public static ODataMessageReader GetMockODataMessageReader()
    {
        MockODataRequestMessage requestMessage = new MockODataRequestMessage();
        return new ODataMessageReader(requestMessage);
    }

    private static IServiceProvider BuildServiceProvider()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

        // Deserializers.
        services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

        services.AddSingleton<ODataResourceDeserializer>();
        services.AddSingleton<ODataEnumDeserializer>();
        services.AddSingleton<ODataPrimitiveDeserializer>();
        services.AddSingleton<ODataResourceSetDeserializer>();
        services.AddSingleton<ODataCollectionDeserializer>();
        services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
        services.AddSingleton<ODataActionPayloadDeserializer>();

        // Serializers.
        services.AddSingleton<IODataSerializerProvider, ODataSerializerProvider>();

        services.AddSingleton<ODataEnumSerializer>();
        services.AddSingleton<ODataPrimitiveSerializer>();
        //   services.AddSingleton<ODataDeltaResourceSetSerializer>();
        services.AddSingleton<ODataResourceSetSerializer>();
        services.AddSingleton<ODataCollectionSerializer>();
        services.AddSingleton<ODataResourceSerializer>();
        services.AddSingleton<ODataServiceDocumentSerializer>();
        services.AddSingleton<ODataEntityReferenceLinkSerializer>();
        services.AddSingleton<ODataEntityReferenceLinksSerializer>();
        services.AddSingleton<ODataErrorSerializer>();
        services.AddSingleton<ODataMetadataSerializer>();
        services.AddSingleton<ODataRawValueSerializer>();

        return services.BuildServiceProvider();
    }
}
