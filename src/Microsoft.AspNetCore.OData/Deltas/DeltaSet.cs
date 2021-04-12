// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// Represents a delta resource set.
    /// </summary>
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix.", Justification = "The set suffix is correct.")]
    public abstract class DeltaSet : Collection<IDeltaItem>
    {
    }
}