﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $count segment.
    /// </summary>
    public class CountSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Gets the static instance of <see cref="CountSegmentTemplate"/>
        /// </summary>
        public static CountSegmentTemplate Instance { get; } = new CountSegmentTemplate();

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSegmentTemplate" /> class.
        /// </summary>
        private CountSegmentTemplate()
        {
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/$count";
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            context.Segments.Add(CountSegment.Instance);
            return true;
        }
    }
}
