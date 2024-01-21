//-----------------------------------------------------------------------------
// <copyright file="fooTemplatesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataRoutingSample.Controllers.v1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Query;
    using Microsoft.AspNetCore.OData.Routing.Attributes;
    using Microsoft.AspNetCore.OData.Routing.Controllers;
    using ODataRoutingSample.Models;

    [ODataRouteComponent("v1")]
    public class fooTemplatesController : ODataController
    {
        private readonly FooDemoData fooDemoData;

        public fooTemplatesController(FooDemoData fooDemoData)
        {
            this.fooDemoData = fooDemoData;
        }

        [HttpPost]
        [EnableQuery]
        public IActionResult Post([FromBody] FooTemplate fooTemplate)
        {
            SetHeaderToIncludeInstanceAnnotations(base.Request.Headers);
            
            var id = Guid.NewGuid().ToString();
            fooTemplate.Id = id;

            this.fooDemoData.FooTemplates[id] = fooTemplate;

            return Created(fooTemplate);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            SetHeaderToIncludeInstanceAnnotations(base.Request.Headers);

            return Ok(this.fooDemoData.FooTemplates.Values);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get(string key)
        {
            SetHeaderToIncludeInstanceAnnotations(base.Request.Headers);

            if (!this.fooDemoData.FooTemplates.TryGetValue(key, out var fooTemplate))
            {
                return NotFound();
            }

            return Ok(fooTemplate);
        }

        private static void SetHeaderToIncludeInstanceAnnotations(IHeaderDictionary headers)
        {
            if (headers.TryGetValue("Prefer", out var stringValues))
            {
                var values = new List<string>();
                var foundIncludeAnnotations = false;
                foreach (var stringValue in stringValues)
                {
                    if (stringValue.StartsWith("odata.include-annotations=", StringComparison.OrdinalIgnoreCase))
                    {
                        foundIncludeAnnotations = true;
                        values.Add(stringValue + ",microsoft.notProvided");
                    }
                }

                if (!foundIncludeAnnotations)
                {
                    values.Add("odata.include-annotations=microsoft.notProvided");
                }

                stringValues = new Microsoft.Extensions.Primitives.StringValues(values.ToArray());
            }
            else
            {
                stringValues = new Microsoft.Extensions.Primitives.StringValues("odata.include-annotations=microsoft.notProvided");
            }

            headers["Prefer"] = stringValues;
        }
    }
}
