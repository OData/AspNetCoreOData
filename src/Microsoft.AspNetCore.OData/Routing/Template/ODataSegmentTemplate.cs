// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
        /// Gets the segment URL template literal.
        /// </summary>
        public abstract string Literal { get; }

        /// <summary>
        /// Gets the segment kind.
        /// </summary>
        public abstract ODataSegmentKind Kind { get; }

        /// <summary>
        /// Gets the Edm type of this segment.
        /// </summary>
        public abstract IEdmType EdmType { get; }

        /// <summary>
        /// Gets the target Navigation source of this segment.
        /// </summary>
        public virtual IEdmNavigationSource NavigationSource => throw new NotSupportedException();

        /// <summary>
        /// Gets a value indicating whether the output value is single value or collection value of this segment.
        /// </summary>
        public abstract bool IsSingle { get; }

        /// <summary>
        /// Translate the template into a real OData path segment, <see cref="ODataPathSegment"/>
        /// </summary>
        /// <param name="context">The translate context, the translated in context.</param>
        /// <returns>True if translated. false if no.</returns>
        public abstract bool TryTranslate(ODataTemplateTranslateContext context);
    }
}
