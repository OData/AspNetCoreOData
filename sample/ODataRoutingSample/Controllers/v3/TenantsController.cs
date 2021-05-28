// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataRoutingSample.Controllers.v3
{
    [ODataRouting]
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
