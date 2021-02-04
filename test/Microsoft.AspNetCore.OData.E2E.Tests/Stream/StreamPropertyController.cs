// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.StreamProperty
{
    public class StreamCustomersController : ODataController
    {
        [HttpGet]
        [EnableQuery]
        public IQueryable<StreamCustomer> Get()
        {
            return CreateCustomers().AsQueryable();
        }

        [HttpGet]
        public IActionResult Get(int key)
        {
            IList<StreamCustomer> customers = CreateCustomers();
            StreamCustomer customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpGet]
        public IActionResult GetName(int key)
        {
            IList<StreamCustomer> customers = CreateCustomers();
            StreamCustomer customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Name);
        }

        [HttpGet]
        public IActionResult GetPhoto(int key)
        {
            IList<StreamCustomer> customers = CreateCustomers();
            StreamCustomer customer = customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Photo);
        }

        private static IList<StreamCustomer> CreateCustomers()
        {
            byte[] byteArray = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            IList<StreamCustomer> customers = Enumerable.Range(0, 5).Select(i =>
                new StreamCustomer
                {
                    Id = i,
                    Name = "FirstName " + i,
                    Photo = new MemoryStream(byteArray, i, 4)
                }).ToList();

            foreach (var c in customers)
            {
                c.PhotoText = new StreamReader(c.Photo).ReadToEnd();
                c.Photo.Seek(0, SeekOrigin.Begin);
            }

            return customers;
        }
    }
}