//-----------------------------------------------------------------------------
// <copyright file="IDeltaDeletedResource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
