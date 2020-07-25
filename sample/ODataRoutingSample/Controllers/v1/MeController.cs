// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
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

        [HttpGet]
        public VipCustomer GetMeFromVipCustomer()
        {
            return new VipCustomer { Id = 10, Name = "Peter", Emails = new List<string> { "abc@ef.com" } };
        }
    }
}
