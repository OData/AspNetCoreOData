// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateAndTimeOfDay
{
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
}
