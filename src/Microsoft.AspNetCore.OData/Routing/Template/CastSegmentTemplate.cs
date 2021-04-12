// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        /// <param name="castType">The cast Edm type.</param>
        /// <param name="expectedType">The expected Edm type.</param>
        /// <param name="navigationSource">The target navigation source. it could be null.</param>
        public CastSegmentTemplate(IEdmType castType, IEdmType expectedType, IEdmNavigationSource navigationSource)
            : this(BuildSegment(castType, expectedType, navigationSource))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CastSegmentTemplate" /> class.
        /// </summary>
        /// <param name="typeSegment">The type segment.</param>
        public CastSegmentTemplate(TypeSegment typeSegment)
        {
            TypeSegment = typeSegment ?? throw Error.ArgumentNull(nameof(typeSegment));

            Literal = typeSegment.EdmType.AsElementType().FullTypeName();

            IsSingle = typeSegment.EdmType.TypeKind != EdmTypeKind.Collection;
        }

        /// <inheritdoc />
        public override string Literal { get; }

        /// <inheritdoc />
        public override IEdmType EdmType => TypeSegment.EdmType;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource => TypeSegment.NavigationSource;

        /// <summary>
        /// Gets the expected type.
        /// </summary>
        public IEdmType ExpectedType => TypeSegment.ExpectedType;

        /// <summary>
        /// Gets the actual structured type.
        /// </summary>
        public IEdmStructuredType CastType { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Cast;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <summary>
        /// Gets the expected type.
        /// </summary>
        public TypeSegment TypeSegment { get; }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            context?.Segments.Add(TypeSegment);
            return true;
        }

        private static TypeSegment BuildSegment(IEdmType castType, IEdmType expectedType, IEdmNavigationSource navigationSource)
        {
            if (castType == null)
            {
                throw Error.ArgumentNull(nameof(castType));
            }

            if (expectedType == null)
            {
                throw Error.ArgumentNull(nameof(expectedType));
            }

            return new TypeSegment(castType, expectedType, navigationSource);
        }
    }
}
