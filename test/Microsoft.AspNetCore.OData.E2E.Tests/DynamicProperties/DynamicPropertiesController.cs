//-----------------------------------------------------------------------------
// <copyright file="DynamicPropertiesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DynamicProperties;

[Route("odata")]
public class DynamicCustomersController : ODataController
{
    // Convention routing
    public IActionResult GetId(int key)
    {
        return Ok(string.Format("{0}_{1}", "Id", key));
    }

    [HttpGet("DynamicCustomers({key})/{property}")] // this is for generic property, but for "Id", we have the more specific route
    public IActionResult GetProperty(int key, string property)
    {
        return Ok($"GetProperty_{property}_{key}");
    }

    [Route("DynamicCustomers({key})/{dynamicproperty}")] // combined with [HttpGet, HttpPatch, HttpDelete]
    [HttpGet]
    [HttpPatch]
    [HttpDelete]
    [HttpGet("DynamicCustomers({key})/Microsoft.AspNetCore.OData.E2E.Tests.DynamicProperties.DynamicVipCustomer/{dynamicproperty}")]
    public IActionResult GetDynamicProperty(int key, string dynamicProperty)
    {
        return Ok(string.Format("{0}_{1}_{2}", dynamicProperty, "GetDynamicProperty", key));
    }

    [HttpGet("DynamicCustomers({key})/Account/{dynamicproperty}")]
    [HttpGet("DynamicCustomers({key})/Microsoft.AspNetCore.OData.E2E.Tests.DynamicProperties.DynamicVipCustomer/Account/{dynamicproperty}")]
    public IActionResult GetDynamicPropertyFromAccount([FromODataUri] int key, [FromODataUri] string dynamicProperty)
    {
        return Ok(string.Format("{0}_{1}_{2}", dynamicProperty, "GetDynamicPropertyFromAccount", key));
    }

    [HttpGet("DynamicCustomers({id})/Order/{dynamicproperty}")]
    public IActionResult GetDynamicPropertyFromOrder([FromODataUri] int id, [FromODataUri] string dynamicproperty)
    {
        return Ok(string.Format("{0}_{1}_{2}", dynamicproperty, "GetDynamicPropertyFromOrder", id));
    }
}

[Route("odata")]
public class DynamicSingleCustomerController : ODataController
{
    [HttpGet("DynamicSingleCustomer/{dynamicproperty}")]
    public IActionResult GetDynamicProperty(string dynamicProperty)
    {
        return Ok(string.Format("{0}_{1}", dynamicProperty, "GetDynamicProperty"));
    }

    [HttpGet("DynamicSingleCustomer/Account/{dynamicproperty}")]
    public IActionResult GetDynamicPropertyFromAccount([FromODataUri] string dynamicProperty)
    {
        return Ok(string.Format("{0}_{1}", dynamicProperty, "GetDynamicPropertyFromAccount"));
    }

    [HttpGet("DynamicSingleCustomer/Order/{dynamicproperty}")]
    public IActionResult GetDynamicPropertyFromOrder(string dynamicproperty)
    {
        return Ok(string.Format("{0}_{1}", dynamicproperty, "GetDynamicPropertyFromOrder"));
    }
}
