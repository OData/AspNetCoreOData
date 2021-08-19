//-----------------------------------------------------------------------------
// <copyright file="ValueSegmentTemplate.cs" company=".NET Foundation">
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
    /// Represents a template that could match a "/$value" segment.
    /// </summary>
    public class ValueSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSegmentTemplate" /> class.
        /// </summary>
        /// <param name="previousType">The value segment Edm type.</param>
        public ValueSegmentTemplate(IEdmType previousType)
            : this(new ValueSegment(previousType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The value segment.</param>
        public ValueSegmentTemplate(ValueSegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
        }

        /// <summary>
        /// Gets the value segment.
        /// </summary>
        public ValueSegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/$value";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.Segments.Add(Segment);
            return true;
        }
    }
}
