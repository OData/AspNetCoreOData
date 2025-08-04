//-----------------------------------------------------------------------------
// <copyright file="IAsyncEnumerableController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IAsyncEnumerableTests;

public class CustomersController : ODataController
{
    private readonly IAsyncEnumerableContext _context;

    public CustomersController(IAsyncEnumerableContext context)
    {
        context.Database.EnsureCreated();
        _context = context;

        if (!_context.Customers.Any())
        {
            Generate();
        }
    }

    [EnableQuery]
    [HttpGet("v1/Customers")]
    public IAsyncEnumerable<Customer> CustomersData()
    {
        IAsyncEnumerable<Customer> customers = CreateCollectionAsync<Customer>();

        return customers;
    }

    [EnableQuery]
    [HttpGet("odata/Customers")]
    [ODataRouteComponent]
    public IAsyncEnumerable<Customer> Get()
    {
        return _context.Customers.AsAsyncEnumerable();
    }

    [EnableQuery]
    [HttpGet("v2/Customers")]
    public ActionResult<IAsyncEnumerable<Customer>> CustomersDataNew()
    {
        return Ok(_context.Customers.AsAsyncEnumerable());
    }

    [EnableQuery(PageSize = 2)]
    [HttpGet("v3/Customers")]
    public IActionResult SearchCustomersForV3Route([FromQuery] Variant variant = Variant.None)
    {
        var asyncEnumerable = _context.Customers.AsAsyncEnumerable();
        if (variant == Variant.Generic)
        {
            asyncEnumerable = GenericAsyncEnumerableWithDelay(asyncEnumerable, TimeSpan.FromSeconds(1));
        }
        else if (variant == Variant.Typed)
        {
            asyncEnumerable = TypedAsyncEnumerableWithDelay(asyncEnumerable, TimeSpan.FromSeconds(1));
        }

        return Ok(asyncEnumerable);
    }

    public async IAsyncEnumerable<Customer> CreateCollectionAsync<T>()
    {
        await Task.Delay(5);
        // Yield the items one by one asynchronously
        yield return new Customer
        {
            Id = 1,
            Name = "Customer1",
            Orders = new List<Order> {
                new Order {
                    Name = "Order1",
                    Price = 25
                },
                new Order {
                     Name = "Order2",
                     Price = 75
                }
            },
            Address = new Address
            {
                Name = "City1",
                Street = "Street1"
            }
        };

        await Task.Delay(5);

        yield return new Customer
        {
            Id = 2,
            Name = "Customer2",
            Orders = new List<Order> {
                new Order {
                    Name = "Order1",
                    Price = 35
                },
                new Order {
                     Name = "Order2",
                     Price = 65
                }
            },
            Address = new Address
            {
                Name = "City2",
                Street = "Street2"
            }
        };
    }

    public void Generate()
    {
        for (int i = 1; i <= 3; i++)
        {
            var customer = new Customer
            {
                Name = "Customer" + (i + 1) % 2,
                Orders =
                    new List<Order> {
                        new Order {
                            Name = "Order" + 2*i,
                            Price = i * 25  
                        },
                        new Order {
                            Name = "Order" + 2*i+1,
                            Price = i * 75
                        }
                    },
                Address = new Address
                {
                    Name = "City" + i % 2,
                    Street = "Street" + i % 2,
                }
            };

            _context.Customers.Add(customer);
        }

        _context.SaveChanges();
    }

    public enum Variant
    {
        None = 0,
        Typed = 1,
        Generic = 2,
    }

    private async IAsyncEnumerable<Customer> TypedAsyncEnumerableWithDelay(IAsyncEnumerable<Customer> asyncEnumerable, TimeSpan delay)
    {
        await foreach (var entity in asyncEnumerable)
        {
            await Task.Delay(delay);
            yield return entity;
        }
    }

    private async IAsyncEnumerable<TEntity> GenericAsyncEnumerableWithDelay<TEntity>(IAsyncEnumerable<TEntity> asyncEnumerable, TimeSpan delay)
    {
        await foreach (var entity in asyncEnumerable)
        {
            await Task.Delay(delay);
            yield return entity;
        }
    }
}
