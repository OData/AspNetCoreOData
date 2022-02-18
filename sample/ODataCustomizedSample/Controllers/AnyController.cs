//-----------------------------------------------------------------------------
// <copyright file="AnyController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataCustomizedSample.Controllers
{
    [Route("")]
    [ODataAttributeRouting]
    public class AnyController : ControllerBase
    {
        [HttpGet("Players")] // ~Players
        public IActionResult DoAnything()
        {
            return Ok("DoAnything");
        }

        // Use Absolute route template
        [HttpGet("/v{version}/Players")] // v{version}/Players
        public IActionResult DoAnything(string version)
        {
            return Ok($"DoAnything at version={version}");
        }

        // Use Absolute route template
        [HttpGet("/v{version}/Players/{playerKey}/Default.PlayPiano(kind={kind},name={name})")]
        [HttpGet("/v{version}/Players/{playerKey}/PlayPiano(kind={kind},name={name})")]
        public string DoPlayPiano(int playerKey, int kind, string name)
        {
             return $"Players{playerKey} do Play Piano (kind={kind},name={name}";
        }
    }
}
