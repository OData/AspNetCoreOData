﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (context.RouteValues.TryGetValue("property", out object value))
            {
                string rawValue = value as string;
                IEdmProperty edmProperty = StructuredType.FindProperty(rawValue);
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
