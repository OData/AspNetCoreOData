//-----------------------------------------------------------------------------
// <copyright file="EnumModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.Tests.Models;

public class EnumModel
{
    public int Id { get; set; }

    public SimpleEnum Simple { get; set; }

    public SimpleEnum? SimpleNullable { get; set; }

    public LongEnum Long { get; set; }

    public ByteEnum Byte { get; set; }

    public SByteEnum SByte { get; set; }

    public ShortEnum Short { get; set; }

    public UShortEnum UShort { get; set; }

    public UIntEnum UInt { get; set; }

    public FlagsEnum Flag { get; set; }

    public FlagsEnum? FlagNullable { get; set; }
}

[Flags]
public enum FlagsEnum
{
    One = 0x1,

    Two = 0x2,

    Four = 0x4
}

public enum LongEnum : long
{
    FirstLong,

    SecondLong,

    ThirdLong,

    FourthLong
}

public enum ShortEnum : short
{
    FirstShort,

    SecondShort,

    ThirdShort
}

public enum ByteEnum : byte
{
    FirstByte,

    SecondByte,

    ThirdByte
}

public enum UIntEnum : uint
{
    FirstUInt,

    SecondUInt,

    ThirdUInt
}

public enum SByteEnum : sbyte
{
    FirstSByte,

    SecondSByte,

    ThirdSByte
}

public enum UShortEnum : ushort
{
    FirstUShort,

    SecondUShort,

    ThirdUShort
}
