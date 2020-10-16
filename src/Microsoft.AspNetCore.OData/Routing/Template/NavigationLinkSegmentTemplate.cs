// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="NavigationPropertyLinkSegment"/>.
    /// </summary>
    public class NavigationLinkSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationLinkSegmentTemplate"/> class.
        /// </summary>
        /// <param name="navigationProperty">The navigation property this link or ref acts on</param>
        /// <param name="navigationSource">The navigation source of entities linked to. This can be null.</param>
        public NavigationLinkSegmentTemplate(IEdmNavigationProperty navigationProperty, IEdmNavigationSource navigationSource)
            : this(new NavigationPropertyLinkSegment(navigationProperty, navigationSource))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationLinkSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The navigation property link segment</param>
        public NavigationLinkSegmentTemplate(NavigationPropertyLinkSegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
        }

        /// <inheritdoc />
        public override string Literal => Segment.Identifier;

        /// <inheritdoc />
        public override IEdmType EdmType => Segment.EdmType;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.NavigationLink;

        /// <inheritdoc />
        public override bool IsSingle => false;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource => Segment.NavigationSource;

        /// <summary>
        /// Gets or sets the navigation property link segment.
        /// </summary>
        public NavigationPropertyLinkSegment Segment { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            // TODO: maybe save the property name.
            // or create the PropertySegment using the information in the context.
            return Segment;
        }
    }
}
