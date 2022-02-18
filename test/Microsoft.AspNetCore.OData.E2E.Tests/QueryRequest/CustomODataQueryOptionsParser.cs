//-----------------------------------------------------------------------------
// <copyright file="CustomODataQueryOptionsParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing.QueryRequest
{
    public class CustomODataQueryOptionsParser : IODataQueryRequestParser
    {
        private static MediaTypeHeaderValue SupportedMediaType = MediaTypeHeaderValue.Parse("text/xml");

        public bool CanParse(HttpRequest request)
        {
            return request.ContentType?.StartsWith(SupportedMediaType.MediaType, StringComparison.Ordinal) == true ? true : false;
        }

        public async Task<string> ParseAsync(HttpRequest request)
        {
            using (var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true))
            {
                var content = await reader.ReadToEndAsync();
                var document = XDocument.Parse(content);
                var queryOptions = document.Descendants("QueryOption").Select(d =>
                new
                {
                    Option = d.Attribute("Option").Value,
                    d.Attribute("Value").Value
                });

                return string.Join("&", queryOptions.Select(d => d.Option + "=" + d.Value));
            }
        }
    }
}
