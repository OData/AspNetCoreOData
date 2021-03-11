// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NavigationPropertyOnComplexType
{
    public class PeopleController : ControllerBase
    {
        private PeopleRepository _repo = new PeopleRepository();

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Person> Get()
        {
            return _repo.People;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get([FromODataUri]int key)
        {
            Person person = _repo.People.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        [EnableQuery]
        public IActionResult GetHomeLocationFromPerson([FromODataUri]int key)
        {
            Person person = _repo.People.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person.HomeLocation);
        }

        [EnableQuery]
        public IActionResult GetRepoLocationsFromPerson([FromODataUri]int key)
        {
            Person person = _repo.People.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person.RepoLocations);
        }

        /*
        [EnableQuery]
        public ITestActionResult GetLocationOfAddress([FromODataUri]int key)
        {
            Person person = _repo.people.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person.Location as Address);
        }*/

        [EnableQuery]
        public IActionResult GetHomeLocationOfGeoLocation([FromODataUri]int key)
        {
            Person person = _repo.People.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person.HomeLocation as GeoLocation);
        }

        [EnableQuery]
        [HttpGet("People({id})/OrderInfo")]
        public IActionResult GetOrdeInfoFromPerson([FromODataUri]int id)
        {
            return Ok(_repo.People.FirstOrDefault(p => p.Id == id).OrderInfo);
        }

        [HttpGet("People({id})/HomeLocation/ZipCode")]
        public IActionResult GetZipCode([FromODataUri]int id)
        {
            Person person = _repo.People.FirstOrDefault(p => p.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person.HomeLocation.ZipCode);
        }

        [HttpPost("People({id})/HomeLocation/ZipCode/$ref")]
        public IActionResult CreateRefToZipCode([FromODataUri] int id, [FromBody] ZipCode zip)
        {
            return Ok(zip);
        }
    }
}
