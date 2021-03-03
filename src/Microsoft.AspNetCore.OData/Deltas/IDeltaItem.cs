// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// The delta item base.
    /// </summary>
    public interface IDeltaItem
    {
        /// <summary>
        /// Gets the delta item kind.
        /// </summary>
        DeltaKind Kind { get; }
    }
}