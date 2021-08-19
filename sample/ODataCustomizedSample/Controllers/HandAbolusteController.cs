//-----------------------------------------------------------------------------
// <copyright file="HandAbolusteController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataCustomizedSample.Controllers
{
    [Route("convention")]
    [Route("explicit")]
    [Route("odata")]
    public class HandAbolusteController : ControllerBase
    {
        [ODataAttributeRouting]
        [HttpGet("/explicit/Employees({key})/Goto(lat={lat},lon={lon})")]
        [HttpGet("~/convention/Employees({key})/ODataCustomizedSample.Models.Goto(lat={lat},lon={lon})")]
        public string Goto(int key, double lat, double lon)
        {
            return $"Move Employees({key}) to location at ({lat},{lon})";
        }
    }
}
