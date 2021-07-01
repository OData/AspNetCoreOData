// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Validation
{
    public class PatchCustomersController : Controller
    {
        [HttpPatch]
        public IActionResult Patch(int key, [FromBody]Delta<PatchCustomer> patch)
        {
            PatchCustomer c = new PatchCustomer() { Id = key, ExtraProperty = "Some value" };
            patch.Patch(c);
            TryValidateModel(c);

            if (ModelState.IsValid)
            {
                return Ok(c);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
