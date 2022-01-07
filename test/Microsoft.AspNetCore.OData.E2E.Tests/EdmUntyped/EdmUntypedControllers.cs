//-----------------------------------------------------------------------------
// <copyright file="EdmUntypedControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EdmUntyped
{
    public class BillsController : ODataController
    {
        [EnableQuery]
        public IActionResult Post([FromBody]Bill bill)
        {
            Assert.NotNull(bill);

            // Let's verify the 'bill' from the "Post" test case
            Assert.Equal(921, bill.ID);
            Assert.Equal("Fan", bill.Name);
            Assert.Equal(Frequency.BiWeekly, bill.Frequency);
            Assert.Equal(3.14, bill.Weight);

            Assert.Equal(new Guid("21EC2020-3AEA-1069-A2DD-08002B30309D"), bill.ContactGuid);

            Assert.NotNull(bill.HomeAddress);
            Assert.Equal("MyStreet", bill.HomeAddress.Street);
            Assert.Equal("MyCity", bill.HomeAddress.City);

            Assert.NotNull(bill.Addresses);
            Assert.Equal(2, bill.Addresses.Count);

            Assert.Equal("Street-1", bill.Addresses[0].Street);
            Assert.Equal("City-1", bill.Addresses[0].City);

            Assert.Equal("Street-2", bill.Addresses[1].Street);
            Assert.Equal("City-2", bill.Addresses[1].City);

            return Ok(true);
        }

        [EnableQuery]
        public IActionResult Patch(int key, Delta<Bill> delta)
        {
            Assert.Equal(2, key);

            // Let's verify the 'delta' from the "Patch" test case
            Assert.True(delta.TryGetPropertyValue("Frequency", out object frequency));
            Assert.Equal(Frequency.BiWeekly, frequency);

            Assert.True(delta.TryGetPropertyValue("ContactGuid", out object contractGuid));
            Assert.Equal(new Guid("21EC2020-3AEA-1069-A2DD-08002B30309D"), contractGuid);

            Assert.True(delta.TryGetPropertyValue("Weight", out object weight));
            Assert.Equal(6.24, weight);

            Assert.True(delta.TryGetPropertyValue("HomeAddress", out object homeAddressObj));
            Delta<Address> homeAddress = Assert.IsType<Delta<Address>>(homeAddressObj);
            Address originalHomeAddress = new Address();
            homeAddress.Patch(originalHomeAddress);
            Assert.Equal("YouStreet", originalHomeAddress.Street);
            Assert.Equal("YouCity", originalHomeAddress.City);

            Assert.True(delta.TryGetPropertyValue("Addresses", out object addressesObj));
            Assert.Collection((IList<Address>)addressesObj,
                e =>
                {
                    Assert.Equal("Street-3", e.Street);
                    Assert.Equal("City-3", e.City);
                },
                e =>
                {
                    Assert.Equal("Street-4", e.Street);
                    Assert.Equal("City-4", e.City);
                });

            return Ok(false);
        }
    }
}
