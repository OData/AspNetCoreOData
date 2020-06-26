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
    public class ValueSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="previousType"></param>
        public ValueSegmentTemplate(IEdmType previousType)
        {
            ValueType = previousType;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template => "$value";

        /// <summary>
        /// 
        /// </summary>
        public IEdmType ValueType { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new ValueSegment(ValueType);
        }
    }
}
