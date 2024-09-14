//-----------------------------------------------------------------------------
// <copyright file="OrderByDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataOrderByTest;

public abstract class OrderedItem
{
    public int ExpectedOrder { get; set; }
}

public class Item : OrderedItem
{
    [Key]
    [Column(Order = 2)]
    public int A { get; set; }

    [Key]
    [Column(Order = 1)]
    public int C { get; set; }

    [Key]
    [Column(Order = 3)]
    public int B { get; set; }
}

public class Item2 : OrderedItem
{
    [Key]
    [Column(Order = 3)]
    public string A { get; set; }

    [Key]
    [Column(Order = 1)]
    public string C { get; set; }

    [Key]
    [Column(Order = 2)]
    public int B { get; set; }
}

public class ItemWithEnum : OrderedItem
{
    [Key]
    [Column(Order = 3)]
    public SmallNumber A { get; set; }

    [Key]
    [Column(Order = 2)]
    public string B { get; set; }

    [Key]
    [Column(Order = 1)]
    public SmallNumber C { get; set; }
}

public class ItemWithoutColumn : OrderedItem
{
    [Key]
    public int C { get; set; }

    [Key]
    public int B { get; set; }

    [Key]
    public int A { get; set; }
}

public enum SmallNumber
{
    One,
    Two,
    Three,
    Four
}

#region Advanced $orderby

public class OrderByStudent
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset Birthday { get; set; }

    public IList<int> Grades { get; set; }

    public OrderByAddress Location { get; set; }
}

public class OrderByAddress
{
    public string City { get; set; }

    public string ZipCode { get; set; }
}
#endregion
