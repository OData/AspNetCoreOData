//-----------------------------------------------------------------------------
// <copyright file="PropertyNameCaseSensitiveDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.PropertyNameCaseSensitive;

public class Bill
{
    public int ID { get; set; }

    public string Name { get; set; }

    public Frequency Frequency { get; set; }

    public Guid ContactGuid { get; set; }

    public Double Weight { get; set; }

    public Address HomeAddress { get; set; }

    public IList<Address> Addresses { get; set; }

    public BillDetail BillDetail { get; set; }

    public IList<BillDetail> Details { get; set; }
}

public enum Frequency
{
    Once,

    BiWeekly,

    Monthly,

    Yealy
}

public class Address
{
    public string Street { get; set; }

    public string City { get; set; }
}

public class BillDetail
{
    public int Id { get; set; }

    public string Title { get; set; }

    public IList<int> DimensionInCentimeter { get; set; }
}

public class DifferentPropertyCase
{
    public int Id { get; set; }
    public string Field { get; set; }
    public string FIELD { get; set; }
    public ICollection<NestedDifferentPropertyCase> Nested { get; set; }
}
public class NestedDifferentPropertyCase
{
    public int Id { get; set; }
    public string ID { get; set; }
}