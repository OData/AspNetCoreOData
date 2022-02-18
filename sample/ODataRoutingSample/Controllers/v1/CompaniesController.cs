//-----------------------------------------------------------------------------
// <copyright file="CompaniesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataRouteComponent("v1")]
    public class CompaniesController : ControllerBase
    {
        [HttpGet]
        [EnableQuery]
        public IEnumerable<Company> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Company
            {
                Id = (short)index
            })
            .ToArray();
        }

        [HttpGet]
        [EnableQuery]
        public Company Get(short key)
        {
            return new Company
            {
                Id = key
            };
        }
    }
}
