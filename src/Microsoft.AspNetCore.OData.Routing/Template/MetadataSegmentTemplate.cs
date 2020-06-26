// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public class MetadataSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        public static MetadataSegmentTemplate Instance { get; } = new MetadataSegmentTemplate();

        /// <summary>
        /// 
        /// </summary>
        private MetadataSegmentTemplate()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template => "$metadata";

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return MetadataSegment.Instance;
        }
    }
}
