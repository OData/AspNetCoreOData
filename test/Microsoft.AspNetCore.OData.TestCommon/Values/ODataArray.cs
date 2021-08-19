//-----------------------------------------------------------------------------
// <copyright file="ODataArray.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
