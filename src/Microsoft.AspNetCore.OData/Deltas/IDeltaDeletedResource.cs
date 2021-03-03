// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using System;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="IDeltaDeletedResource" /> allows and tracks changes to a deleted resource.
    /// </summary>
    public interface IDeltaDeletedResource : IDelta
    {
        /// <inheritdoc />
        Uri Id { get; set; }

        /// <inheritdoc />
        DeltaDeletedEntryReason? Reason { get; set; }
    }
}