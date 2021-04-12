// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="DynamicPathSegment"/>.
    /// Be noted:
    /// a dynamic path segment is a real segment (not a template), its literal is dynamic property name.
    /// </summary>
    public class DynamicSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The open property segment</param>
        public DynamicSegmentTemplate(DynamicPathSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));
        }

        /// <inheritdoc />
        public override string Literal => Segment.Identifier;

        /// <inheritdoc />
        public override IEdmType EdmType => null;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Dynamic;

        /// <inheritdoc />
        public override bool IsSingle => false;

        /// <summary>
        /// Gets or sets the open property segment.
        /// </summary>
        public DynamicPathSegment Segment { get; }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            context?.Segments.Add(Segment);
            return true;
        }
    }
}
