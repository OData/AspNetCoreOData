// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
