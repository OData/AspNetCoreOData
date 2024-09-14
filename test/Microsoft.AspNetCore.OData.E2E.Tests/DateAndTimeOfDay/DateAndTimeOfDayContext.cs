//-----------------------------------------------------------------------------
// <copyright file="DateAndTimeOfDayContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay;

public class DateAndTimeOfDayContext : DbContext
{
    public DateAndTimeOfDayContext(DbContextOptions<DateAndTimeOfDayContext> options)
        : base(options)
    {
    }

    public DbSet<EfCustomer> Customers { get; set; }
}

public class EdmDateWithEfContext : DbContext
{
    public EdmDateWithEfContext(DbContextOptions<EdmDateWithEfContext> options)
        : base(options)
    {
    }

    public DbSet<EfPerson> People { get; set; }
}
