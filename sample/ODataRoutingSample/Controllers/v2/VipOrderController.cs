//-----------------------------------------------------------------------------
// <copyright file="VipOrderController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    [ODataRouteComponent("v2{data}")]
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
