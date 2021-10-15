//-----------------------------------------------------------------------------
// <copyright file="PeopleController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class PeopleController : ControllerBase
    {
        private static IList<Person> _persons = new List<Person>
        {
            new Person
            {
                FirstName = "Goods",
                LastName = "Zhangg",
            },
            new Person
            {
                FirstName = "Magazine",
                LastName = "Jingchan",
            },
            new Person
            {
                FirstName = "Fiction",
                LastName = "Hollewye"
            },
        };

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(CancellationToken token)
        {
            return Ok(_persons);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(string keyFirstName, string keyLastName)
        {
            var person = _persons.FirstOrDefault(p => p.FirstName == keyFirstName && p.LastName == keyLastName);
            if (person == null)
            {
                return NotFound($"Not found person with FirstName = {keyFirstName} and LastName = {keyLastName}");
            }

            return Ok(person);
        }

        // [ODataAttributeRouting]
        // [HttpGet("People/$filter({filterClause})")] // it's failed in ODL
        // [HttpGet("People/$filter(true)")] // it can pass the build, but the route template cannot change
        // [HttpGet("People/$filter(@p1)")] // it also can pass the build, but the route template cannot change.
        public IActionResult FindPerson(/*[FromRoute] FilterQueryOption filter*/)
        {
            FilterQueryOption filter = Request.RouteValues["filter"] as FilterQueryOption;

            return Ok(filter.ApplyTo(_persons.AsQueryable(), new ODataQuerySettings()));
        }
    }
}
