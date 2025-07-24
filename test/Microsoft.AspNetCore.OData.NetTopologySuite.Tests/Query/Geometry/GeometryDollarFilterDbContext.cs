//-----------------------------------------------------------------------------
// <copyright file="GeometryDollarFilterDbContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geometry;

public class GeometryDollarFilterDbContext : DbContext
{
    public GeometryDollarFilterDbContext(DbContextOptions<GeometryDollarFilterDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Plant>(e =>
        {
            e.Property(x => x.Location).HasColumnType("geometry"); // Default is geography
            e.Property(x => x.Route).HasColumnType("geometry"); // Default is geography
        });
    }

    public DbSet<Plant> Plants { get; set; }
}
