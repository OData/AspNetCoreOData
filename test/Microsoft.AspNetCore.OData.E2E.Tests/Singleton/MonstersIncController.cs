﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Singleton
{
    /// <summary>
    /// Present a singleton named "MonstersInc"
    /// Use attribute routing
    /// </summary>
    [Route("odata/MonstersInc")]
    public class MonstersIncController : ODataController
    {
        public static Company MonstersInc;

        static MonstersIncController()
        {
            InitData();
        }

        private static void InitData()
        {
            MonstersInc = new Company()
            {
                ID = 1,
                Name = "MonstersInc",
                Revenue = 1000,
                Category = CompanyCategory.Electronics,
                Partners = new List<Partner>(),
                Branches = new List<Office>() { new Office { City = "Shanghai", Address = "Minhang" }, new Office { City = "Xi'an", Address = "Dayanta" } },
            };
        }

        #region Query
        [EnableQuery]
        [HttpGet("")]
        public IActionResult QueryCompany()
        {
            return Ok(MonstersInc);
        }

        [HttpGet("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany")]
        public IActionResult QueryCompanyFromDerivedType()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany);
            }
            return BadRequest("The target cannot be casted");
        }

        [HttpGet("Revenue")]
        public IActionResult GetCompanyRevenue()
        {
            return Ok(MonstersInc.Revenue);
        }

        [HttpGet("Branches/$count")]
        public IActionResult GetBranchesCount(ODataQueryOptions<Office> options)
        {
            IQueryable<Office> eligibleBranches = MonstersInc.Branches.AsQueryable();
            if (options.Filter != null)
            {
                eligibleBranches = options.Filter.ApplyTo(eligibleBranches, new ODataQuerySettings()).Cast<Office>();
            }
            return Ok(eligibleBranches.Count());
        }

        [HttpGet("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany/Location")]
        public IActionResult GetDerivedTypeProperty()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Location);
            }
            return BadRequest("The target cannot be casted");
        }

        [HttpGet("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany/Office")]
        public IActionResult QueryDerivedTypeComplexProperty()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Office);
            }
            return BadRequest("The target cannot be casted");
        }

        [HttpGet("Partners")]
        public IActionResult QueryNavigationProperty()
        {
            return Ok(MonstersInc.Partners);
        }
        #endregion

        #region Update
        [HttpPut("")]
        public IActionResult UpdateCompanyByPut([FromBody] Company newCompany)
        {
            MonstersInc = newCompany;
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPut("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany")]
        public IActionResult UpdateCompanyByPutWithDerivedTypeObject([FromBody] SubCompany newCompany)
        {
            MonstersInc = newCompany;
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPatch("")]
        public IActionResult UpdateCompanyByPatch([FromBody]Delta<Company> item)
        {
            item.Patch(MonstersInc);
            return StatusCode(StatusCodes.Status204NoContent);
        }
        #endregion

        #region Navigation link
        [HttpPost("Partners/$ref")]
        public IActionResult AddOrUpdateNavigationLink([FromBody] Uri link)
        {
            int relatedKey = Request.GetKeyValue<int>(link);
            Partner partner = PartnersController.Partners.First(x => x.ID == relatedKey);
            MonstersInc.Partners.Add(partner);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("Partners({relatedKey})/$ref")]
        public IActionResult DeleteNavigationLink(string relatedKey)
        {
            int key = int.Parse(relatedKey);
            Partner partner = MonstersInc.Partners.First(x => x.ID == key);

            MonstersInc.Partners.Remove(partner);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet("Partners/$ref")]
        public IActionResult GetNavigationLink()
        {
            return Ok();
        }

        [HttpPost("Partners")]
        public IActionResult AddPartnersToCompany([FromBody] Partner partner)
        {
            PartnersController.Partners.Add(partner);
            if (MonstersInc.Partners == null)
            {
                MonstersInc.Partners = new List<Partner>() { partner };
            }
            else
            {
                MonstersInc.Partners.Add(partner);
            }

            return Created(partner);
        }
        #endregion

        #region Action and function
        [HttpPost("Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource")]
        public IActionResult CallActionResetDataSource()
        {
            InitData();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpGet("Microsoft.Test.E2E.AspNet.OData.Singleton.GetPartnersCount()")]
        public IActionResult CallFunctionGetPartnersCount()
        {
            return Ok(MonstersInc.Partners.Count);
        }
        #endregion
    }
}
