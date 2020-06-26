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
    public class PropertyCatchAllSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="declaredType"></param>
        public PropertyCatchAllSegmentTemplate(IEdmStructuredType declaredType)
        {
            StructuredType = declaredType;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template => "{property}";

        /// <summary>
        /// 
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
