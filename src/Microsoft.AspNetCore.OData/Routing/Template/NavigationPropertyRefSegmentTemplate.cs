// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $ref segment.
    /// </summary>
    public class NavigationPropertyRefSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyRefSegmentTemplate" /> class.
        /// </summary>
        /// <param name="navigation">The Edm navigation property.</param>
        /// <param name="navigationSource">The Edm navigation property.</param>
        public NavigationPropertyRefSegmentTemplate(IEdmNavigationProperty navigation, IEdmNavigationSource navigationSource)
            : this(new NavigationPropertyLinkSegment(navigation, navigationSource))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyRefSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The navigation property link segment.</param>
        public NavigationPropertyRefSegmentTemplate(NavigationPropertyLinkSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));

            IsSingle = !Segment.NavigationProperty.Type.IsCollection();
        }

        /// <inheritdoc />
        public override string Literal => "$ref";

        /// <inheritdoc />
        public override IEdmType EdmType => Segment.EdmType;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Ref;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource => Segment.NavigationSource;

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationProperty Navigation => Segment.NavigationProperty;

        /// <summary>
        /// Gets the navigation property link segment.
        /// </summary>
        public NavigationPropertyLinkSegment Segment { get; }

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            // pay attention the usage of "NavigationPropertyLinkSegment" is different with the usage of ODL.
            return Segment;
        }
    }
}
