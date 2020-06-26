// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public class PropertySegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="property">The .</param>
        public PropertySegmentTemplate(IEdmStructuralProperty property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        public PropertySegmentTemplate(string template)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template => Property.Name;

        /// <summary>
        /// 
        /// </summary>
        public IEdmStructuralProperty Property { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new PropertySegment(Property);
        }
    }
}
