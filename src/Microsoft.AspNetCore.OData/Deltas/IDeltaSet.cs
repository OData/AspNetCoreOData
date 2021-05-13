// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// The interface for a delta resource set.
    /// </summary>
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix.", Justification = "The set suffix is correct.")]
    public interface IDeltaSet : ICollection<IDeltaSetItem>
    {
    }
}