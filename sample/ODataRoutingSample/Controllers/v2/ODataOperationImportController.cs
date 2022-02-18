//-----------------------------------------------------------------------------
// <copyright file="ODataOperationImportController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataRoutingSample.Controllers.v2
{
    [ODataRouteComponent("v2{data}")]
    public class ODataOperationImportController : ControllerBase
    {
        [HttpGet]
        public int RateByOrder(int order)
        {
            return order;
        }
    }
}
