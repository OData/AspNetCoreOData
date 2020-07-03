// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $count segment.
    /// </summary>
    public class CountSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the static instance of <see cref="CountSegmentTemplate"/>
        /// </summary>
        public static CountSegmentTemplate Instance { get; } = new CountSegmentTemplate();

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSegmentTemplate" /> class.
        /// </summary>
        private CountSegmentTemplate()
        {
        }

        /// <inheritdoc />
        public override string Literal => "$count";

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Count;

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return CountSegment.Instance;
        }
    }
}
