//-----------------------------------------------------------------------------
// <copyright file="PartnersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Singleton;

public class PartnersController : ODataController
{
    public static List<Partner> Partners;

    static PartnersController()
    {
        InitData();
    }

    private static void InitData()
    {
        Partners = Enumerable.Range(0, 10).Select(i =>
               new Partner()
               {
                   ID = i,
                   Name = string.Format("Name {0}", i)
               }).ToList();
    }

    #region Query
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(Partners.AsQueryable());
    }

    [EnableQuery]
    public IActionResult GetPartners()
    {
        return Ok(Partners.AsQueryable());
    }

    [EnableQuery]
    public IActionResult Get(int key)
    {
        return Ok(Partners.SingleOrDefault(p=>p.ID == key));
    }

    public IActionResult GetCompanyFromPartner([FromODataUri] int key)
    {
        var company = Partners.First(e => e.ID == key).Company;
        if (company == null)
        {
            return StatusCode(StatusCodes.Status204NoContent);
        }
        return Ok(company);
    }
    #endregion 

    #region Update
    public IActionResult POST([FromBody] Partner partner)
    {
        Partners.Add(partner);
        return Created(partner);
    }
    #endregion

    #region Navigation link
    [AcceptVerbs("PUT")]
    public IActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
    {
        if (navigationProperty != "Company")
        {
            return BadRequest();
        }

        var strArray = link.AbsoluteUri.Split('/');
        var company = strArray[strArray.Length - 1];

        if (company == "Umbrella")
        {
            Partners.First(e => e.ID == key).Company = UmbrellaController.Umbrella;
        }
        else if (company == "MonstersInc")
        {
            Partners.First(e => e.ID == key).Company = MonstersIncController.MonstersInc;
        }
        else
            return BadRequest();

        return StatusCode(StatusCodes.Status204NoContent);
    }

    public IActionResult DeleteRef([FromODataUri] int key, string navigationProperty)
    {
        if (navigationProperty != "Company")
        {
            return BadRequest();
        }

        Partners.First(e => e.ID == key).Company = null;
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPut("Partners({key})/Company")]
    public IActionResult PutToCompany(int key, [FromBody]Company company)
    {
        var navigateCompany = Partners.First(e => e.ID == key).Company;
        Partners.First(e => e.ID == key).Company = company;
        if (navigateCompany.Name == "Umbrella")
        {
            UmbrellaController.Umbrella = navigateCompany;
        }
        else
        {
            MonstersIncController.MonstersInc = navigateCompany;
        }
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPatch("Partners({key})/Company")]
    public IActionResult PatchToCompany(int key, Delta<Company> company)
    {
        var navigateCompany = Partners.First(e => e.ID == key).Company;
        company.Patch(Partners.First(e => e.ID == key).Company);
        if (navigateCompany.Name == "Umbrella")
        {
            company.Patch(UmbrellaController.Umbrella);
        }
        else
        {
            company.Patch(MonstersIncController.MonstersInc);
        }
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPost]
    public IActionResult PostToCompany(int key, [FromBody] Company company)
    {
        return Ok();
    }

    #endregion

    #region Action
    [HttpPost]
    public IActionResult ResetDataSourceOnCollectionOfPartner()
    {
        InitData();
        return StatusCode(StatusCodes.Status204NoContent);
    }
    #endregion
}
