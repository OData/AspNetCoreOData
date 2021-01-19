// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataModel("v1")]
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
