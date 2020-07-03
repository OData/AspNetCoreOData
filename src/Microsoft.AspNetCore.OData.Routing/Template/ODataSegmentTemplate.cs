// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents an OData segment template that could match an <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public abstract class ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the segment URL literal.
        /// </summary>
        public abstract string Literal { get; }

        /// <summary>
        /// Gets the segment kind.
        /// </summary>
        public abstract ODataSegmentKind Kind { get; }

        /// <summary>
        /// Gets a value indicating whether the output value is single value or collection value of this segment.
        /// </summary>
        public abstract bool IsSingle { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="previous"></param>
        /// <param name="routeValue"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public abstract ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString);
    }
}
