//-----------------------------------------------------------------------------
// <copyright file="ODataObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
