// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    public interface IODataPathParser
    {
        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="odataPath">The OData path template to parse.</param>
        /// <param name="requestProvider">The OData service provider.</param>
        /// <returns>A parsed representation of the template, or <c>null</c> if the template does not match the model.</returns>
        ODataPath Parse(IEdmModel model, Uri serviceRoot, Uri odataPath, IServiceProvider requestProvider);
    }

    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public class DefaultODataPathParser : IODataPathParser
    {
        /// <summary>
        /// Parse the string like "/users/{id}/contactFolders/{contactFolderId}/contacts"
        /// to segments
        /// </summary>
        /// <param name="model">the Edm model.</param>
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

            try
            {
                return uriParser.ParsePath();
            }
            catch (ODataException ex)
            {
                return null;
            }
        }
    }
}
