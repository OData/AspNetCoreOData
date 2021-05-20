// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
        /// <param name="navigationProperty">The Edm navigation property.</param>
        /// <param name="navigationSource">The target navigation source.</param>
        public NavigationSegmentTemplate(IEdmNavigationProperty navigationProperty, IEdmNavigationSource navigationSource)
            : this(new NavigationPropertySegment(navigationProperty, navigationSource))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The navigation property segment.</param>
        public NavigationSegmentTemplate(NavigationPropertySegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
        }

        /// <summary>
        /// Gets the wrapped Edm navigation property.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty => Segment.NavigationProperty;

        /// <summary>
        /// Gets the navigation property segment.
        /// </summary>
        public NavigationPropertySegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{NavigationProperty.Name}";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.Segments.Add(Segment);
            return true;
        }
    }
}
