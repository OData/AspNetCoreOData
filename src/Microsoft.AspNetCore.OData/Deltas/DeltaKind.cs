// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// The Kind of the object within the DeltaPayload used to distinguish between
    /// Resource/DeletedResource/DeltaDeletedLink/AddedLink.
    /// </summary>
    public enum DeltaKind
    {
        /// <summary>
        /// Corresponds to any Unknown item added.
        /// </summary>
        Unknown,

        /// <summary>
        /// Corresponds to ODataResource.
        /// </summary>
        DeltaResource,

        /// <summary>
        /// Corresponds to ODataDeletedResource.
        /// </summary>
        DeltaDeletedResource,

        /// <summary>
        /// Corresponds to ODataDeltaDeletedLink.
        /// </summary>
        DeltaDeletedLink,

        /// <summary>
        /// Corresponds to ODataDeltaLink.
        /// </summary>
        DeltaLink
    }
}