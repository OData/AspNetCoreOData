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
        /// 
        /// </summary>
        public RefSegmentTemplate(IEdmNavigationProperty navigation)
        {
            Navigation = navigation;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template => "$ref";

        /// <summary>
        /// 
        /// </summary>
        public IEdmNavigationProperty Navigation { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            // ODL implementation is complex, here i just use the NavigationPropertyLinkSegment
            return new NavigationPropertyLinkSegment(Navigation, previous);
        }
    }
}
