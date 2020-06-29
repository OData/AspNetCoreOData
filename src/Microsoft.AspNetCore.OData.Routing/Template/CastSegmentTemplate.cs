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
    /// Represents a template that could match a type cast segment.
    /// </summary>
    public class CastSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastSegmentTemplate" /> class.
        /// </summary>
        /// <param name="castType">The actual structured type.</param>
        public CastSegmentTemplate(IEdmStructuredType castType)
        {
            CastType = castType ?? throw new ArgumentNullException(nameof(castType));
        }

        /// <inheritdoc />
        public override string Template => CastType.FullTypeName();

        /// <summary>
        /// Gets the actual structured type.
        /// </summary>
        public IEdmStructuredType CastType { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new TypeSegment(CastType, previous);
        }
    }
}
