//-----------------------------------------------------------------------------
// <copyright file="MetadataSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match "$metadata".
    /// </summary>
    public class MetadataSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the static instance of $metadata
        /// </summary>
        public static MetadataSegmentTemplate Instance { get; } = new MetadataSegmentTemplate();

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSegmentTemplate" /> class.
        /// </summary>
        private MetadataSegmentTemplate()
        {
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/$metadata";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.Segments.Add(MetadataSegment.Instance);
            return true;
        }
    }
}
