//-----------------------------------------------------------------------------
// <copyright file="foosController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataRoutingSample.Controllers.v1
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Formatter;
    using Microsoft.AspNetCore.OData.Query;
    using Microsoft.AspNetCore.OData.Routing.Attributes;
    using Microsoft.AspNetCore.OData.Routing.Controllers;
    using ODataRoutingSample.Models;

    [ODataRouteComponent("v1")]
    public class foosController : ODataController
    {
        private readonly FooDemoData fooDemoData;

        public foosController(FooDemoData fooDemoData)
        {
            this.fooDemoData = fooDemoData;
        }

        [HttpPost]
        [EnableQuery]
        public IActionResult Post([FromBody] FooProperties fooProperties)
        {
            var foo = CreateNewFoo(fooProperties);

            return Created(foo);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(this.fooDemoData.Foos.Values);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(string key)
        {
            if (!this.fooDemoData.Foos.TryGetValue(key, out var foo))
            {
                return NotFound();
            }

            return Ok(foo);
        }

        [HttpPost]
        [EnableQuery]
        public IActionResult create(ODataActionParameters parameters)
        {
            if (!parameters.TryGetValue("template", out var template) || template is not FooTemplate fooTemplateNavigation)
            {
                return BadRequest("The 'template' parameter is required for the 'create' action.");
            }

            if (!this.fooDemoData.FooTemplates.TryGetValue(fooTemplateNavigation.Id, out var fooTemplate))
            {
                return NotFound($"Could not find the 'fooTemplate' with ID '{fooTemplateNavigation.Id}'");
            }

            var fooProperties = new FooProperties()
            {
                Fizz = fooTemplate.Fizz,
                FizzProvided = fooTemplate.FizzProvided,
                Buzz = fooTemplate.Buzz,
                BuzzProvided = fooTemplate.BuzzProvided,
            };
            var foo = CreateNewFoo(fooProperties);

            return Created(foo);
        }

        private Foo CreateNewFoo(FooProperties fooProperties)
        {
            var id = Guid.NewGuid().ToString();
            var foo = new Foo()
            {
                Id = id,
                Fizz = fooProperties.FizzProvided ? fooProperties.Fizz : DefaultFizz(),
                Buzz = fooProperties.BuzzProvided ? fooProperties.Buzz : DefaultBuzz(),
                Frob = fooProperties.FrobProvided ? fooProperties.Frob : DefaultFrob(),
            };

            this.fooDemoData.Foos[id] = foo;

            return foo;
        }

        private static Fizz DefaultFizz()
        {
            return new Fizz();
        }

        private static Buzz DefaultBuzz()
        {
            return null;
        }

        private static Frob DefaultFrob()
        {
            return new Frob();
        }
    }
}
