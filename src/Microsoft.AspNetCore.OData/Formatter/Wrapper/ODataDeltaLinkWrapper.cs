// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeltaLink"/> added link.
    /// </summary>
    public sealed class ODataDeltaLinkWrapper : ODataDeltaLinkBaseWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaLinkWrapper"/>.
        /// </summary>
        /// <param name="deltaLink">The wrapped added link item.</param>
        public ODataDeltaLinkWrapper(ODataDeltaLink deltaLink)
        {
            DeltaLink = deltaLink;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataDeltaLink"/>.
        /// </summary>
        public ODataDeltaLink DeltaLink { get; }
    }
}
