// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmEntitySet"/>.
    /// </summary>
    public class EntitySetSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetSegmentTemplate" /> class.
        /// </summary>
        /// <param name="entitySet">The Edm entity set.</param>
        public EntitySetSegmentTemplate(IEdmEntitySet entitySet)
            : this(new EntitySetSegment(entitySet))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The entity set segment.</param>
        public EntitySetSegmentTemplate(EntitySetSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));
        }

        /// <inheritdoc />
        public override string Literal => Segment.EntitySet.Name;

        /// <inheritdoc />
        public override IEdmType EdmType => Segment.EdmType;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource => Segment.EntitySet;

        /// <summary>
        /// Gets the wrapped entity set.
        /// </summary>
        public IEdmEntitySet EntitySet => Segment.EntitySet;

        /// <summary>
        /// Gets the entity set segment.
        /// </summary>
        public EntitySetSegment Segment { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.EntitySet;

        /// <inheritdoc />
        public override bool IsSingle => false;

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataSegmentTemplateTranslateContext context)
        {
            return Segment;
        }
    }
}
