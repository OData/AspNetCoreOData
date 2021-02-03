// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataModel("v1")]
    public class OrganizationsController : Controller
    {
        [HttpGet]
        public IActionResult GetPrice([FromODataUri]string organizationId, [FromODataUri] string partId)
        {
            return Ok($"Caculated the price using {organizationId} and {partId}");
        }

        [HttpGet]
        [ODataRoute("Organizations/GetPrice2(organizationId={orgId},partId={parId})")]
        public IActionResult GetMorePrice(string orgId, string parId)
        {
            return Ok($"Caculated the price using {orgId} and {parId}");
        }

        [HttpGet]
        [ODataRoute("Organizations/GetPrice2(organizationId={orgId},partId={parId})/GetPrice2(organizationId={orgId2},partId={parId2})")]
        public IActionResult GetMorePrice2(string orgId, string parId, string orgId2, string parId2)
        {
            return Ok($"Caculated the price using {orgId} and {parId} | using {orgId2} and {parId2}");
        }
    }
}
