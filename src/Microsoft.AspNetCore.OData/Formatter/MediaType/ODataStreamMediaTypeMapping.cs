//-----------------------------------------------------------------------------
// <copyright file="ODataStreamMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// Media type mapping that associates requests with stream property.
    /// </summary>
    public class ODataStreamMediaTypeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataStreamMediaTypeMapping"/> class.
        /// </summary>
        public ODataStreamMediaTypeMapping()
            : base("application/octet-stream")
        {
        }

        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull(nameof(request));
            }

            return request.ODataFeature().Path.IsStreamPropertyPath() ? 1 : 0;
        }
    }
}
