//-----------------------------------------------------------------------------
// <copyright file="CaseSqliteTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute;

public class CaseSqliteTests
{
    [Fact]
    public void Case_FilterOrderByAndPaginationExecuteInSql()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        DbContextOptionsBuilder<CaseSqliteContext> optionsBuilder = new();
        DbContextOptions<CaseSqliteContext> options = optionsBuilder.UseSqlite(connection).Options;
        using CaseSqliteContext db = new(options);
        db.Database.EnsureCreated();
        db.Items.AddRange(
            new CaseSqliteItem { Id = 1, Name = "Delta", Fallback = "Zulu" },
            new CaseSqliteItem { Id = 2, Name = null, Fallback = "Bravo" },
            new CaseSqliteItem { Id = 3, Name = null, Fallback = "Charlie" },
            new CaseSqliteItem { Id = 4, Name = "Alpha", Fallback = "Delta" });
        db.SaveChanges();

        ODataConventionModelBuilder modelBuilder = new();
        modelBuilder.EntitySet<CaseSqliteItem>("Items").EntityType.Property(item => item.Fallback).IsRequired();
        IEdmModel model = modelBuilder.GetEdmModel();
        ODataQueryContext context = new(model, typeof(CaseSqliteItem), null);
        DefaultHttpContext httpContext = new();
        httpContext.Request.QueryString = new("?$compute=case(Name ne null:Name,true:Fallback) as displayName&$filter=displayName ne 'Delta'&$orderby=displayName&$skip=1&$top=2");
        ODataQueryOptions<CaseSqliteItem> queryOptions = new(context, httpContext.Request);
        IQueryable<CaseSqliteItem> query = (IQueryable<CaseSqliteItem>)queryOptions.ApplyTo(db.Items,
            new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.False });

        string sql = query.ToQueryString();
        Assert.Contains("CASE", sql);
        Assert.Contains("WHERE", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("LIMIT", sql);
        Assert.Contains("OFFSET", sql);
        Assert.Equal(new[] { 2, 3 }, query.ToArray().Select(item => item.Id));
    }
}

public class CaseSqliteContext(DbContextOptions<CaseSqliteContext> options) : DbContext(options)
{
    public DbSet<CaseSqliteItem> Items => Set<CaseSqliteItem>();
}

public class CaseSqliteItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Fallback { get; set; }
}
