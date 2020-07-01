// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// 
    /// </summary>
    [NonValidatingParameterBinding]
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
    public class ODataActionParameters : Dictionary<string, object>
    {
    }
}
