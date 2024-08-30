//-----------------------------------------------------------------------------
// <copyright file="ODataBoolean.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.TestCommon.Values;

/// <summary>
/// A OData boolean value.
/// </summary>
public class ODataBoolean : IODataValue
{
    public static ODataBoolean True = new ODataBoolean(true);

    public static ODataBoolean False = new ODataBoolean(false);

    private ODataBoolean(bool value)
    {
        Value = value;
    }

    public bool Value { get; }
}
