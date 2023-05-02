//-----------------------------------------------------------------------------
// <copyright file="PeopleController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class PeopleController : ODataController
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

        [HttpPost]
        [EnableQuery]
        public IActionResult Post([FromBody] Person person)
        {
            _persons.Add(person);

            EdmUntypedObject odataObject = person.Condition.Properties["StringEqualsIgnoreCase"] as EdmUntypedObject;

            IEnumerable enumerable = odataObject["awsTag"] as IEnumerable;
            foreach (var a in enumerable)
            {

            }

            // person.Data = 42; // ODataPrimitiveValue
            person.Other = new Address
            {
                City = "Redmond",
                Street = "148TH AVE NE"
            };

            // shall we support
            //person.Other = new Dictionary<string, object>
            //{

            //}

            person.Sources = new List<object>
            {
                null,
                42,
                "A string Value",
                true,
                new Address { City = "Issaquah", Street = "Klahanie Way" },
                new EdmUntypedObject
                {
                    { "a1", "abc" }
                },
                // So far, it can't support it?
                new EdmUntypedCollection
                {
                    null,
                    "A string Value",
                    // The following resources can't work: https://github.com/OData/odata.net/issues/2661
                    // new Address { City = "Issaquah", Street = "Klahanie Way" }, 
                    //new Person
                    //{
                    //    FirstName = "Kerry", LastName = "Xu"
                    //}
                }
            };

            return Created(person);
        }
    }
}
