// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// Media type mapping that associates requests with $count.
    /// </summary>
    public class ODataCountMediaTypeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCountMediaTypeMapping"/> class.
        /// </summary>
        public ODataCountMediaTypeMapping()
            : base("text/plain")
        {
        }

        internal static bool IsCountRequest(ODataPath path)
        {
            return path != null && path.LastSegment is CountSegment;
        }

        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return IsCountRequest(request.ODataFeature().Path) ? 1 : 0;
        }
    }
}
