// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags
{
    public class ETagUntypedCustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get(int key)
        {
            IEdmModel model = Request.GetModel();
            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            EdmEntityObject customer = new EdmEntityObject(entityType);
            customer.TrySetPropertyValue("ID", key);
            customer.TrySetPropertyValue("Name", "Sam");
            return Ok(customer);
        }
    }
}