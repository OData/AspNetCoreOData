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

            // TODO:
            IsSingle = castType.TypeKind != EdmTypeKind.Collection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="castType"></param>
        /// <param name="expectedType"></param>
        /// <param name="navigationSource"></param>
        public CastSegmentTemplate(IEdmType castType, IEdmType expectedType, IEdmNavigationSource navigationSource)
        {
            EdmType = castType ?? throw new ArgumentNullException(nameof(castType));
            ExpectedType = expectedType ?? throw new ArgumentNullException(nameof(expectedType));
            NavigationSource = navigationSource ?? throw new ArgumentNullException(nameof(navigationSource));

            // TODO:
            IsSingle = castType.TypeKind != EdmTypeKind.Collection;
        }

        /// <inheritdoc />
        public override string Literal => CastType.FullTypeName();

        /// <inheritdoc />
        public override IEdmType EdmType { get; }

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the expected type.
        /// </summary>
        public IEdmType ExpectedType { get; }

        /// <summary>
        /// Gets the actual structured type.
        /// </summary>
        public IEdmStructuredType CastType { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Cast;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new TypeSegment(CastType, previous);
        }
    }
}
