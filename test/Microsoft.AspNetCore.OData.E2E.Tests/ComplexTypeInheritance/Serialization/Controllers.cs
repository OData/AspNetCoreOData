// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Serialization
{

    public class InheritanceCustomersController : ODataController
    {

        private readonly IList<InheritanceCustomer> _customers;

        public InheritanceCustomersController()
        {
            InheritanceAddress address = new InheritanceAddress
            {
                City = "Tokyo",
                Street = "Tokyo Rd"
            };

            InheritanceAddress usAddress = new InheritanceUsAddress
            {
                City = "Redmond",
                Street = "One Microsoft Way",
                ZipCode = 98052
            };

            InheritanceAddress cnAddress = new InheritanceCnAddress
            {
                City = "Shanghai",
                Street = "ZiXing Rd",
                PostCode = "200241"
            };

            _customers = Enumerable.Range(1, 5).Select(e =>
                new InheritanceCustomer
                {
                    Id = e,
                    Location = new InheritanceLocation
                    {
                        Name = "Location #" + e,
                        Address = e < 3 ? address : e < 4 ? usAddress : cnAddress
                    }
                }).ToList();
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_customers);
        }

    }

}
