//-----------------------------------------------------------------------------
// <copyright file="TypelessEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

internal static class TypelessEdmModel
{
    const string typelessNamespace = "Microsoft.AspNetCore.OData.E2E.Tests.Typeless";
    const string defaultNamespace = "Default";

    private static readonly EdmModel model;
    private static readonly EdmEntityType deltaEntityType;
    private static readonly EdmEntityType changeSetEntityType;
    private static readonly EdmEntityType customerEntityType;
    private static readonly EdmEntityType orderEntityType;
    private static readonly EdmComplexType addressComplexType;

    static TypelessEdmModel()
    {
        model = new EdmModel();

        deltaEntityType = model.AddEntityType(typelessNamespace, "Delta");
        var deltaChangeIdProperty = deltaEntityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        deltaEntityType.AddKeys(deltaChangeIdProperty);

        changeSetEntityType = model.AddEntityType(typelessNamespace, "ChangeSet");
        var changeSetIdProperty = changeSetEntityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        changeSetEntityType.AddKeys(changeSetIdProperty);

        changeSetEntityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "Changed",
            Target = EdmCoreModel.Instance.GetEntityType(),
            TargetMultiplicity = EdmMultiplicity.One
        });

        addressComplexType = model.AddComplexType(typelessNamespace, "Address");
        addressComplexType.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
        addressComplexType.AddStructuralProperty("State", EdmPrimitiveTypeKind.String);

        orderEntityType = model.AddEntityType(typelessNamespace, "Order");
        var orderIdProperty = orderEntityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        orderEntityType.AddKeys(orderIdProperty);
        orderEntityType.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Decimal);
        orderEntityType.AddStructuralProperty("OrderDate", EdmPrimitiveTypeKind.DateTimeOffset);
        orderEntityType.AddStructuralProperty("ShippingAddress", new EdmComplexTypeReference(addressComplexType, false));

        customerEntityType = model.AddEntityType(typelessNamespace, "Customer");
        var customerIdProperty = customerEntityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        customerEntityType.AddKeys(customerIdProperty);
        customerEntityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        customerEntityType.AddStructuralProperty("CreditLimit", EdmPrimitiveTypeKind.Decimal);

        var ordersNavigationProperty = customerEntityType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "Orders",
            Target = orderEntityType,
            TargetMultiplicity = EdmMultiplicity.Many
        });

        var customerNavigationProperty = orderEntityType.AddUnidirectionalNavigation(
            new EdmNavigationPropertyInfo
            {
                Name = "Customer",
                TargetMultiplicity = EdmMultiplicity.One,
                Target = customerEntityType
            });

        var getChangesFunction = new EdmFunction(
            namespaceName: defaultNamespace,
            name: "GetChanges",
            returnType: new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(changeSetEntityType, false))),
            isBound: true,
            entitySetPathExpression: null,
            isComposable: true);
        getChangesFunction.AddParameter("bindingParameter", new EdmEntityTypeReference(deltaEntityType, false));
        model.AddElement(getChangesFunction);

        var orderEntityCollectionTypeReference = new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(orderEntityType, false)));

        var getChangedFunction = new EdmFunction(
            namespaceName: defaultNamespace,
            name: "GetChanged",
            returnType: orderEntityCollectionTypeReference,
            isBound: true,
            entitySetPathExpression: null,
            isComposable: true);
        getChangedFunction.AddParameter("bindingParameter", orderEntityCollectionTypeReference);
        model.AddElement(getChangedFunction);

        var getUnchangedFunction = new EdmFunction(
            namespaceName: defaultNamespace,
            name: "GetUnchanged",
            returnType: orderEntityCollectionTypeReference,
            isBound: true,
            entitySetPathExpression: null,
            isComposable: true);
        getUnchangedFunction.AddParameter("bindingParameter", orderEntityCollectionTypeReference);
        model.AddElement(getUnchangedFunction);

        var defaultEntityContainer = model.AddEntityContainer("Default", "Container");
        var customersEntitySet = defaultEntityContainer.AddEntitySet("Customers", customerEntityType);
        var ordersEntitySet = defaultEntityContainer.AddEntitySet("Orders", orderEntityType);

        customersEntitySet.AddNavigationTarget(ordersNavigationProperty, ordersEntitySet);
        ordersEntitySet.AddNavigationTarget(customerNavigationProperty, customersEntitySet);

        defaultEntityContainer.AddSingleton("TypelessDelta", deltaEntityType);
        defaultEntityContainer.AddSingleton("TypedDelta", deltaEntityType);
        defaultEntityContainer.AddEntitySet("ChangeSets", changeSetEntityType);

        model.SetAnnotationValue(getChangesFunction, new ReturnedEntitySetAnnotation("ChangeSets"));
    }

    public static EdmEntityType DeltaEntityType => deltaEntityType;

    public static EdmEntityType ChangeSetEntityType => changeSetEntityType;

    public static EdmEntityType CustomerEntityType => customerEntityType;

    public static EdmEntityType OrderEntityType => orderEntityType;

    public static EdmComplexType AddressComplexType => addressComplexType;

    public static EdmModel GetModel() => model;
}
