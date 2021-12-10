//-----------------------------------------------------------------------------
// <copyright file="DollarApplyController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    [Route("default")]
    [Route("custom")]
    public class InMemorySalesController : ODataController
    {
        private readonly DollarApplyDbContext db;

        public InMemorySalesController(DollarApplyDbContext db)
        {
            this.db = db;
            DollarApplyDbContextInitializer.SeedDatabase(this.db);
        }

        [EnableQuery]
        [Route("Sales")]
        public ActionResult<IQueryable<Sale>> GetInMemorySales()
        {
            return db.Sales;
        }
    }

    [Route("defaultsql")]
    [Route("customsql")]
    public class SqlSalesController : ODataController
    {
        private readonly DollarApplySqlDbContext db;

        public SqlSalesController(DollarApplySqlDbContext db)
        {
            this.db = db;
            DollarApplyDbContextInitializer.SeedDatabase(this.db);
        }

        [EnableQuery]
        [HttpGet("Sales")]
        public ActionResult<IQueryable<Sale>> GetSqlSales()
        {
            return db.Sales;
        }
    }

    [Route("default")]
    [Route("custom")]
    public class InMemoryProductsController : ODataController
    {
        private readonly DollarApplyDbContext db;

        public InMemoryProductsController(DollarApplyDbContext db)
        {
            this.db = db;
            DollarApplyDbContextInitializer.SeedDatabase(this.db);
        }

        [EnableQuery]
        [Route("Products")]
        public ActionResult<IQueryable<Product>> Get()
        {
            return db.Products;
        }
    }

    [Route("defaultsql")]
    [Route("customsql")]
    public class SqlProductsController : ODataController
    {
        private readonly DollarApplySqlDbContext db;

        public SqlProductsController(DollarApplySqlDbContext db)
        {
            this.db = db;
            DollarApplyDbContextInitializer.SeedDatabase(this.db);
        }

        [EnableQuery]
        [Route("Products")]
        public ActionResult<IQueryable<Product>> Get()
        {
            return db.Products;
        }
    }

    public class EmployeesController : ODataController
    {
        [EnableQuery]
        public ActionResult<IEnumerable<Employee>> Get()
        {
            return DataSource.Employees;
        }
    }
}
