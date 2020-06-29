using ODataAuthorizationSample.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataAuthorizationSample.Controllers
{
    public class CustomersController : ODataController
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;

            if (_context.Customers.Count() == 0)
            {
                IList<Customer> customers = new List<Customer>
                {
                    new Customer
                    {
                        Name = "Jonier",
                        HomeAddress = new Address { City = "Redmond", Street = "156 AVE NE"},
                        FavoriteAddresses = new List<Address>
                        {
                            new Address { City = "Redmond", Street = "256 AVE NE"},
                            new Address { City = "Redd", Street = "56 AVE NE"},
                        },
                        Order = new Order { Title = "104m" },
                        Orders = Enumerable.Range(0, 2).Select(e => new Order { Title = "abc" + e }).ToList()
                    },
                    new Customer
                    {
                        Name = "Sam",
                        HomeAddress = new Address { City = "Bellevue", Street = "Main St NE"},
                        FavoriteAddresses = new List<Address>
                        {
                            new Address { City = "Red4ond", Street = "456 AVE NE"},
                            new Address { City = "Re4d", Street = "51 NE"},
                        },
                        Order = new Order { Title = "Zhang" },
                        Orders = Enumerable.Range(0, 2).Select(e => new Order { Title = "xyz" + e }).ToList()
                    },
                    new Customer
                    {
                        Name = "Peter",
                        HomeAddress = new Address {  City = "Hollewye", Street = "Main St NE"},
                        FavoriteAddresses = new List<Address>
                        {
                            new Address { City = "R4mond", Street = "546 NE"},
                            new Address { City = "R4d", Street = "546 AVE"},
                        },
                        Order = new Order { Title = "Jichan" },
                        Orders = Enumerable.Range(0, 2).Select(e => new Order { Title = "ijk" + e }).ToList()
                    },
                };

                foreach (var customer in customers)
                {
                    _context.Customers.Add(customer);
                    _context.Orders.Add(customer.Order);
                    _context.Orders.AddRange(customer.Orders);
                }

                _context.SaveChanges();
            }
        }

        [EnableQuery]
        public IActionResult Get()
        {
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return Ok(_context.Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_context.Customers.FirstOrDefault(c => c.Id == key));
        }

        /// <summary>
        /// If testing in IISExpress with the POST request to: http://localhost:2087/test/my/a/Customers
        /// Content-Type : application/json
        /// {
        ///    "Name": "Jonier","
        /// }
        /// 
        /// Check the reponse header, you can see 
        /// "Location" : "http://localhost:2087/test/my/a/Customers(0)"
        /// </summary>
        [EnableQuery]
        public IActionResult Post([FromBody]Customer customer)
        {
            return Created(customer);
        }

        public IActionResult Delete(int key)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == key);
            _context.Customers.Remove(customer);
            return Ok(customer);
        }

        [ODataRoute("GetTopCustomer")]
        public IActionResult GetTopCustomer()
        {
            return Ok(_context.Customers.FirstOrDefault());
        }
    }
}
