// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    [ODataModel("v2{data}")]
    public class VipOrderController : ControllerBase
    {
        [HttpGet]
        public Order Get()
        {
            return new Order { Id = 9, Title = "Singleton Title" };
        }

        [HttpGet]
        public string GetTitleFromOrder()
        {
            return "Singleton Title";
        }
    }
}
