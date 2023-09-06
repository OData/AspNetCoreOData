//-----------------------------------------------------------------------------
// <copyright file="RegressionsDbContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Regressions
{
    public class RegressionsDbContext : DbContext
    {
        public RegressionsDbContext(DbContextOptions<RegressionsDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<DataFile> DataFiles { get; set; }
    }
}