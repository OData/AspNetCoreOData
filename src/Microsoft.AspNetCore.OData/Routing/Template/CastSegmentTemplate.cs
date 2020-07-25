// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
        /// <param name="typeSegment"></param>
        public CastSegmentTemplate(TypeSegment typeSegment)
        {
            TypeSegment = typeSegment ?? throw new ArgumentNullException(nameof(typeSegment));

            Literal = typeSegment.EdmType.AsElementType().FullTypeName();

            IsSingle = typeSegment.EdmType.TypeKind != EdmTypeKind.Collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CastSegmentTemplate" /> class.
        /// </summary>
        /// <param name="castType">The cast Edm type.</param>
        /// <param name="expectedType">The expected Edm type.</param>
        /// <param name="navigationSource">The target navigation source. it could be null.</param>
        public CastSegmentTemplate(IEdmType castType, IEdmType expectedType, IEdmNavigationSource navigationSource)
            : this(new TypeSegment(castType, expectedType, navigationSource))
        {
            //EdmType = castType ?? throw new ArgumentNullException(nameof(castType));
            //ExpectedType = expectedType ?? throw new ArgumentNullException(nameof(expectedType));
            //NavigationSource = navigationSource;

            //if (castType.TypeKind != expectedType.TypeKind)
            //{
            //    throw new ODataException(string.Format(CultureInfo.CurrentCulture, SRResources.InputCastTypeKindNotMatch, castType.TypeKind, expectedType.TypeKind));
            //}

            //IsSingle = true;
            //if (expectedType.TypeKind == EdmTypeKind.Collection)
            //{
            //    IsSingle = false;
            //    ExpectedType = expectedType.AsElementType();
            //}

            //if (EdmHelpers.IsRelatedTo(EdmType, ExpectedType))
            //{
            //    throw new ODataException(string.Format(CultureInfo.CurrentCulture, SRResources.TypeMustBeRelated, EdmType.FullTypeName(), ExpectedType.FullTypeName()));
            //}

            //TypeSegment = new TypeSegment(castType, expectedType, navigationSource);

            //Literal = castType.AsElementType().FullTypeName();
        }

        internal TypeSegment TypeSegment { get; }

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

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataSegmentTemplateTranslateContext context)
        {
            return TypeSegment;
        }
    }
}
