//-----------------------------------------------------------------------------
// <copyright file="ODataMetadataHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// The default metadata handler used to output OData CSDL.
/// </summary>
public class ODataMetadataHandler : IODataMetadataHandler
{
    /// <summary>
    /// Implements the core logic associated with the filter given a <see cref="HttpContext"/> and the <see cref="IEdmModel"/>.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <param name="model">The Edm model.</param>
    /// <returns>An awaitable result of calling the handler.</returns>
    public async ValueTask InvokeAsync(HttpContext context, IEdmModel model)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(model);

        if (IsJson(context))
        {
            await WriteAsJsonAsync(context, model);
        }
        else
        {
            await WriteAsXmlAsync(context, model);
        }
    }

    protected virtual async ValueTask WriteAsJsonAsync(HttpContext context, IEdmModel model)
    {
        context.Response.ContentType = "application/json";

        JsonWriterOptions options = new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = true,
            SkipValidation = false
        };

        // We can't use response body directly since ODL writes the JSON CSDL using Synchronous operations.
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

    protected virtual async ValueTask WriteAsXmlAsync(HttpContext context, IEdmModel model)
    {
        context.Response.ContentType = "application/xml";

        // We can't use response body directly since ODL writes the XML CSDL using Synchronous operations.
        // Keep them for later reference.
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
}
