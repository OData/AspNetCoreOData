//-----------------------------------------------------------------------------
// <copyright file="DefaultODataPathTemplateParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Parser;

/// <summary>
/// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
/// </summary>
public class DefaultODataPathTemplateParser : IODataPathTemplateParser
{
    /// <summary>
    /// Parse the string like "/users/{id}/contactFolders/{contactFolderId}/contacts"
    /// to segments
    /// </summary>
    /// <param name="model">the Edm model.</param>
    /// <param name="odataPath">the OData path.</param>
    /// <param name="requestProvider">The service provider.</param>
    /// <returns>Null or <see cref="ODataPathTemplate"/>.</returns>
    public virtual ODataPathTemplate Parse(IEdmModel model, string odataPath, IServiceProvider requestProvider)
    {
        if (model == null || string.IsNullOrEmpty(odataPath))
        {
            return null;
        }

        ODataUriParser uriParser;
        if (requestProvider == null)
        {
            uriParser = new ODataUriParser(model, new Uri(odataPath, UriKind.Relative));
        }
        else
        {
            uriParser = new ODataUriParser(model, new Uri(odataPath, UriKind.Relative), requestProvider);
        }

        uriParser.EnableUriTemplateParsing = true;

        uriParser.Resolver = new UnqualifiedCallAndAlternateKeyResolver(model)
        {
            EnableCaseInsensitive = true
        };

        uriParser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash; // support key in parentheses and key as segment.

        ODataPath path = uriParser.ParsePath();

        // Templatify
        ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(model);
        path.WalkWith(handler);

        return new ODataPathTemplate(handler.Templates);
    }
}
