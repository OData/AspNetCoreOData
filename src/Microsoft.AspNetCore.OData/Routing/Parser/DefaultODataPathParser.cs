// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    internal class DefaultODataPathParser : IODataPathParser
    {
        /// <summary>
        /// Parse the string like "/users/1/contactFolders/..."
        /// to segments
        /// </summary>
        /// <param name="model">the Edm model.</param>
        /// <param name="serviceRoot">The service root uri.</param>
        /// <param name="odataPath">the setting.</param>
        /// <param name="requestProvider">The service provider.</param>
        /// <returns>Null or <see cref="ODataPathTemplate"/>.</returns>
        public virtual ODataPath Parse(IEdmModel model, Uri serviceRoot, Uri odataPath, IServiceProvider requestProvider)
        {
            ODataUriParser uriParser;
            if (serviceRoot != null)
            {
                uriParser = new ODataUriParser(model, serviceRoot, odataPath, requestProvider);
            }
            else
            {
                uriParser = new ODataUriParser(model, odataPath, requestProvider);
            }

            uriParser.Resolver = new UnqualifiedODataUriResolver { EnableCaseInsensitive = true };
            uriParser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash; // support key in parenthese and key as segment.

            // The ParsePath throws OData exceptions if the odata path is not valid.
            // That's expected.
            return uriParser.ParsePath();
        }
    }
}
