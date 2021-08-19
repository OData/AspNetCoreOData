//-----------------------------------------------------------------------------
// <copyright file="CastSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
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
        {
            if (castType == null)
            {
                throw Error.ArgumentNull(nameof(castType));
            }

            if (expectedType == null)
            {
                throw Error.ArgumentNull(nameof(expectedType));
            }

            TypeSegment = new TypeSegment(castType, expectedType, navigationSource);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CastSegmentTemplate" /> class.
        /// </summary>
        /// <param name="typeSegment">The type segment.</param>
        public CastSegmentTemplate(TypeSegment typeSegment)
        {
            TypeSegment = typeSegment ?? throw Error.ArgumentNull(nameof(typeSegment));
        }

        /// <summary>
        /// Gets the cast type.
        /// </summary>
        public IEdmType CastType => TypeSegment.EdmType;

        /// <summary>
        /// Gets the expected type.
        /// </summary>
        public IEdmType ExpectedType => TypeSegment.ExpectedType;

        /// <summary>
        /// Gets the expected type.
        /// </summary>
        public TypeSegment TypeSegment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{CastType.AsElementType().FullTypeName()}";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.Segments.Add(TypeSegment);
            return true;
        }
    }
}
