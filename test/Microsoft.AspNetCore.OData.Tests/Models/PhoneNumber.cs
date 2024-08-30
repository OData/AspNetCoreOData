//-----------------------------------------------------------------------------
// <copyright file="PhoneNumber.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Tests.Models;

public struct PhoneNumber
{
    public int CountryCode { get; set; }

    public int AreaCode { get; set; }

    public int Number { get; set; }

    public PhoneType PhoneType { get; set; }
}

public enum PhoneType
{
    HomePhone,
    CellPhone,
    WorkPhone,
    Fax
}
