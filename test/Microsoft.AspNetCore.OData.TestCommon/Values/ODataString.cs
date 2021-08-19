//-----------------------------------------------------------------------------
// <copyright file="ODataString.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.TestCommon.Values
{
    /// <summary>
    /// A OData string value.
    /// </summary>
    public class ODataString : IODataValue
    {
        public ODataString(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
