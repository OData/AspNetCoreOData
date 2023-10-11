//-----------------------------------------------------------------------------
// <copyright file="DeltaTokenControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken
{
    public class TestCustomersController : ODataController
    {
        public IActionResult Get()
        {
            IEdmModel model = Request.GetModel();

            IEdmEntityType customerType = model.FindDeclaredType("Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestCustomer") as IEdmEntityType;
            IEdmEntityType customerWithAddressType = model.FindDeclaredType("Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestCustomerWithAddress") as IEdmEntityType;
            IEdmComplexType addressType = model.FindDeclaredType("Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestAddress") as IEdmComplexType;
            IEdmEntityType orderType = model.FindDeclaredType("Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestOrder") as IEdmEntityType;
            IEdmEntitySet ordersSet = model.FindDeclaredEntitySet("TestOrders") as IEdmEntitySet;
            EdmChangedObjectCollection changedObjects = new EdmChangedObjectCollection(customerType);

            EdmDeltaComplexObject a = new EdmDeltaComplexObject(addressType);
            a.TrySetPropertyValue("State", "State");
            a.TrySetPropertyValue("ZipCode", null);

            EdmDeltaResourceObject changedEntity = new EdmDeltaResourceObject(customerWithAddressType);
            changedEntity.TrySetPropertyValue("Id", 1);
            changedEntity.TrySetPropertyValue("Name", "Name");
            changedEntity.TrySetPropertyValue("Address", a);
            changedEntity.TrySetPropertyValue("PhoneNumbers", new List<string> { "123-4567", "765-4321" });
            changedEntity.TrySetPropertyValue("OpenProperty", 10);
            changedEntity.TrySetPropertyValue("NullOpenProperty", null);
            changedObjects.Add(changedEntity);

            EdmComplexObjectCollection places = new EdmComplexObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(addressType, true))));
            EdmDeltaComplexObject b = new EdmDeltaComplexObject(addressType);
            b.TrySetPropertyValue("City", "City2");
            b.TrySetPropertyValue("State", "State2");
            b.TrySetPropertyValue("ZipCode", 12345);
            b.TrySetPropertyValue("OpenProperty", 10);
            b.TrySetPropertyValue("NullOpenProperty", null);
            places.Add(a);
            places.Add(b);

            var newCustomer = new EdmDeltaResourceObject(customerType);
            newCustomer.TrySetPropertyValue("Id", 10);
            newCustomer.TrySetPropertyValue("Name", "NewCustomer");
            newCustomer.TrySetPropertyValue("FavoritePlaces", places);
            changedObjects.Add(newCustomer);

            var newOrder = new EdmDeltaResourceObject(orderType);
            newOrder.NavigationSource = ordersSet;
            newOrder.TrySetPropertyValue("Id", 27);
            newOrder.TrySetPropertyValue("Amount", 100);
            changedObjects.Add(newOrder);

            var deletedCustomer = new EdmDeltaDeletedResourceObject(customerType);
            deletedCustomer.Id = new Uri("7", UriKind.RelativeOrAbsolute);
            deletedCustomer.Reason = DeltaDeletedEntryReason.Changed;
            changedObjects.Add(deletedCustomer);

            var deletedOrder = new EdmDeltaDeletedResourceObject(orderType);
            deletedOrder.NavigationSource = ordersSet;
            deletedOrder.Id = new Uri("12", UriKind.RelativeOrAbsolute);
            deletedOrder.Reason = DeltaDeletedEntryReason.Deleted;
            changedObjects.Add(deletedOrder);

            var deletedLink = new EdmDeltaDeletedLink(customerType);
            deletedLink.Source = new Uri("http://localhost/odata/TestCustomers(1)");
            deletedLink.Target = new Uri("http://localhost/odata/TestOrders(12)");
            deletedLink.Relationship = "Orders";
            changedObjects.Add(deletedLink);

            var addedLink = new EdmDeltaLink(customerType);
            addedLink.Source = new Uri("http://localhost/odata/TestCustomers(10)");
            addedLink.Target = new Uri("http://localhost/odata/TestOrders(27)");
            addedLink.Relationship = "Orders";
            changedObjects.Add(addedLink);

            return Ok(changedObjects);
        }
    }

    public class TestOrdersController : ODataController
    {
        public IActionResult Get()
        {
            IEdmModel model = Request.GetModel();
            IEdmComplexType addressType = model.FindDeclaredType("Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestAddress") as IEdmComplexType;
            IEdmEntityType orderType = model.FindDeclaredType("Microsoft.AspNetCore.OData.E2E.Tests.DeltaToken.TestOrder") as IEdmEntityType;
            EdmChangedObjectCollection changedObjects = new EdmChangedObjectCollection(orderType);

            EdmDeltaComplexObject sampleList = new EdmDeltaComplexObject(addressType);
            sampleList.TrySetPropertyValue("State", "sample state");
            sampleList.TrySetPropertyValue("ZipCode", 9);
            sampleList.TrySetPropertyValue("title", "sample title"); // primitive dynamic

            EdmDeltaComplexObject location = new EdmDeltaComplexObject(addressType);
            location.TrySetPropertyValue("State", "State");
            location.TrySetPropertyValue("ZipCode", null);
            location.TrySetPropertyValue("OpenProperty", 10); // primitive dynamic
            location.TrySetPropertyValue("key-samplelist", sampleList); // complex dynamic

            EdmDeltaResourceObject changedOrder = new EdmDeltaResourceObject(orderType);
            changedOrder.TrySetPropertyValue("Id", 1);
            changedOrder.TrySetPropertyValue("Amount", 42);
            changedOrder.TrySetPropertyValue("Location", location);
            changedObjects.Add(changedOrder);

            return Ok(changedObjects);
        }
    }
}
