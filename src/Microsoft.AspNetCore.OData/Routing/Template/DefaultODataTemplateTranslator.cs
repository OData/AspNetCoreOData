// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Default implemetation for <see cref="IODataTemplateTranslator"/>.
    /// </summary>
    internal class DefaultODataTemplateTranslator : IODataTemplateTranslator
    {
        /// <inheritdoc />
        public virtual ODataPath Translate(ODataPathTemplate path, ODataTemplateTranslateContext context)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // calculate every time
            IList<ODataPathSegment> segments = new List<ODataPathSegment>();
            foreach (var segment in path.Segments)
            {
                ODataPathSegment odataSegment = segment.Translate(context);
                if (odataSegment == null)
                {
                    return null;
                }

                segments.Add(odataSegment);
            }

            return new ODataPath(segments);
        }
    }
}
