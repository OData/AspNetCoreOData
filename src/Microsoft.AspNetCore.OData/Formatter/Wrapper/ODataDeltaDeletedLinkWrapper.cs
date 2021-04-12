// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataDeltaDeletedLink"/> deleted link.
    /// </summary>
    public sealed class ODataDeltaDeletedLinkWrapper : ODataDeltaLinkBaseWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaDeletedLinkWrapper"/>.
        /// </summary>
        /// <param name="deltaDeletedLink">The wrapped deleted link.</param>
        public ODataDeltaDeletedLinkWrapper(ODataDeltaDeletedLink deltaDeletedLink)
        {
            DeltaDeletedLink = deltaDeletedLink;
        }

        /// <summary>
        /// Gets the wrapped <see cref="ODataDeltaDeletedLink"/>.
        /// </summary>
        public ODataDeltaDeletedLink DeltaDeletedLink { get; }
    }
}
