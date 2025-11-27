//-----------------------------------------------------------------------------
// <copyright file="DateOnlyAndTimeOnlyModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateOnlyAndTimeOnly;

public class DCustomer
{
    public int Id { get; set; }

    // non-nullable
    public DateTime DateTime { get; set; }
    public DateTimeOffset Offset { get; set; }
    public DateOnly DateOnly { get; set; }
    public TimeOnly TimeOnly { get; set; }

    // nullable
    public DateTime? NullableDateTime { get; set; }
    public DateTimeOffset? NullableOffset { get; set; }
    public DateOnly? NullableDateOnly { get; set; }
    public TimeOnly? NullableTimeOnly { get; set; }

    // Collection
    public IList<DateTime> DateTimes { get; set; }
    public IList<DateTimeOffset> Offsets { get; set; }
    public IList<DateOnly> DateOnlys { get; set; }
    public IList<TimeOnly> TimeOnlys { get; set; }

    // Collection of nullable
    public IList<DateTime?> NullableDateTimes { get; set; }
    public IList<DateTimeOffset?> NullableOffsets { get; set; }
    public IList<DateOnly?> NullableDateOnlys { get; set; }
    public IList<TimeOnly?> NullableTimeOnlys { get; set; }
}

public class EfCustomer
{
    public int Id { get; set; }

    // non-nullable
    [Column(TypeName = "datetime2")]
    public DateTime DateTime { get; set; }
    public DateTimeOffset Offset { get; set; }

    // nullable
    public DateTime? NullableDateTime { get; set; }
    public DateTimeOffset? NullableOffset { get; set; }
}

public class EfPerson
{
    public int Id { get; set; }

    [Column("Birthday", TypeName = "Date")]
    public DateTime? Birthday { get; set; }
}
