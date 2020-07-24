// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataModel("v1")]
    public class ODataOperationImportController : ControllerBase
    {
        [HttpGet]
        public int RateByOrder(int order)
        {
            return order;
        }
    }
}
