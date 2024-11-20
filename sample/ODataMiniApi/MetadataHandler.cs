//-----------------------------------------------------------------------------
// <copyright file="MetadataHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using System.Text.Encodings.Web;

namespace ODataMiniApi;

public class MetadataHandler
{
    public static async Task HandleMetadata(HttpContext context)
    {
        IEdmModel model = GetEdmModel(context);
        if (IsJson(context))
        {
            await WriteAsJson(context, model);
        }
        else
        {
            await WriteAsXml(context, model);
        }
    }

    internal static async Task WriteAsJson(HttpContext context, IEdmModel model)
    {
        context.Response.ContentType = "application/json";

        JsonWriterOptions options = new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = true,
            SkipValidation = false
        };

        // we can't use response body directly since ODL writes the JSON CSDL using Synchronous operations.
        using (MemoryStream memStream = new MemoryStream())
        {
            using (Utf8JsonWriter jsonWriter = new Utf8JsonWriter(memStream, options))
            {
                CsdlJsonWriterSettings settings = new CsdlJsonWriterSettings();
                settings.IsIeee754Compatible = true;
                IEnumerable<EdmError> errors;
                bool ok = CsdlWriter.TryWriteCsdl(model, jsonWriter, settings, out errors);
                jsonWriter.Flush();
            }

            memStream.Seek(0, SeekOrigin.Begin);
            string output = new StreamReader(memStream).ReadToEnd();
            await context.Response.WriteAsync(output).ConfigureAwait(false);
        }
    }

    internal static async Task WriteAsXml(HttpContext context, IEdmModel model)
    {
        context.Response.ContentType = "application/xml";

        // we can't use response body directly since ODL writes the XML CSDL using Synchronous operations.
        //XmlWriterSettings settings = new XmlWriterSettings();
        //settings.Encoding = Encoding.UTF8;
        //settings.Indent = true; // for better readability

        //using (XmlWriter xw = XmlWriter.Create(context.Response.Body, settings))
        //{
        //    IEnumerable<EdmError> errors;
        //    CsdlWriter.TryWriteCsdl(model, xw, CsdlTarget.OData, out errors);
        //    xw.Flush();
        //}

        //await Task.CompletedTask;

        using (StringWriter sw = new StringWriter())
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true; // for better readability

            using (XmlWriter xw = XmlWriter.Create(sw, settings))
            {
                IEnumerable<EdmError> errors;
                CsdlWriter.TryWriteCsdl(model, xw, CsdlTarget.OData, out errors);
                xw.Flush();
            }

            string output = sw.ToString();
            await context.Response.WriteAsync(output).ConfigureAwait(false);
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

    private static IEdmModel GetEdmModel(HttpContext context)
    {
        // You can retrieve/create the Edm model by yourself, or create and use the Model Provider service
        Endpoint endpoint = context.GetEndpoint();
        ODataPrefixMetadata prefixMetadata = endpoint.Metadata.GetMetadata<ODataPrefixMetadata>();
        if (prefixMetadata != null)
        {
            ODataOptions options = context.RequestServices.GetService<IOptions<ODataOptions>>()?.Value;
            if (options != null)
            {
                if (options.RouteComponents.TryGetValue(prefixMetadata.Prefix, out var routeComponents))
                {
                    return routeComponents.EdmModel;
                }
            }
        }

        throw new InvalidOperationException($"Please calling WithOData() to register the EdmModel.");
    }
}
