//-----------------------------------------------------------------------------
// <copyright file="PeopleController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataAlternateKeySample.Models;

namespace ODataAlternateKeySample.Controllers
{
    public class PeopleController : ODataController
    {
        private readonly IAlternateKeyRepository _repository;

        public PeopleController(IAlternateKeyRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_repository.GetPeople());
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(int key)
        {
            var c = _repository.GetPeople().FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }

        [HttpGet("odata/People(c_or_r={cr},passport={passport})")]
        public IActionResult FindPeopleByCountryAndPassport(string cr, string passport)
        {
            var c = _repository.GetPeople().FirstOrDefault(c => c.CountryOrRegion == cr && c.Passport == passport);
            if (c == null)
            {
                return NotFound();
            }

            return Ok(c);
        }
    }
}
