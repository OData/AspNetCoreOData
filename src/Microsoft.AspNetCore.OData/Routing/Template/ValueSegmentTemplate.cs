// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $value segment.
    /// </summary>
    public class ValueSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSegmentTemplate" /> class.
        /// </summary>
        /// <param name="previousType">The value segment Edm type.</param>
        public ValueSegmentTemplate(IEdmType previousType)
            : this(new ValueSegment(previousType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The value segment.</param>
        public ValueSegmentTemplate(ValueSegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
        }

        /// <inheritdoc />
        public override string Literal => "$value";

        /// <inheritdoc />
        public override IEdmType EdmType => Segment.EdmType;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Value;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource => null;

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <summary>
        /// Gets the value segment.
        /// </summary>
        public ValueSegment Segment { get; }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            context?.Segments.Add(Segment);
            return true;
        }
    }
}
