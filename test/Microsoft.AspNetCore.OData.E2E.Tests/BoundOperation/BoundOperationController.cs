//-----------------------------------------------------------------------------
// <copyright file="BoundOperationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.BoundOperation
{
    [Route("AttributeRouting")]
    public class EmployeesController : ODataController
    {
        private static List<Employee> _employees = null;
        private static List<Manager> _managers = null;

        private static void InitEmployeesAndManagers()
        {
            _managers = Enumerable.Range(1, 5).Select(i => new Manager
            {
                ID = i,
                Name = "Name" + i + "/",
                Address = new Address()
                {
                    Street = "Street" + i,
                    City = "City" + i,
                },
                Emails = Enumerable.Range(1, i).Select(j => string.Format("Name{0}_{1}@microsoft.com", i, j)).ToList(),
                Salary = i * 10,
                Heads = i,
            }).ToList();

            _employees = Enumerable.Range(6, 5).Select(i =>
            new Employee
            {
                ID = i,
                Name = "Name" + i + "?",
                Address = new Address()
                {
                    Street = "Street" + i,
                    City = "City" + i,
                },
                Emails = Enumerable.Range(1, i).Select(j => string.Format("Name{0}_{1}@microsoft.com", i, j)).ToList(),
                Salary = i * 10,
            }).ToList();

            int k = 20;
            _employees.Add(
            new Employee
            {
                ID = k,
                Name = "Name" + k + "#",
                Address = new Address()
                {
                    Street = "Street" + k,
                    City = "City" + k,
                },
                Emails = new List<string>(),
                Salary = k * 10,
            });


            _employees.AddRange(_managers);
        }

        static EmployeesController()
        {
            InitEmployeesAndManagers();
        }

        public IList<Employee> Customers { get { return _employees; } }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IActionResult Get()
        {
            return Ok(_employees.AsQueryable());
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IActionResult GetFromManager()
        {
            return Ok(_managers.AsQueryable());
        }

        // ~/Employees/Namespace.GetCount()
        [HttpGet]
        public int GetCount()
        {
            return _employees.Count();
        }

        // ~/Employees/Namespace.GetCount()
        [HttpGet("Employees/Default.GetCount()")]
        public int GetCountAttributeRouting()
        {
            return this.GetCount() * 2;// multiplied by 2 is to make it easy to verify if it is returned by attribute routing function.
        }

        /*
        // ~/Employees/Namespace.GetCount(Name='Name1')
        [HttpGet]
        public int GetCount(string name)
        {
            name = name.Replace("%2F", "/");
            return _employees.Where(e => e.Name.Contains(name)).Count();
        }

        // ~/Employees/Namespace.GetCount(Name='Name1')
        [HttpGet("Employees/Default.GetCount(Name={name})")]
        public int GetCountAttributeRouting([FromODataUri]string name)
        {
            return this.GetCount(name) * 2;
        }

        // ~/Employees/Namespace.Manager/Namespace.GetCount()
        [HttpGet]
        public int GetCountOnCollectionOfManager()
        {
            return _managers.Count();
        }

        // ~/Employees/Namesapce.Manager/Namespace.GetCount()
        [HttpGet("Employees/NS.Manager/Default.GetCount()")]
        public int GetCountOnCollectionOfManagerAttributeRouting()
        {
            return this.GetCountOnCollectionOfManager() * 2;
        }

        // ~/Employees(1)/OptionalAddresses
        // ~/Employees(1)/OptionalAddresses/$count
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetOptionalAddresses()")]
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetOptionalAddresses()/$count")]
        [HttpGet]
        [EnableQuery]
        public IActionResult GetOptionalAddresses(int key)
        {
            IList<Address> addresses = new List<Address>();
            addresses.Add(new Address { City = "Shanghai", Street = "Zixing" });
            return Ok(addresses);
        }

        // ~/Employees(1)/OptionalAddresses
        // ~/Employees(1)/OptionalAddresses/$count
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetOptionalAddresses()")]
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetOptionalAddresses()/$count")]
        [EnableQuery]
        [HttpGet("Employees({key})/OptionalAddresses")]
        [HttpGet("Employees({key})/OptionalAddresses/$count")]
        [HttpGet("Employees({key})/Default.GetOptionalAddresses()")]
        [HttpGet("Employees({key})/Default.GetOptionalAddresses()/$count")]
        public IActionResult GetOptionalAddressesAttributeRouting(int key)
        {
            IList<Address> addresses = new List<Address>();
            addresses.Add(new Address { City = "Shanghai", Street = "Zixing" });
            addresses.Add(new Address { City = "Beijing", Street = "Zhongshan" });
            return Ok(addresses);
        }

        // ~/Employees(1)/Emails
        // ~/Employees(1)/Emails/$count
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetEmails()
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetEmails()/$count
        [HttpGet]
        [EnableQuery]
        public IActionResult GetEmails(int key)
        {
            IList<string> emails = new List<string>();
            emails.Add("a@a.com");
            return Ok(emails);
        }

        // ~/Employees(1)/Emails
        // ~/Employees(1)/Emails/$count
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetEmails()
        // ~/Employees(1)/Microsoft.Test.E2E.AspNet.OData.BoundOperation.GetEmails()/$count
        [EnableQuery]
        [HttpGet("Employees({key})/Emails")]
        [HttpGet("Employees({key})/Emails/$count")]
        [HttpGet("Employees({key})/Default.GetEmails()")]
        public IActionResult GetEmailsAttributeRouting([FromODataUri]int key)
        {
            IList<string> emails = new List<string>();
            emails.Add("a@a.com");
            emails.Add("b@b.com");
            return Ok(emails);
        }

        // ~/Employees(1)/Namesapce.GetEmailsCount()
        [HttpGet]
        public int GetEmailsCount(int key)
        {
            return _employees.Where(e => e.ID == key).First().Emails.Count;
        }

        // ~/Employees(1)/Namesapce.GetEmailsCount()
        [HttpGet("Employees({key})/Default.GetEmailsCount()")]
        public int GetEmailsCountAttributeRouting([FromODataUri]int key)
        {
            return this.GetEmailsCount(key) * 2;
        }

        // ~/Employees(1)/Namespace.Manager/Namespace.GetEmailsCount()
        [HttpGet]
        public int GetEmailsCountOnManager(int key)
        {
            return _managers.Where(e => e.ID == key).First().Emails.Count;
        }

        // ~/Employees(1)/Namespace.Manager/Namesapce.GetEmailsCount()
        [HttpGet("Employees({key})/NS.Manager/Default.GetEmailsCount()")]
        public int GetEmailsCountOnManagerAttributeRouting(int key)
        {
            return this.GetEmailsCountOnManager(key) * 2;
        }

        // using [FromODataUri] or not is non-sense for primitive. except for string type.
        [HttpGet("Employees/Default.PrimitiveFunction(param={param},price={price},name={name},names={names})")]
        public string PrimitiveFunction(int param, double? price, [FromODataUri]string name, [FromODataUri]IEnumerable<string> names)
        {
            StringBuilder sb = new StringBuilder("(param=" + param);
            sb.Append(",price=").Append(price == null ? "null" : price.ToString());
            sb.Append(",name=").Append(name == null ? "null" : "'" + name + "'");

            Assert.NotNull(names);
            sb.Append(",names=[").Append(String.Join(",", names.Select(n => n == null ? "null" : "'" + n + "'")));
            sb.Append("])");
            return sb.ToString();
        }

        // using [FromODataUri] or not is non-sense for primitive. except for string type.
        [HttpGet("Employees/Default.EnumFunction(bkColor={bkColor},frColor={frColor},colors={colors})")]
        public string EnumFunction([FromODataUri]Color bkColor, [FromODataUri]Color? frColor, [FromODataUri]IEnumerable<Color> colors)
        {
            StringBuilder sb = new StringBuilder("(bkColor='" + bkColor);
            sb.Append("',frColor=").Append(frColor == null ? "null" : "'" + frColor.ToString() + "'");

            Assert.NotNull(colors);
            sb.Append(",colors=[").Append(String.Join(",", colors.Select(c => "'" + c + "'")));
            sb.Append("])");
            return sb.ToString();
        }

        [HttpGet("Employees/Default.ComplexFunction(address={address},location={location},addresses={addresses})")]
        public IActionResult ComplexFunction([FromODataUri]Address address, [FromODataUri]Address location, [FromODataUri]IEnumerable<Address> addresses)
        {
            return Ok(new[] { address, location }.Concat(addresses));
        }

        [HttpGet("Employees/Default.EntityFunction(person={person},guard={guard},staff={staff})")]
        public IActionResult EntityFunction([FromODataUri]Employee person, [FromODataUri]Employee guard, [FromODataUri]IEnumerable<Employee> staff)
        {
            VerifyEmployee(person);
            VerifyEmployee(guard);
            foreach (var p in staff)
            {
                VerifyEmployee(p);
            }

            return Ok();
        }

        [HttpGet("Employees/Default.GetWholeSalary(minSalary={min})")]
        public string GetWholeSalaryWithMin(double min)
        {
            return GetWholeSalary(min);
        }

        [HttpGet("Employees/Default.GetWholeSalary(minSalary={min},maxSalary={max})")]
        public string GetWholeSalaryWithMinAndMax(double min, double max)
        {
            return GetWholeSalary(min, max);
        }

        [HttpGet("Employees/Default.GetWholeSalary(minSalary={minSalary},maxSalary={maxSalary},aveSalary={aveSalary})")]
        public string GetWholeSalary(double minSalary, double maxSalary = 0, double aveSalary = 8.9)
        {
            return String.Format(CultureInfo.InvariantCulture, "GetWholeSalary({0}, {1}, {2})", minSalary, maxSalary, aveSalary);
        }

        private static void VerifyEmployee(Employee employee)
        {
            if (employee == null)
            {
                return;
            }

            // entity reference call
            if (employee.ID == 8)
            {
                Assert.Null(employee.Name);
                return;
            }

            // entity call
            var manager = employee as Manager;
            if (manager != null)
            {
                Assert.Equal(901, manager.ID);
                Assert.Equal("John", manager.Name);
                Assert.Equal(9, manager.Heads);
            }
            else
            {
                Assert.Equal(801, employee.ID);
                Assert.Equal("Mike", employee.Name);
            }
        }

        [HttpPost("ResetDataSource")]
        public IActionResult ResetDataSource()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            InitEmployeesAndManagers();
            return Ok();
        }

        // ~/Employees/Namespace.IncreaseSalary
        [EnableQuery]
        [HttpPost]
        public IActionResult IncreaseSalary([FromBody]ODataUntypedActionParameters odataActionParameters)
        {
            string name = odataActionParameters["Name"] as string;
            IEnumerable<Employee> candidates = _employees.Where(e => e.Name.StartsWith(name));
            candidates.Select(e => e.Salary = e.Salary * 2);
            return Ok(candidates);
        }

        // ~/Employees/Namespace.IncreaseSalary
        [EnableQuery]
        [HttpPost("Employees/Default.IncreaseSalary")]
        public IActionResult IncreaseSalaryAttributeRouting([FromBody]ODataUntypedActionParameters odataActionParameters)
        {
            string name = odataActionParameters["Name"] as string;
            IEnumerable<Employee> candidates = _employees.Where(e => e.Name.StartsWith(name));
            candidates.Select(e => e.Salary = e.Salary * 2);
            return Ok(candidates.Where(e => e.ID <= 5));
        }

        // ~/Employees/Namespace.Manager/Namespace.IncreaseSalary
        [HttpPost]
        public IActionResult IncreaseSalaryOnCollectionOfManager([FromBody]ODataUntypedActionParameters odataActionParameters)
        {
            string name = odataActionParameters["Name"] as string;
            IEnumerable<Employee> candidates = _managers.Where(e => e.Name.StartsWith(name));
            candidates.Select(e => e.Salary = e.Salary * 2);
            return Ok(candidates);
        }

        // ~/Employees/Namespace.Manager/Namesapce.IncreaseSalary
        [EnableQuery]
        [HttpPost("Employees/NS.Manager/Default.IncreaseSalary")]
        public IActionResult IncreaseSalaryOnCollectionOfManagerAttributeRouting([FromBody]ODataUntypedActionParameters odataActionParameters)
        {
            string name = odataActionParameters["Name"] as string;
            IEnumerable<Employee> candidates = _managers.Where(e => e.Name.StartsWith(name));
            candidates.Select(e => e.Salary = e.Salary * 2);
            return Ok(candidates.Where(m => m.ID % 2 == 0));
        }


        // ~/Employees(1)/Namespace.IncreaseSalary
        [HttpPost]
        public IActionResult IncreaseSalaryOnEmployee([FromODataUri]int key)
        {
            var employee = _employees.Where(e => e.ID == key).First();
            employee.Salary *= 2;
            return Ok(employee.Salary);
        }

        // ~/Employees(1)/Namespace.IncreaseSalary
        [HttpPost("Employees({key})/Default.IncreaseSalary")]
        public IActionResult IncreaseSalaryOnEmployeeAttributeRouting([FromODataUri]int key)
        {
            var employee = _employees.Where(e => e.ID == key).First();
            employee.Salary *= 4;
            return Ok(employee.Salary);
        }

        // ~/Employees(1)/Namespace.Manager/Namespace.IncreaseSalary
        [HttpPost]
        public IActionResult IncreaseSalaryOnManager([FromODataUri] int key)
        {
            var manager = _managers.Where(m => m.ID == key).First();
            manager.Salary *= 2;
            return Ok(manager.Salary);
        }

        // ~/Employees(1)/Namespace.Manager/Namespace.IncreaseSalary
        [HttpPost("Employees({key})/NS.Manager/Default.IncreaseSalary")]
        public IActionResult IncreaseSalaryOnManagerAttributeRouting([FromODataUri] int key)
        {
            var manager = _managers.Where(m => m.ID == key).First();
            manager.Salary *= 4;
            return Ok(manager.Salary);
        }

        [HttpPost("Employees/Default.PrimitiveAction")]
        public IActionResult PrimitiveAction([FromBody]ODataActionParameters parameters)
        {
            Assert.Equal(4, parameters.Count);
            Assert.Equal(7, parameters["param"]);
            Assert.Equal(9.9, parameters["price"]);
            Assert.Equal("Tony", parameters["name"]);

            Assert.NotNull(parameters["names"]);
            IList<string> names = (parameters["names"] as IEnumerable<string>).ToList();
            Assert.NotNull(names);

            Assert.Equal("Mike", names[0]);
            Assert.Null(names[1]);
            Assert.Equal("John", names[2]);

            return Ok(true);
        }

        [HttpPost("Employees/Default.EnumAction")]
        public IActionResult EnumAction([FromBody]ODataActionParameters parameters)
        {
            Assert.Equal(3, parameters.Count);
            Assert.Equal(Color.Red, parameters["bkColor"]);
            Assert.Equal(Color.Green, parameters["frColor"]);

            Assert.NotNull(parameters["colors"]);
            IList<Color> colors = (parameters["colors"] as IEnumerable<Color>).ToList();
            Assert.NotNull(colors);

            Assert.Equal(Color.Red, colors[0]);
            Assert.Equal(Color.Blue, colors[1]);

            return Ok(true);
        }

        [HttpPost("Employees/Default.ComplexAction")]
        public IActionResult ComplexAction([FromBody]ODataActionParameters parameters)
        {
            Assert.Equal(3, parameters.Count);

            Assert.NotNull(parameters["addresses"]);
            IList<Address> addresses = (parameters["addresses"] as IEnumerable<Address>).ToList();
            Assert.NotNull(addresses);

            foreach (Address address in new [] { parameters["address"], addresses[0]})
            {
                Assert.NotNull(address);
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);
            }

            foreach (SubAddress location in new[] { parameters["location"], addresses[1] })
            {
                Assert.NotNull(location);
                Assert.Equal("LianHua Rd.", location.Street);
                Assert.Equal("Shanghai", location.City);
                Assert.Equal(9.9, location.Code);
            }

            return Ok(true);
        }

        [HttpPost("Employees/Default.EntityAction")]
        public IActionResult EntityAction([FromBody]ODataActionParameters parameters)
        {
            Assert.Equal(3, parameters.Count);

            Assert.NotNull(parameters["staff"]);
            IList<Employee> staff = (parameters["staff"] as IEnumerable<Employee>).ToList();
            Assert.NotNull(staff);

            foreach (Employee person in new[] { parameters["person"], staff[0] })
            {
                Assert.NotNull(person);
                Assert.Equal(801, person.ID);
                Assert.Equal("Mike", person.Name);
                Assert.Null(person.Address);
            }

            foreach (Manager guard in new[] {parameters["guard"], staff[1]})
            {
                Assert.NotNull(guard);
                Assert.Equal(901, guard.ID);
                Assert.Equal("John", guard.Name);
                Assert.Equal(9, guard.Heads);
                Assert.Null(guard.Address);
            }

            return Ok(true);
        }*/
    }
}
