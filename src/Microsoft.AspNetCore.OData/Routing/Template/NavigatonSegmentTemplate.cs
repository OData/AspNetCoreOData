// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        {
            Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

            IsSingle = !navigation.Type.IsCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSegmentTemplate" /> class.
        /// </summary>
        /// <param name="navigation">The Edm navigation property.</param>
        /// <param name="targetNavigationSource">The target navigation source.</param>
        public NavigationSegmentTemplate(IEdmNavigationProperty navigation, IEdmNavigationSource targetNavigationSource)
        {
            Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            TargetNavigationSource = targetNavigationSource;

            IsSingle = !navigation.Type.IsCollection();
        }

        /// <inheritdoc />
        public override string Literal => Navigation.Name;

        /// <inheritdoc />
        public override IEdmType EdmType => Navigation.Type.Definition;

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationProperty Navigation { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Navigation;

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationSource TargetNavigationSource { get; }

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataSegmentTemplateTranslateContext context)
        {
            return new NavigationPropertySegment(Navigation, TargetNavigationSource);
        }
    }
}
