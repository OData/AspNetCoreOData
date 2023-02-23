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
                LastName = "Zha/ngg",
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

        // People(FirstName='Goods',LastName='Zha%2Fngg')
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
    }
}
