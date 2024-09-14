//-----------------------------------------------------------------------------
// <copyright file="EmployeesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation.BulkOperationDataModel;

namespace Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation;

internal class EmployeesController : ODataController
{
    public EmployeesController()
    {
        if (null == Employees)
        {
            InitEmployees();
        }
    }

    /// <summary>
    /// static so that the data is shared among requests.
    /// </summary>
    public static IList<Employee> Employees = null;

    private List<Friend> Friends = null;

    private void InitEmployees()
    {
        Friends = new List<Friend>
        {
            new Friend
            {
                Id = 1,
                Name = "Test0"
            },
            new Friend
            {
                Id = 2,
                Name = "Test1",
                Orders = new List<Order>()
                {
                    new Order
                    {
                        Id = 1,
                        Price = 2
                    } 
                }
            },
            new Friend
            {
                Id = 3,
                Name = "Test3"
            },
            new Friend
            {
                Id = 4,
                Name = "Test4"
            }
        };
        Employees = new List<Employee>
        {
            new Employee()
            {
                ID=1,
                Name="Name1",
                Friends = this.Friends.Where(x=>x.Id ==1 || x.Id==2).ToList()
            },
            new Employee()
            {
                ID=2,Name="Name2",
                Friends =  this.Friends.Where(x=>x.Id ==3 || x.Id==4).ToList()
            },
            new Employee()
            {
                ID=3,
                Name="Name3"
            },
        };
    }

    [HttpPatch]
    public IActionResult PatchEmployees([FromBody] DeltaSet<Employee> coll)
    {
        Assert.NotNull(coll);
        return Ok(coll);
    }
}
