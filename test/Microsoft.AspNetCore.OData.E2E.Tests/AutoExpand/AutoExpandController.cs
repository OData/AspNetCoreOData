//-----------------------------------------------------------------------------
// <copyright file="AutoExpandController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AutoExpand
{
    public class RootsController : ODataController
    {
        private static Root _root = new Root
        {
            Id = 99,
            Name = "Sam",
            E1s = new[]
            {
                new Expandable1
                {
                    Id = "E1S_1"
                },
                new Expandable1
                {
                    Id = "E1S_2"
                }
            },
            E2s = new[]
            {
                new Expandable2
                {
                    Id = "E2S_1",
                    E1s = new[]
                    {
                        new Expandable1 { Id = "E2S_1_E1S_1"},
                        new Expandable1 { Id = "E2S_1_E1S_2"}
                    },
                    E3s = new[]
                    {
                        new Expendables3
                        {
                            E1s = new[]
                            {
                                new Expandable1 {Id = "E2S_1_E3S_1"},
                                new Expandable1 {Id = "E2S_1_E3S_2"}
                            }
                        }
                    }
                },
                new Expandable2
                {
                    Id = "E2S_2",
                    E1s = new[]
                    {
                        new Expandable1 { Id = "E2S_2_E1S_1"},
                        new Expandable1 { Id = "E2S_2_E1S_2"}
                    },
                    E3s = new[]
                    {
                        new Expendables3
                        {
                            E1s = new[]
                            {
                                new Expandable1 {Id = "E2S_2_E3S_1"},
                                new Expandable1 {Id = "E2S_2_E3S_2"}
                            }
                        }
                    }
                }
            }
        };

        [HttpGet("/odata/root")]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_root);
        }
    }

    public class CustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(AutoExpandDataSource.Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            Customer c = AutoExpandDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find customer with key = {key}");
            }

            return Ok(c);
        }

        [EnableQuery]
        public IActionResult GetHomeAddress(int key)
        {
            Customer c = AutoExpandDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find customer with key = {key}");
            }

            return Ok(c.HomeAddress);
        }

        [EnableQuery]
        public IActionResult Post([FromBody] Customer customer)
        {
            return Created(customer);
        }

        [EnableQuery]
        public IActionResult Put(int key, [FromBody] Customer customer)
        {
            var existingCustomer = AutoExpandDataSource.Customers.FirstOrDefault(d => d.Id == key);

            if (existingCustomer == null)
            {
                return BadRequest();
            }

            return Updated(existingCustomer);
        }
    }

    public class PeopleController : ODataController
    {
        [EnableQuery(MaxExpansionDepth = 4)]
        public IQueryable<People> Get()
        {
            return AutoExpandDataSource.People.AsQueryable();
        }
    }

    public class NormalOrdersController : ODataController
    {
        [EnableQuery]
        public IQueryable<NormalOrder> Get()
        {
            return AutoExpandDataSource.NormalOrders.AsQueryable();
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            NormalOrder n = AutoExpandDataSource.NormalOrders.FirstOrDefault(c => c.Id == key);
            if (n == null)
            {
                return NotFound($"Cannot find NormalOrder with key = {key}");
            }

            return Ok(n);
        }
    }

    public class EnableQueryMenusController : ODataController
    {
        private static readonly List<Menu> menus = new List<Menu>
        {
            new Menu
            {
                Id = 1,
                Tabs = new List<Tab>
                {
                    new Tab
                    {
                        Id = 1,
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Id = 1,
                                Notes = new List<Note>
                                {
                                    new Note { Id = 1 }
                                }
                            }
                        }
                    }
                }
            }
        };

        [EnableQuery(MaxExpansionDepth = 4)]
        public ActionResult Get()
        {
            return Ok(menus);
        }
    }

    public class QueryOptionsOfTMenusController : ODataController
    {
        private static readonly List<Menu> menus = new List<Menu>
        {
            new Menu
            {
                Id = 1,
                Tabs = new List<Tab>
                {
                    new Tab
                    {
                        Id = 1,
                        Items = new List<Item>
                        {
                            new Item
                            {
                                Id = 1,
                                Notes = new List<Note>
                                {
                                    new Note { Id = 1 }
                                }
                            }
                        }
                    }
                }
            }
        };

        public ActionResult Get(ODataQueryOptions<Menu> queryOptions)
        {
            var validationSettings = new ODataValidationSettings
            {
                MaxExpansionDepth = 4
            };

            queryOptions.Validate(validationSettings);

            var result = queryOptions.ApplyTo(menus.AsQueryable());
            
            return Ok(result);
        }
    }
}
