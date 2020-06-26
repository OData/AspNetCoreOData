// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataModel("v1")]
    public class MeController : ControllerBase
    {
        [HttpGet]
        public Customer Get()
        {
            return new Customer { Id = 9, Name = "Sam" };
        }
    }
}
