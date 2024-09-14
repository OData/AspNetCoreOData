//-----------------------------------------------------------------------------
// <copyright file="ODataNumber.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.TestCommon.Values;

//public class ODataNumber<T> : IODataValue where T : struct
//{
//    public ODataNumber(T value)
//    {
//        Value = value;
//    }

//    public T Value { get; }
//}

/// <summary>
/// A OData number.
/// </summary>
public abstract class ODataNumber : IODataValue
{
}

/// <summary>
/// OData int32
/// </summary>
public class ODataInt : ODataNumber
{
    public ODataInt(int value)
    {
        Value = value;
    }

    public int Value { get; }
}

/// <summary>
/// OData int64
/// </summary>
public class ODataLong : ODataNumber
{
    public ODataLong(long value)
    {
        Value = value;
    }

    public long Value { get; }
}

/// <summary>
/// OData decimal
/// </summary>
public class ODataDecimal : ODataNumber
{
    public ODataDecimal(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }
}

/// <summary>
/// OData double
/// </summary>
public class ODataDouble : ODataNumber
{
    public ODataDouble(double value)
    {
        Value = value;
    }

    public double Value { get; }
}
