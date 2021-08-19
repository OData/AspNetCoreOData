//-----------------------------------------------------------------------------
// <copyright file="DynamicSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that can match a <see cref="DynamicPathSegment"/>.
    /// Be noted:
    /// a dynamic path segment is a real segment (not a template), its literal is dynamic property name.
    /// </summary>
    public class DynamicSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The open property segment</param>
        public DynamicSegmentTemplate(DynamicPathSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));
        }

        /// <summary>
        /// Gets or sets the open property segment.
        /// </summary>
        public DynamicPathSegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{Segment.Identifier}";
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
