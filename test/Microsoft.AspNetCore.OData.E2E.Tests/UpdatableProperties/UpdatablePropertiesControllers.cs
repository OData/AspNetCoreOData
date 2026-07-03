//-----------------------------------------------------------------------------
// <copyright file="UpdatablePropertiesControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UpdatableProperties;

public class CustomersController : ODataController
{
    // Requests for this key trim the nested Address resource from the updatable set before applying
    // the delta; every other key applies the delta with the full set of properties.
    private const int RestrictedKey = 1;

    // Requests for this key trim the single-valued Order navigation property from the updatable set.
    private const int NavRestrictedKey = 3;

    // Requests for this key clear the updatable set and re-add only Name (allow-list style).
    private const int AllowListKey = 4;

    private static Customer GetCustomer(int key) => new Customer
    {
        Id = key,
        Name = "Original Name",
        Address = new Address { City = "Redmond", Street = "One Microsoft Way" },
        Order = new Order { Id = 100, Description = "Original Order", Amount = 100m }
    };

    private static void TrimUpdatableProperties(int key, Delta<Customer> delta)
    {
        switch (key)
        {
            case RestrictedKey:
                delta.UpdatableProperties.Remove(nameof(Customer.Address));
                break;

            case NavRestrictedKey:
                delta.UpdatableProperties.Remove(nameof(Customer.Order));
                break;

            case AllowListKey:
                delta.UpdatableProperties.Clear();
                delta.UpdatableProperties.Add(nameof(Customer.Name));
                break;
        }
    }

    [EnableQuery]
    public IActionResult Patch(int key, [FromBody] Delta<Customer> delta)
    {
        Customer customer = GetCustomer(key);

        TrimUpdatableProperties(key, delta);

        delta.Patch(customer);

        return Ok(customer);
    }

    [EnableQuery]
    public IActionResult Put(int key, [FromBody] Delta<Customer> delta)
    {
        Customer customer = GetCustomer(key);

        TrimUpdatableProperties(key, delta);

        delta.Put(customer);

        return Ok(customer);
    }
}
