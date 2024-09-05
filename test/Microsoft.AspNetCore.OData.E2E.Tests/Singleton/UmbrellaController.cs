//-----------------------------------------------------------------------------
// <copyright file="UmbrellaController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Singleton;

/// <summary>
/// Present a singleton named "Umbrella"
/// Use convention routing
/// </summary>
public class UmbrellaController : ODataController
{
    public static Company Umbrella;

    static UmbrellaController()
    {
        InitData();
    }

    private static void InitData()
    {
        Umbrella = new Company()
        {
            ID = 1,
            Name = "Umbrella",
            Revenue = 1000,
            Category = CompanyCategory.Communication,
            Partners = new List<Partner>(),
            Branches = new List<Office>(),
            Projects = new List<Project>(),
        };
    }

    #region Query
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(Umbrella);
    }

    public IActionResult GetFromSubCompany()
    {
        var subCompany = Umbrella as SubCompany;
        if (subCompany != null)
        {
            return Ok(subCompany);
        }
        return BadRequest();
    }

    public IActionResult GetRevenueFromCompany()
    {
        return Ok(Umbrella.Revenue);
    }

    public IActionResult GetNameFromCompany()
    {
        return Ok(Umbrella.Name);
    }

    public IActionResult GetCategoryFromCompany()
    {
        return Ok(Umbrella.Category);
    }

    public IActionResult GetLocationFromSubCompany()
    {
        var subCompany = Umbrella as SubCompany;
        if (subCompany != null)
        {
            return Ok(subCompany.Location);
        }
        return BadRequest();
    }

    public IActionResult GetOffice()
    {
        var subCompany = Umbrella as SubCompany;
        if (subCompany != null)
        {
            return Ok(subCompany.Office);
        }
        return BadRequest();
    }

    [EnableQuery]
    public IActionResult GetPartnersFromCompany()
    {
        return Ok(Umbrella.Partners);
    }
    #endregion

    #region Update
    public IActionResult Put([FromBody]Company newCompany)
    {
        Umbrella = newCompany;
        return StatusCode(StatusCodes.Status204NoContent);
    }

    public IActionResult PutUmbrellaFromSubCompany([FromBody]SubCompany newCompany)
    {
        Umbrella = newCompany;
        return StatusCode(StatusCodes.Status204NoContent);
    }

    public IActionResult Patch([FromBody]Delta<Company> item)
    {
        item.Patch(Umbrella);
        return StatusCode(StatusCodes.Status204NoContent);
    }
    #endregion

    #region Navigation link
    [AcceptVerbs("POST")]
    public IActionResult CreateRef(string navigationProperty, [FromBody] Uri link)
    {
        int relatedKey = Request.GetKeyValue<int>(link);
        Partner partner = PartnersController.Partners.First(x => x.ID == relatedKey);

        if (navigationProperty != "Partners" || partner == null)
        {
            return BadRequest();
        }

        Umbrella.Partners.Add(partner);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [AcceptVerbs("DELETE")]
    public IActionResult DeleteRef(string relatedKey, string navigationProperty)
    {
        int key = int.Parse(relatedKey);
        Partner partner = Umbrella.Partners.First(x => x.ID == key);

        if (navigationProperty != "Partners")
        {
            return BadRequest();
        }

        Umbrella.Partners.Remove(partner);
        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpPost]
    public IActionResult PostToPartners([FromBody] Partner partner)
    {
        PartnersController.Partners.Add(partner);
        if (Umbrella.Partners == null)
        {
            Umbrella.Partners = new List<Partner>() { partner };
        }
        else
        {
            Umbrella.Partners.Add(partner);
        }

        return Created(partner);
    }
    #endregion

    #region Action and Function
    [HttpPost]
    public IActionResult ResetDataSourceOnCompany()
    {
        InitData();
        return StatusCode(StatusCodes.Status204NoContent);
    }

    public IActionResult GetPartnersCount()
    {
        return Ok(Umbrella.Partners.Count);
    }
    #endregion
}
