//-----------------------------------------------------------------------------
// <copyright file="OrderByController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataOrderByTest
{
    [Route("odata")]
    public class OrderByItemsController : ODataController
    {
        private readonly OrderByContext _db;
        private static readonly IQueryable<ItemWithoutColumn> _itemWithoutColumns;

        static OrderByItemsController()
        {
            _itemWithoutColumns = new List<ItemWithoutColumn>()
            {
                // The key is A, B, C
                new ItemWithoutColumn() { A = 2, B = 1, C = 1, ExpectedOrder = 4 },
                new ItemWithoutColumn() { A = 1, B = 2, C = 1, ExpectedOrder = 3 },
                new ItemWithoutColumn() { A = 1, B = 1, C = 2, ExpectedOrder = 2 },
                new ItemWithoutColumn() { A = 1, B = 1, C = 1, ExpectedOrder = 1 }
            }.AsQueryable();
        }

        public OrderByItemsController(OrderByContext context)
        {
            context.Database.EnsureCreated();
            if (!context.Items.Any())
            {
                AddInSet(context.Items,
                    // The key is C, A, B
                    new Item() { A = 1, B = 99, C = 2, ExpectedOrder = 4 },
                    new Item() { A = 2, B = 2, C = 1, ExpectedOrder = 3 },
                    new Item() { A = 2, B = 1, C = 1, ExpectedOrder = 2 },
                    new Item() { A = 1, B = 96, C = 1, ExpectedOrder = 1 }
                );

                AddInSet(context.Items2,
                    // The key is C, B, A
                    new Item2() { A = "AA", C = "BB", B = 99, ExpectedOrder = 2 },
                    new Item2() { A = "BB", C = "AA", B = 98, ExpectedOrder = 1 },
                    new Item2() { A = "01", C = "XX", B = 1, ExpectedOrder = 3 },
                    new Item2() { A = "00", C = "ZZ", B = 96, ExpectedOrder = 4 }
                );

                AddInSet(context.ItemsWithEnum,
                    // The key is C, B, A
                    new ItemWithEnum() { A = SmallNumber.One, B = "A", C = SmallNumber.One, ExpectedOrder = 1 },
                    new ItemWithEnum() { A = SmallNumber.One, B = "B", C = SmallNumber.One, ExpectedOrder = 3 },
                    new ItemWithEnum() { A = SmallNumber.One, B = "B", C = SmallNumber.Two, ExpectedOrder = 4 },
                    new ItemWithEnum() { A = SmallNumber.Two, B = "A", C = SmallNumber.One, ExpectedOrder = 2 }
                );
                context.SaveChanges();
            }

            _db = context;
        }

        private static void AddInSet<T>(DbSet<T> set, params T[] items) where T : class
        {
            foreach (var item in items)
            {
                set.Add(item);
            }
        }

        [EnableQuery]
        [HttpGet("Items")]
        public IActionResult GetItems()
        {
            return Ok(_db.Items);
        }

        [EnableQuery]
        [HttpGet("Items2")]
        public IActionResult GetItems2()
        {
            return Ok(_db.Items2);
        }

        [EnableQuery]
        [HttpGet("ItemsWithEnum")]
        public IActionResult GetItemsWithEnum()
        {
            return Ok(_db.ItemsWithEnum);
        }

        [EnableQuery]
        [HttpGet("ItemsWithoutColumn")]
        public IActionResult GetItemsWithoutColumn()
        {
            return Ok(_itemWithoutColumns);
        }
    }
}
