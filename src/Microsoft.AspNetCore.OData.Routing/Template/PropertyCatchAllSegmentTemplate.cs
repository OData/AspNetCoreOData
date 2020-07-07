// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match '{property}' segment.
    /// </summary>
    public class PropertyCatchAllSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCatchAllSegmentTemplate" /> class.
        /// </summary>
        /// <param name="declaredType">The declared type.</param>
        public PropertyCatchAllSegmentTemplate(IEdmStructuredType declaredType)
        {
            StructuredType = declaredType;
        }

        /// <inheritdoc />
        public override string Literal => "{property}";

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Property;

        /// <inheritdoc />
        public override IEdmType EdmType => throw new NotSupportedException();

        /// <inheritdoc />
        public override bool IsSingle => throw new NotSupportedException();

        /// <summary>
        /// Gets the declared type for this property template.
        /// </summary>
        public IEdmStructuredType StructuredType { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            if (routeValue.TryGetValue("property", out object value))
            {
                string rawValue = value as string;
                IEdmProperty edmProperty = StructuredType.FindProperty(rawValue);
                if (edmProperty != null && edmProperty.PropertyKind == EdmPropertyKind.Structural)
                {
                    return new PropertySegment((IEdmStructuralProperty)edmProperty);
                }
            }

            return null;
        }
    }
}
