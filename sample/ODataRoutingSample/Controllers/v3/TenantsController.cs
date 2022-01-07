//-----------------------------------------------------------------------------
// <copyright file="TenantsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v3
{
    public class TestEntitiesController : Controller
    {
        /* HttpPatch http://localhost:5000/v3/TestEntities/1/Query
         * 
         * Request Body is: 
{
    "results": [
        {
            "EmailClusterId@odata.type": "#Int64",
            "EmailClusterId": 2629759514
        },
        {
            "EmailClusterId@odata.type": "#Int64",
            "EmailClusterId": 2629759515
        }
    ]
}
         */
        [HttpPatch]
        public IActionResult PatchToQuery(int key, Delta<HuntingQueryResults> delta)
        {
            var changedPropertyNames = delta.GetChangedPropertyNames();

            HuntingQueryResults original = new HuntingQueryResults();
            delta.Patch(original);

            return Ok(key);
        }
    }

    [ODataAttributeRouting]
    [Route("v3")]
    public class tenantsController : Controller
    {
        [HttpPut("tenants/{tenantId}/devices/{deviceId}")]
        [HttpPut("tenants({tenantId})/devices({deviceId})")]
        public IActionResult PutToDevices(string tenantId, string deviceId)
        {
            return Ok($"PutTo Devices - tenantId={tenantId}: deviceId={deviceId}");
        }

        [HttpGet("tenants/{tenantId}/folders/{folderId}")]
        [HttpGet("tenants({tenantId})/folders({folderId})")]
        public IActionResult GetFolders(string tenantId, Guid folderId)
        {
            return Ok($"GetFolders - tenantId={tenantId}: folderId={folderId}");
        }

        [HttpGet("tenants/{tenantId}/pages/{pageId}")]
        [HttpGet("tenants({tenantId})/pages({pageId})")]
        public IActionResult GetDriverPages(string tenantId, int pageId)
        {
            // Example:
            // 1) ~/v3/tenants/23281137-7a37-4c2f-ad57-a511f38dea09/pages/2  ==> works
            // 2) ~/v3/tenants/'23281137-7a37-4c2f-ad57-a511f38dea09'/pages/2  ==> works
            // 3) ~/v3/tenants/'23281137-7a37-4c2f-ad57-a511f38dea09'/pages/ab  ==> throw exception on 'ab'
            return Ok($"GetDriverPages - tenantId={tenantId}: pageId={pageId}");
        }
    }
}
