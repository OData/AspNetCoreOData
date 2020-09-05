// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmNavigationProperty"/>.
    /// </summary>
    public class NavigationSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSegmentTemplate" /> class.
        /// </summary>
        /// <param name="navigation">The Edm navigation property.</param>
        public NavigationSegmentTemplate(IEdmNavigationProperty navigation)
            : this(new NavigationPropertySegment(navigation, null))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSegmentTemplate" /> class.
        /// </summary>
        /// <param name="navigation">The Edm navigation property.</param>
        /// <param name="targetNavigationSource">The target navigation source.</param>
        public NavigationSegmentTemplate(IEdmNavigationProperty navigation, IEdmNavigationSource targetNavigationSource)
            : this (new NavigationPropertySegment(navigation, targetNavigationSource))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The navigation property segment.</param>
        public NavigationSegmentTemplate(NavigationPropertySegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));

            IsSingle = !segment.NavigationProperty.Type.IsCollection();
        }

        /// <inheritdoc />
        public override string Literal => Navigation.Name;

        /// <inheritdoc />
        public override IEdmType EdmType => Navigation.Type.Definition;

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationProperty Navigation => Segment.NavigationProperty;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Navigation;

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationSource TargetNavigationSource => Segment.NavigationSource;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <summary>
        /// Gets the navigation property segment.
        /// </summary>
        public NavigationPropertySegment Segment { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            return Segment;
        }
    }
}
