//-----------------------------------------------------------------------------
// <copyright file="PropertyCatchAllSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

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
            StructuredType = declaredType ?? throw Error.ArgumentNull(nameof(declaredType));
        }

        /// <summary>
        /// Gets the declared type for this property template.
        /// </summary>
        public IEdmStructuredType StructuredType { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/{property}";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (context.RouteValues.TryGetValue("property", out object value))
            {
                string rawValue = value as string;
                IEdmProperty edmProperty = StructuredType.ResolveProperty(rawValue);
                if (edmProperty != null && edmProperty.PropertyKind == EdmPropertyKind.Structural)
                {
                    context.Segments.Add(new PropertySegment((IEdmStructuralProperty)edmProperty));
                    return true;
                }
            }

            return false;
        }
    }
}
