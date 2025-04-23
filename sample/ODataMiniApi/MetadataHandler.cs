//-----------------------------------------------------------------------------
// <copyright file="CustomizedMetadataHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataMiniApi;


[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
public class ODataModelConfigurationAttribute : Attribute, IODataModelConfiguration
{
    public ODataModelBuilder Apply(HttpContext context, ODataModelBuilder builder, Type clrType)
    {
        if (clrType == typeof(Customer))
        {
            builder.AddComplexType(typeof(Info));
        }

        return builder;
    }
}

/*
public class CustomizedMetadataHandler : ODataMetadataHandler
{
    protected override async ValueTask WriteAsJsonAsync(HttpContext context, IEdmModel model)
    {
        context.Response.ContentType = "application/json";

        JsonWriterOptions options = new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false,
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

    protected override async ValueTask WriteAsXmlAsync(HttpContext context, IEdmModel model)
    {
        context.Response.ContentType = "application/xml";

        using (StringWriter sw = new StringWriter())
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = false;

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
}
*/