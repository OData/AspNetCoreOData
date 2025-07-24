//-----------------------------------------------------------------------------
// <copyright file="GeographyDollarFilterController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geography;

public class SitesController : ODataController
{
    private readonly GeographyDollarFilterDbContext db;

    public SitesController(GeographyDollarFilterDbContext db)
    {
        this.db = db;
        this.SeedDatabase();
    }

    [EnableQuery]
    public ActionResult<IQueryable<Site>> Get()
    {
        return db.Sites;
    }

    #region Helper Methods

    private void SeedDatabase()
    {
        this.db.Database.EnsureCreated();

        if (!this.db.Sites.Any())
        {
            var geographyFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            // Open the connection manually
            var connection = this.db.Database.GetDbConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            this.db.Database.UseTransaction(transaction);

            try
            {
                this.db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Sites ON");

                this.db.Sites.AddRange(
                    new Site
                    {
                        Id = 1,
                        Location = geographyFactory.CreatePoint(new Coordinate(-122.123889, 47.669444)),
                        Route = geographyFactory.CreateLineString(
                        [
                            new Coordinate(-122.20, 47.65),
                            new Coordinate(-122.18, 47.66),
                            new Coordinate(-122.16, 47.67)
                        ])
                    },
                    new Site
                    {
                        Id = 2,
                        Location = geographyFactory.CreatePoint(new Coordinate(-122.335167, 47.608013)),
                        Route = geographyFactory.CreateLineString(
                        [
                            new Coordinate(-122.10, 47.60),
                            new Coordinate(-122.08, 47.62),
                            new Coordinate(-122.06, 47.62)
                        ])
                    }
                );

                this.db.SaveChanges();

                this.db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Sites OFF");

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
    }

    #endregion Helper Methods
}
