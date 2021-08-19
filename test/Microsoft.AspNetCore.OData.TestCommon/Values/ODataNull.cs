//-----------------------------------------------------------------------------
// <copyright file="ODataNull.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// A OData null value.
    /// </summary>
    public class ODataNull : IODataValue
    {
        public static ODataNull Null = new ODataNull();
    }
}
