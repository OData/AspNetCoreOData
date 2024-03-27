//-----------------------------------------------------------------------------
// <copyright file="ODataPrefixMetadata.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Defines a contract use to specify the OData prefix metadata in <see cref="Endpoint.Metadata"/>.
    /// </summary>
    public sealed class ODataPrefixMetadata
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="ODataPrefixMetadata"/> class.
        /// </summary>
        /// <param name="prefix">The route component prefix</param>
        public ODataPrefixMetadata(string prefix)
        {
            Prefix = prefix ?? string.Empty;
        }

        /// <summary>
        /// Gets the route component prefix.
        /// </summary>
        public string Prefix { get; }
    }
}
