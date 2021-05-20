// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $ref segment.
    /// </summary>
    public class RefSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefSegmentTemplate" /> class.
        /// </summary>
        /// <param name="navigation">The Edm navigation property.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        public RefSegmentTemplate(IEdmNavigationProperty navigation, IEdmNavigationSource navigationSource)
        {
            Navigation = navigation ?? throw Error.ArgumentNull(nameof(navigation));

            NavigationSource = navigationSource;
        }

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationProperty Navigation { get; }

        /// <inheritdoc />
        public IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/$ref";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.RouteValues["navigationProperty"] = Navigation.Name;

            // ODL implementation is complex, here i just use the NavigationPropertyLinkSegment
            context.Segments.Add(new NavigationPropertyLinkSegment(Navigation, NavigationSource));
            return true;
        }
    }
}
