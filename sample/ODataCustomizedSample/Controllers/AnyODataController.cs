//-----------------------------------------------------------------------------
// <copyright file="AnyODataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ODataCustomizedSample.Controllers
{
    public class AnyODataController : ODataController
    {
        [HttpGet("Players/$count")] // ~/players/$count
        [Route("Players")] // It will combine as two routes: [Post,Patch] ~/Players
        [HttpPost]
        [HttpPatch]
        public IActionResult Dosomething()
        {
            return Ok("Dosomething on Players -> AnyODataController");
        }

        [HttpGet("v{version}/Players/{key}/PlayPiano(kind={k},name={n})")]
        public IActionResult LetsPlayPiano(string version, int key, int k, string n)
        {
            string output = $"Players {key} are playing piano with kind={k} and name={n}, version={version} -> AnyODataController";
            return Ok(output);
        }
    }
}
