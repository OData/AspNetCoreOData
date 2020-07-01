// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatting;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Commons;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v2
{
    [ODataModel("v2{data}")]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Order
            {
                Id = index,
                Title = "Title + " + index
            })
            .ToArray();
        }

        [HttpGet]
        public Order Get(int key)
        {
            return new Order
            {
                Id = key,
                Title = "Title + " + key
            };
        }

        [HttpGet]
        public bool CanMoveToAddress(int key, [FromODataUri] Address address)
        {
            return true;
        }

        [HttpGet]
        public string GetProperty(int key, string property)
        {
            return $"{property} in order";
        }

        [HttpGet]
        public string GetTitle(int key)
        {
            return "Orders Title";
        }

        public string CreateRefToCategory(int key)
        {
            return "CreateRefToCategory";
        }
    }
}
