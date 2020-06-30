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
    /// Represents a template that could match an <see cref="IEdmStructuralProperty"/>.
    /// </summary>
    public class PropertySegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySegmentTemplate" /> class.
        /// </summary>
        /// <param name="property">The wrapped Edm property.</param>
        public PropertySegmentTemplate(IEdmStructuralProperty property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));

            IsSingle = !property.Type.IsCollection();
        }

        /// <inheritdoc />
        public override string Template => Property.Name;

        /// <summary>
        /// Gets the wrapped Edm property.
        /// </summary>
        public IEdmStructuralProperty Property { get; }

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new PropertySegment(Property);
        }
    }
}
