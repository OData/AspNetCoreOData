// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $value segment.
    /// </summary>
    public class ValueSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSegmentTemplate" /> class.
        /// </summary>
        /// <param name="previousType">The value segment Edm type.</param>
        public ValueSegmentTemplate(IEdmType previousType)
        {
            ValueType = previousType;
        }

        /// <inheritdoc />
        public override string Template => "$value";

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <summary>
        /// Gets the value type.
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
