// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace ODataCustomizedSample.Controllers
{
    public class GenericODataController : ControllerBase
    {
        [EnableQuery]
        public List<string> GetTest(string classname)
        {
            var y = new List<string> { $"classname={classname}", "Customer", "Car", "School" };

            return y;
        }
    }
}
