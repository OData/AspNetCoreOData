// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="NavigationPropertyLinkSegment"/>.
    /// </summary>
    public class NavigationPropertyLinkSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyLinkSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The navigation property link segment</param>
        public NavigationPropertyLinkSegmentTemplate(NavigationPropertyLinkSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));
        }

        /// <inheritdoc />
        public override string Literal => Segment.Identifier;

        /// <inheritdoc />
        public override IEdmType EdmType => null;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.NavigationLink;

        /// <inheritdoc />
        public override bool IsSingle => false;

        /// <summary>
        /// Gets or sets the navigation property link segment.
        /// </summary>
        public NavigationPropertyLinkSegment Segment { get; private set; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            // TODO: maybe save the property name.
            // or create the PropertySegment using the information in the context.
            return Segment;
        }
    }
}
