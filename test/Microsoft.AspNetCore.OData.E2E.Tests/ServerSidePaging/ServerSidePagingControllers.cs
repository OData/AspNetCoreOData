//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ServerSidePaging
{
    public class ServerSidePagingCustomersController : ODataController
    {
        private readonly IList<ServerSidePagingCustomer> _serverSidePagingCustomers;

        public ServerSidePagingCustomersController()
        {
            _serverSidePagingCustomers = Enumerable.Range(1, 7)
                .Select(i => new ServerSidePagingCustomer
                {
                    Id = i,
                    Name = "Customer Name " + i
                }).ToList();

            for (int i = 0; i < _serverSidePagingCustomers.Count; i++)
            {
                // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
                // NextPageLink will be expected on the Customers collection as well as
                // the Orders child collection on Customer 1
                _serverSidePagingCustomers[i].ServerSidePagingOrders = Enumerable.Range(1, 6 - i)
                    .Select(j => new ServerSidePagingOrder
                    {
                        Id = j,
                        Amount = (i + j) * 10,
                        ServerSidePagingCustomer = _serverSidePagingCustomers[i]
                    }).ToList();
            }
        }

        [EnableQuery(PageSize = 5)]
        public IActionResult Get()
        {
            return Ok(_serverSidePagingCustomers);
        }
    }

    public class ServerSidePagingEmployeesController : ODataController
    {
        private static List<ServerSidePagingEmployee> employees = new List<ServerSidePagingEmployee>(
            Enumerable.Range(1, 13).Select(idx => new ServerSidePagingEmployee
            {
                Id = idx,
                HireDate = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2022, 11, 07).AddMonths(idx), TimeZoneInfo.Local)
            }));

        [HttpGet]
        [EnableQuery(PageSize = 3)]
        public IActionResult GetEmployeesHiredInPeriod([FromRoute] DateTime fromDate, [FromRoute] DateTime toDate)
        {
            var hiredInPeriod = employees.Where(d => d.HireDate >= fromDate && d.HireDate <= toDate);

            return Ok(hiredInPeriod);
        }
    }
}
