// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match $metadata.
    /// </summary>
    public class MetadataSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the static instace of $metadata
        /// </summary>
        public static MetadataSegmentTemplate Instance { get; } = new MetadataSegmentTemplate();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSegmentTemplate" /> class.
        /// </summary>
        private MetadataSegmentTemplate()
        { }

        /// <inheritdoc />
        public override string Literal => "$metadata";

        /// <inheritdoc />
        public override IEdmType EdmType => null;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource => null;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Metadata;

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            context?.Segments.Add(MetadataSegment.Instance);
            return true;
        }
    }
}
