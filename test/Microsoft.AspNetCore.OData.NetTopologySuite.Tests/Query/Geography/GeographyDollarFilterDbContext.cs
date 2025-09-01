//-----------------------------------------------------------------------------
// <copyright file="GeographyDollarFilterDbContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geography;

public class GeographyDollarFilterDbContext : DbContext
{
    public GeographyDollarFilterDbContext(DbContextOptions<GeographyDollarFilterDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Site>(e =>
        {
            e.Property(x => x.Location).HasColumnType("geography"); // Default is geography
            e.Property(x => x.Route).HasColumnType("geography"); // Default is geography
        });
    }

    public DbSet<Site> Sites { get; set; }
}
