// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
            : this(new PropertySegment(property))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The property segment.</param>
        public PropertySegmentTemplate(PropertySegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));

            IsSingle = !segment.Property.Type.IsCollection();
        }

        /// <inheritdoc />
        public override string Literal => Property.Name;

        /// <inheritdoc />
        public override IEdmType EdmType => Property.Type.Definition;

        /// <summary>
        /// Gets the wrapped Edm property.
        /// </summary>
        public IEdmStructuralProperty Property => Segment.Property;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Property;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <summary>
        /// Gets the property segment.
        /// </summary>
        public PropertySegment Segment { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            return Segment;
        }
    }
}
