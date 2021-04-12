// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataCustomizedSample.Controllers
{
    [Route("convention")]
    [Route("explicit")]
    [Route("odata")]
    public class HandAbolusteController : ControllerBase
    {
        [ODataRouting]
        [HttpGet("/explicit/Employees({key})/Goto(lat={lat},lon={lon})")]
        [HttpGet("~/convention/Employees({key})/ODataCustomizedSample.Models.Goto(lat={lat},lon={lon})")]
        public string Goto(int key, double lat, double lon)
        {
            return $"Move Employees({key}) to location at ({lat},{lon})";
        }
    }
}
