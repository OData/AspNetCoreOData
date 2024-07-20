//-----------------------------------------------------------------------------
// <copyright file="EnumsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    [Route("convention")]
    public class EmployeesController : ODataController
    {
        private readonly ListsContext _dbContext;
        public EmployeesController(ListsContext context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        private static IList<Employee> Employees = null;

        private void InitEmployees()
        {
            Employees = new List<Employee>
            {
                new Employee()
                {
                    ID=1,
                    Name="Name1",
                    ListTestInt= new []{1,2,3,4,5},
                    ListTestUri= new []{new System.Uri("https://google.com"), new System.Uri("https://facebook.com")},
                    ListTestUint= new uint[]{1,2,3,4,5},
                    ListTestFloat= new []{1.2f,2.3f,3.4f},
                    ListTestDouble = new []{1,23,4.56,7.89},
                    ListTestString = new []{"Hello", "From","The","Other", "Side"},
                    ListTestBool= new []{true, false, true},

                },
                new Employee()
                {
                    ID=2,
                    Name="Name2",
                    ListTestInt= new []{1,2,3,4,5},
                    ListTestUri= new []{new System.Uri("https://google.com"), new System.Uri("https://facebook.com")},
                    ListTestUint= new uint[]{1,2,3,4,5},
                    ListTestFloat= new []{1.2f,2.3f,3.4f},
                    ListTestDouble = new []{1,23,4.56,7.89},
                    ListTestString = new []{"I", "Must","Have","Called", "A", "Thousand", "Times"},
                    ListTestBool= new []{true, false, true},

                },
                new Employee()
                {
                    ID=3,Name="Name3",
                    ListTestInt= new []{1,2,3,4,5},
                    ListTestUri= new []{new System.Uri("https://google.com"), new System.Uri("https://facebook.com")},
                    ListTestUint= new uint[]{1,2,3,4,5},
                    ListTestFloat= new []{1.2f,2.3f,3.4f},
                    ListTestDouble = new []{1,23,4.56,7.89},
                    ListTestString = new []{"Hello", "It's","Me"},
                    ListTestBool= new []{true, false, true},

                },
            };
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IActionResult Get()
        {
            return Ok(Employees.AsQueryable());
        }

        public IActionResult Get(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key));
        }

        public IActionResult Post([FromBody] Employee employee)
        {
            employee.ID = Employees.Count + 1;
            Employees.Add(employee);

            return Created(employee);
        }

        public IActionResult Put(int key, [FromBody] Employee employee)
        {
            employee.ID = key;
            Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);

            if (originalEmployee == null)
            {
                Employees.Add(employee);

                return Created(employee);
            }

            Employees.Remove(originalEmployee);
            Employees.Add(employee);
            return Ok(employee);
        }

        public IActionResult Patch(int key, [FromBody] Delta<Employee> delta)
        {
            Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);

            if (originalEmployee == null)
            {
                Employee temp = new Employee();
                delta.Patch(temp);
                Employees.Add(temp);
                return Created(temp);
            }

            delta.Patch(originalEmployee);
            return Ok(delta);
        }

        public IActionResult Delete(int key)
        {
            IEnumerable<Employee> appliedEmployees = Employees.Where(c => c.ID == key);

            if (appliedEmployees.Count() == 0)
            {
                return BadRequest(string.Format("The entry with ID {0} doesn't exist", key));
            }

            Employee employee = appliedEmployees.Single();
            Employees.Remove(employee);
            return this.StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost]
        //public IActionResult AddSkill([FromODataUri] int key, [FromBody] ODataActionParameters parameters)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    Skill skill = (Skill)parameters["skill"];

        //    if (key == 6)
        //    {
        //        Assert.Equal(Skill.Sql, skill);
        //        return Ok();
        //    }

        //    Employee employee = Employees.FirstOrDefault(e => e.ID == key);
        //    if (!employee.SkillSet.Contains(skill))
        //    {
        //        employee.SkillSet.Add(skill);
        //    }

        //    return Ok(employee.SkillSet);
        //}

        [HttpPost("ResetDataSource")]
        public IActionResult ResetDataSource()
        {
            this.InitEmployees();
            return this.StatusCode(StatusCodes.Status204NoContent);
        }


    }

}
