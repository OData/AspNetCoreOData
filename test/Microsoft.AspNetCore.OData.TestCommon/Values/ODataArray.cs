// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// A array of OData value.
    /// </summary>
    public class ODataArray : List<IODataValue>, IODataValue
    {
        public string ContextUri { get; set; }

        public string NextLink { get; set; }

        public int? TotalCount { get; set; }
    }
}
