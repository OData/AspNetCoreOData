//-----------------------------------------------------------------------------
// <copyright file="GeometryDollarFilterController.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geometry;

public class PlantsController : ODataController
{
    private readonly GeometryDollarFilterDbContext db;

    public PlantsController(GeometryDollarFilterDbContext db)
    {
        this.db = db;
        this.SeedDatabase();
    }

    [EnableQuery]
    public ActionResult<IQueryable<Plant>> Get()
    {
        return db.Plants;
    }

    #region Helper Methods

    private void SeedDatabase()
    {

        this.db.Database.EnsureCreated();

        if (!this.db.Plants.Any())
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0);

            // Open the connection manually
            var connection = this.db.Database.GetDbConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            this.db.Database.UseTransaction(transaction);

            try
            {
                this.db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Plants ON");

                this.db.Plants.AddRange(
                    new Plant
                    {
                        Id = 1,
                        Location = geometryFactory.CreatePoint(new Coordinate(15, 72)),
                        Route = geometryFactory.CreateLineString(
                        [
                            new Coordinate(8, 66),
                            new Coordinate(22, 72),
                            new Coordinate(36, 76)
                        ])
                    },
                    new Plant
                    {
                        Id = 2,
                        Location = geometryFactory.CreatePoint(new Coordinate(46, 61)),
                        Route = geometryFactory.CreateLineString(
                        [
                            new Coordinate(65, 52),
                            new Coordinate(82, 56),
                            new Coordinate(90, 56)
                        ])
                    }
                );

                this.db.SaveChanges();

                this.db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Plants OFF");

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
