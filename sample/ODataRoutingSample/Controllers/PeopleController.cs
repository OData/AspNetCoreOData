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
            /* As example: If you send a POST request to: http://localhost:5000/People, you can get all things with 'person' parameter.
{
    "firstname": "sam",
    "lastname": "xu",
    "data":[[42],{"k1": "abc", "k2": 42, "k3": { "a1": 2, "b2": null}, "k4": [null, 42]}],
    "infos": [ 42, "str"],
    "customProperties": {
        "NullProp": null,
        "CollectionDynamic": [
            {
                "P1": "v1",
                "P2": "v2"
            },
            {
                "Y1": 1,
                "Y2": true
            }
        ],
        "StringEqualsIgnoreCase": {
          "IntProp": 42,
          "awsTag":[
            null,
            {
                "X1": "Red",
                "Data": {
                    "D1": 42
                }
            },
            "finance",
            "hr",
            "legal",
            43
        ]
      },
      "key1": "value1",
      "key2": [
          "value2",
          "value3"
      ],
      "key3": "value4"
    }
}
             */

            if (person == null || !ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(message);
            }

            _persons.Add(person);
            person.CustomProperties.Properties.TryGetValue("StringEqualsIgnoreCase", out var property);

            EdmUntypedObject odataObject = property as EdmUntypedObject;
            if (odataObject != null)
            {
                IEnumerable enumerable = odataObject["awsTag"] as IEnumerable;
                foreach (var a in enumerable)
                {
                    // do thing, just for testing.
                }
            }

            // Keep the following codes, it tests
            // 1) you can use any C# class to create an object and assign it to 'Edm.Untyped' property
            // 2) you can use dictionary now also.
            //person.Other = new Address
            //{
            //    City = "Redmond",
            //    Street = "148TH AVE NE"
            //};
            person.Other = new Dictionary<string, object>
            {
                { "City", "Redmond" },
                { "Street/A", "1345TH" }
            };

            person.Sources = new List<object>
            {
                null,
                42,
                "A string Value",
                Color.Red,
                AnyEnum.E1,
                true,
                new Address { City = "Issaquah", Street = "Klahanie Way" },
                new EdmUntypedObject
                {
                    { "a1", "abc" }
                },
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

            /* If you keep the above value, you could get the following response (It could be not up-to-date)
{
    "@odata.context": "http://localhost:5000/$metadata#People/$entity",
    "@odata.type": "#ODataRoutingSample.Models.Person",
    "@odata.id": "http://localhost:5000/People(FirstName='sam',LastName='xu')",
    "@odata.editLink": "People(FirstName='sam',LastName='xu')",
    "FirstName": "sam",
    "LastName": "xu",
    "Data": [
        [
            42
        ],
        {
            "k1": "abc",
            "k2@odata.type": "#Decimal",
            "k2": 42,
            "k3": {
                "a1@odata.type": "#Decimal",
                "a1": 2,
                "b2": null
            },
            "k4": [
                null,
                42
            ]
        }
    ],
    "Other": {
        "City": "Redmond",
        "Street/A": "1345TH"
    },
    "Infos": [
        42,
        "str"
    ],
    "Sources": [
        null,
        42,
        "A string Value",
        "Red",
        "E1",
        true,
        {
            "City": "Issaquah",
            "Street": "Klahanie Way"
        },
        {
            "a1": "abc"
        },
        [
            null,
            "A string Value"
        ]
    ],
    "CustomProperties": {
        "@odata.type": "#ODataRoutingSample.Models.PersonExtraInfo",
        "NullProp": null,
        "key1": "value1",
        "key3": "value4",
        "CollectionDynamic": [
            {
                "P1": "v1",
                "P2": "v2"
            },
            {
                "Y1@odata.type": "#Decimal",
                "Y1": 1,
                "Y2": true
            }
        ],
        "StringEqualsIgnoreCase": {
            "IntProp@odata.type": "#Decimal",
            "IntProp": 42,
            "awsTag": [
                null,
                {
                    "X1": "Red",
                    "Data": {
                        "D1@odata.type": "#Decimal",
                        "D1": 42
                    }
                },
                "finance",
                "hr",
                "legal",
                43
            ]
        },
        "key2": [
            "value2",
            "value3"
        ]
    }
}
             */
            return Created(person);
        }
    }
}
