//-----------------------------------------------------------------------------
// <copyright file="GenericODataController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
