// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
