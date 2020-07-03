// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        public RefSegmentTemplate(IEdmNavigationProperty navigation)
        {
            Navigation = navigation;

            IsSingle = !navigation.Type.IsCollection();
        }

        /// <inheritdoc />
        public override string Literal => "$ref";

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Ref;

        /// <summary>
        /// Gets the wrapped navigation property.
        /// </summary>
        public IEdmNavigationProperty Navigation { get; }

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            // ODL implementation is complex, here i just use the NavigationPropertyLinkSegment
            return new NavigationPropertyLinkSegment(Navigation, previous);
        }
    }
}
