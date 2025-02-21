//-----------------------------------------------------------------------------
// <copyright file="TypelessDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

internal static class TypelessDataSource
{
    private readonly static EdmChangedObjectCollection typelessChangeSets;
    private readonly static EdmEntityObjectCollection typelessOrders;
    private readonly static DeltaSet<ChangeSet> typedChangeSets;

    static TypelessDataSource()
    {
        typelessChangeSets = CreateTypelessChangeSets();
        typelessOrders = CreateOrders();
        typedChangeSets = CreateTypedChangedSets();
    }

    private static EdmChangedObjectCollection CreateTypelessChangeSets()
    {
        var changeSets = new EdmChangedObjectCollection(TypelessEdmModel.ChangeSetEntityType);

        var changeSetObject1 = new EdmEntityObject(TypelessEdmModel.ChangeSetEntityType);
        changeSetObject1.TrySetPropertyValue("Id", 1);
        changeSetObject1.TrySetPropertyValue("Changed", CreateTypelessChangedCustomer());
        changeSets.Add(changeSetObject1);

        var changeSetObject2 = new EdmEntityObject(TypelessEdmModel.ChangeSetEntityType);
        changeSetObject2.TrySetPropertyValue("Id", 2);
        changeSetObject2.TrySetPropertyValue("Changed", CreateTypelessChangedOrder());
        changeSets.Add(changeSetObject2);

        return changeSets;
    }

    private static DeltaSet<ChangeSet> CreateTypedChangedSets()
    {
        var changeSetDeltaSet = new DeltaSet<ChangeSet>();

        var changeSetDeltaItem1 = new Delta<ChangeSet>();
        changeSetDeltaItem1.TrySetPropertyValue("Id", 1);
        changeSetDeltaItem1.TrySetPropertyValue("Changed", CreateTypedChangedCustomer());
        changeSetDeltaSet.Add(changeSetDeltaItem1);

        var changeSetDeltaItem2 = new Delta<ChangeSet>();
        changeSetDeltaItem2.TrySetPropertyValue("Id", 2);
        changeSetDeltaItem2.TrySetPropertyValue("Changed", CreateTypedChangedOrder());
        changeSetDeltaSet.Add(changeSetDeltaItem2);

        return changeSetDeltaSet;
    }

    private static EdmDeltaResourceObject CreateTypelessChangedCustomer()
    {
        var changedOrders = new EdmChangedObjectCollection(
            EdmCoreModel.Instance.GetEntityType());

        var changeOrder1Object = new EdmDeltaResourceObject(TypelessEdmModel.OrderEntityType);
        changeOrder1Object.TrySetPropertyValue("Id", 1);
        changedOrders.Add(changeOrder1Object);

        var changedOrder2Object = new EdmDeltaDeletedResourceObject(TypelessEdmModel.OrderEntityType);
        changedOrder2Object.Id = new Uri("http://tempuri.org/Orders(2)");
        changedOrder2Object.TrySetPropertyValue("Id", 2);
        changedOrders.Add(changedOrder2Object);

        var changedCustomerObject = new EdmDeltaResourceObject(TypelessEdmModel.CustomerEntityType);
        changedCustomerObject.TrySetPropertyValue("Id", 1);
        changedCustomerObject.TrySetPropertyValue("Orders", changedOrders);

        return changedCustomerObject;
    }

    private static EdmDeltaResourceObject CreateTypelessChangedOrder()
    {
        var changedOrderObject = new EdmDeltaResourceObject(TypelessEdmModel.OrderEntityType);
        changedOrderObject.TrySetPropertyValue("Id", 1);
        changedOrderObject.TrySetPropertyValue("Amount", 310m);

        return changedOrderObject;
    }

    private static Delta<Customer> CreateTypedChangedCustomer()
    {
        var ordersDeltaSet = new DeltaSet<Order>();

        var order1DeltaObject = new Delta<Order>();
        order1DeltaObject.TrySetPropertyValue("Id", 1);
        ordersDeltaSet.Add(order1DeltaObject);

        var order2DeltaObject = new DeltaDeletedResource<Order>();
        order2DeltaObject.Id = new Uri("http://tempuri.org/Orders(2)");
        order2DeltaObject.TrySetPropertyValue("Id", 2);
        ordersDeltaSet.Add(order2DeltaObject);

        var customerDeltaObject = new Delta<Customer>();
        customerDeltaObject.TrySetPropertyValue("Id", 1);
        customerDeltaObject.TrySetPropertyValue("Orders", ordersDeltaSet);

        return customerDeltaObject;
    }

    private static Delta<Order> CreateTypedChangedOrder()
    {
        var orderDeltaObject = new Delta<Order>();
        orderDeltaObject.TrySetPropertyValue("Id", 1);
        orderDeltaObject.TrySetPropertyValue("Amount", 310m);

        return orderDeltaObject;
    }

    private static EdmEntityObjectCollection CreateOrders()
    {
        var customer1Object = new EdmEntityObject(TypelessEdmModel.CustomerEntityType);
        customer1Object.TrySetPropertyValue("Id", 1);
        customer1Object.TrySetPropertyValue("Name", "Sue");
        customer1Object.TrySetPropertyValue("CreditLimit", 1300m);

        var customer2Object = new EdmEntityObject(TypelessEdmModel.CustomerEntityType);
        customer2Object.TrySetPropertyValue("Id", 2);
        customer2Object.TrySetPropertyValue("Name", "Joe");
        customer2Object.TrySetPropertyValue("CreditLimit", 1700m);

        var order1Object = new EdmEntityObject(TypelessEdmModel.OrderEntityType);
        order1Object.TrySetPropertyValue("Id", 1);
        order1Object.TrySetPropertyValue("Amount", 310m);
        order1Object.TrySetPropertyValue("OrderDate", new DateTimeOffset(2025, 02, 7, 11, 59, 59, TimeSpan.Zero));
        order1Object.TrySetPropertyValue("Customer", customer1Object);

        var order2Object = new EdmEntityObject(TypelessEdmModel.OrderEntityType);
        order2Object.TrySetPropertyValue("Id", 2);
        order2Object.TrySetPropertyValue("Amount", 290m);
        order2Object.TrySetPropertyValue("OrderDate", new DateTimeOffset(2025, 02, 14, 11, 59, 59, TimeSpan.Zero));
        order2Object.TrySetPropertyValue("Customer", customer2Object);

        return new EdmEntityObjectCollection(
            new EdmCollectionTypeReference(
                new EdmCollectionType(
                    new EdmEntityTypeReference(TypelessEdmModel.OrderEntityType, false))))
        {
            order1Object,
            order2Object
        };
    }

    public static EdmChangedObjectCollection TypelessChangeSets => typelessChangeSets;
    
    public static DeltaSet<ChangeSet> TypedChangeSets => typedChangeSets;
    
    public static EdmEntityObjectCollection TypelessOrders => typelessOrders;
}
