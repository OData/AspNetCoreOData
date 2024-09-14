//-----------------------------------------------------------------------------
// <copyright file="AlternateKeysEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.Community.V1;
using Microsoft.OData.Edm.Vocabularies.V1;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AlternateKeys;

public static class AlternateKeysEdmModel
{
    private static IEdmModel _edmModel;
    public static IEdmModel GetEdmModel()
    {
        if (_edmModel != null)
        {
            return _edmModel;
        }

        EdmModel model = new EdmModel();

        // entity type 'Customer' with single alternate keys
        EdmEntityType customer = new EdmEntityType("NS", "Customer");
        customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        var ssn = customer.AddStructuralProperty("SSN", EdmPrimitiveTypeKind.String);
        model.AddAlternateKeyAnnotation(customer, new Dictionary<string, IEdmProperty>
        {
            {"SSN", ssn}
        });
        model.AddElement(customer);

        // entity type 'Order' with multiple alternate keys
        EdmEntityType order = new EdmEntityType("NS", "Order");
        order.AddKeys(order.AddStructuralProperty("OrderId", EdmPrimitiveTypeKind.Int32));
        var orderName = order.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        var orderToken = order.AddStructuralProperty("Token", EdmPrimitiveTypeKind.Guid);
        order.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Int32);
        model.AddAlternateKeyAnnotation(order, new Dictionary<string, IEdmProperty>
        {
            {"Name", orderName}
        });

        model.AddAlternateKeyAnnotation(order, new Dictionary<string, IEdmProperty>
        {
            {"Token", orderToken}
        });

        model.AddElement(order);

        // entity type 'Person' with composed alternate keys
        EdmEntityType person = new EdmEntityType("NS", "Person");
        person.AddKeys(person.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        var countryRegion = person.AddStructuralProperty("Country_Region", EdmPrimitiveTypeKind.String);
        var passport = person.AddStructuralProperty("Passport", EdmPrimitiveTypeKind.String);
        model.AddAlternateKeyAnnotation(person, new Dictionary<string, IEdmProperty>
        {
            {"Country_Region", countryRegion},
            {"Passport", passport}
        });
        model.AddElement(person);

        // complex type address
        EdmComplexType address = new EdmComplexType("NS", "Address");
        var street = address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
        var city = address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
        model.AddElement(address);

        // entity type 'Company' with complex type alternate keys
        EdmEntityType company = new EdmEntityType("NS", "Company");
        company.AddKeys(company.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        company.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Int32);
        company.AddStructuralProperty("Location", new EdmComplexTypeReference(address, isNullable: true));

        // ODL extension method doesn't build the path expression
        //model.AddAlternateKeyAnnotation(company, new Dictionary<string, IEdmProperty>
        //{
        //    {"City", city},
        //    {"Street", street}
        //});

        AddComplexPropertyAlternateKey(model, company);
        model.AddElement(company);

        // entity sets
        EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
        model.AddElement(container);
        container.AddEntitySet("Customers", customer);
        container.AddEntitySet("Orders", order);
        container.AddEntitySet("People", person);
        container.AddEntitySet("Companies", company);

        return _edmModel = model;
    }

    private static void AddComplexPropertyAlternateKey(EdmModel model, EdmEntityType company)
    {
        // Alternate key 1 -> Code
        List<IEdmExpression> propertyRefs = new List<IEdmExpression>();
        IEdmRecordExpression propertyRef = new EdmRecordExpression(
                new EdmPropertyConstructor("Alias", new EdmStringConstant("Code")),
                new EdmPropertyConstructor("Name", new EdmPropertyPathExpression("Code")));
        propertyRefs.Add(propertyRef);

        EdmRecordExpression alternateKey1 = new EdmRecordExpression(new EdmPropertyConstructor("Key", new EdmCollectionExpression(propertyRefs)));

        // Alternate key 2 -> City & Street
        propertyRefs = new List<IEdmExpression>();
        propertyRef = new EdmRecordExpression(
                new EdmPropertyConstructor("Alias", new EdmStringConstant("City")),
                new EdmPropertyConstructor("Name", new EdmPropertyPathExpression("Location/City")));
        propertyRefs.Add(propertyRef);

        propertyRef = new EdmRecordExpression(
                new EdmPropertyConstructor("Alias", new EdmStringConstant("Street")),
                new EdmPropertyConstructor("Name", new EdmPropertyPathExpression("Location/Street")));
        propertyRefs.Add(propertyRef);

        EdmRecordExpression alternateKey2 = new EdmRecordExpression(new EdmPropertyConstructor("Key", new EdmCollectionExpression(propertyRefs)));

        IEdmTerm coreAlternateTerm = AlternateKeysVocabularyModel.Instance.FindDeclaredTerm("OData.Community.Keys.V1.AlternateKeys");
        Assert.NotNull(coreAlternateTerm);

        var annotation = new EdmVocabularyAnnotation(
            company,
            coreAlternateTerm,
            new EdmCollectionExpression(alternateKey1, alternateKey2));

        annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
        model.SetVocabularyAnnotation(annotation);
    }

    // ODL 7.x Uri parser only supports Community.AlternateKeys
    // Once ODL supports Core.AlternateKeys, we can consider to use Core.AlternateKeys
    private static void AddComplexPropertyAlternateKey1(EdmModel model, EdmEntityType company)
    {
        List<IEdmExpression> propertyRefs = new List<IEdmExpression>();

        IEdmRecordExpression propertyRef = new EdmRecordExpression(
                new EdmPropertyConstructor("Alias", new EdmStringConstant("City")),
                new EdmPropertyConstructor("Name", new EdmPropertyPathExpression("Location/City")));

        propertyRefs.Add(propertyRef);

        propertyRef = new EdmRecordExpression(
                new EdmPropertyConstructor("Alias", new EdmStringConstant("Street")),
                new EdmPropertyConstructor("Name", new EdmPropertyPathExpression("Location/Street")));

        propertyRefs.Add(propertyRef);

        EdmRecordExpression alternateKeyRecord = new EdmRecordExpression(new EdmPropertyConstructor("Key", new EdmCollectionExpression(propertyRefs)));

        IEdmTerm coreAlternateTerm = CoreVocabularyModel.Instance.FindDeclaredTerm("Org.OData.Core.V1.AlternateKeys");
        Assert.NotNull(coreAlternateTerm);

        var annotation = new EdmVocabularyAnnotation(
            company,
            coreAlternateTerm,
            new EdmCollectionExpression(alternateKeyRecord));

        annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
        model.SetVocabularyAnnotation(annotation);
    }
}
