//-----------------------------------------------------------------------------
// <copyright file="PropertyNameCaseSensitiveControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped
{
    public class UntypedController : ODataController
    {
        [EnableQuery]
        [HttpGet("odata/people")]
        public IActionResult Get()
        {
            return Ok(UntypedDataSource.GetAllPeople());
        }

        [EnableQuery]
        [HttpGet("odata/people/{id}")]
        public IActionResult Get(int id)
        {
            InModelPerson person = UntypedDataSource.GetAllPeople().FirstOrDefault(p => p.Id == id);
            return Ok(person);
        }

        [EnableQuery]
        [HttpGet("odata/people/{id}/data")]
        public IActionResult GetData(int id)
        {
            InModelPerson person = UntypedDataSource.GetAllPeople().FirstOrDefault(p => p.Id == id);
            return Ok(person.Data);
        }

        [EnableQuery]
        [HttpGet("odata/people/{id}/infos")]
        public IActionResult GetInfos(int id)
        {
            InModelPerson person = UntypedDataSource.GetAllPeople().FirstOrDefault(p => p.Id == id);
            return Ok(person.Infos);
        }

        [EnableQuery]
        [HttpPost("odata/people")]
        public IActionResult Post([FromBody] InModelPerson person)
        {
            Assert.NotNull(person);

            return Ok(true);
        }

        [EnableQuery]
        public IActionResult Patch(int key, Delta<InModelPerson> delta)
        {
            Assert.Equal(2, key);

            return Ok(false);
        }
    }
}
