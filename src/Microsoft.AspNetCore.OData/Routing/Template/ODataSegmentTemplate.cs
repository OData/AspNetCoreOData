// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Base class for OData segment template
    /// </summary>
    public abstract class ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the templates. It's case-insensitive template.
        /// It's used to build the routing template in conventional routing.
        /// It's not used in attribute routing.
        /// The template string should include the leading "/" if apply.
        /// </summary>
        /// <param name="options">The route options.</param>
        /// <returns>The built segment templates.</returns>
        public abstract IEnumerable<string> GetTemplates(ODataRouteOptions options);

        /// <summary>
        /// Translate the template into a real OData path segment <see cref="ODataPathSegment"/>
        /// </summary>
        /// <param name="context">The translate context.</param>
        /// <returns>True if translated. False if no.</returns>
        public abstract bool TryTranslate(ODataTemplateTranslateContext context);

        /// <summary>
        /// Gets the templates by default. It's case-insensitive template.
        /// It's used to build the routing template in conventional routing.
        /// It's not used in attribute routing.
        /// The template string should include the leading "/" if apply.
        /// </summary>
        /// <remarks>Used in Unit test.</remarks>
        /// <returns>The built segment templates.</returns>
        internal IEnumerable<string> GetTemplates()
        {
            return GetTemplates(null);
        }
    }
}
