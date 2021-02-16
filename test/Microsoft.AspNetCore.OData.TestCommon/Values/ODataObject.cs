// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// An OData object
    /// </summary>
    public class ODataObject : Dictionary<string, IODataValue>, IODataValue
    {
        public string ContextUri { get; set; }
    }
}
