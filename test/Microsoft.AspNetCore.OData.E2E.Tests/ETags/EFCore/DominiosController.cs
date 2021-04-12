// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags.EFCore
{
    public class DominiosController : ODataController
    {
        private ETagCurrencyTokenEfContext _db;

        public DominiosController(ETagCurrencyTokenEfContext context)
        {
            _db = context;
            ETagCurrencyTokenEfContextInitializer.Seed(_db);
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_db.Dominios);
        }
    }
}
