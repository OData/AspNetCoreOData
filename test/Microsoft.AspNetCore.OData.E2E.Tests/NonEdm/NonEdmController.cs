using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NonEdm
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ODataQueryOptions<Customer> options)
        {
            return Ok(options.ApplyTo(NonEdmDbContext.GetCustomers().AsQueryable()));
        }
    }

    public class NonEdmDbContext
    {
        private static IList<Customer> _customers;

        public static IList<Customer> GetCustomers()
        {
            if (_customers == null)
            {
                Generate();
            }
            return _customers;
        }

        private static void Generate()
        {
            _customers = Enumerable.Range(1, 5).Select(e =>
                new Customer
                {
                    Id = e,
                    Name = "Customer #" + e,
                    Gender = e%2 == 0 ? Gender.Female : Gender.Male,
                }).ToList();
        }
    }
}
