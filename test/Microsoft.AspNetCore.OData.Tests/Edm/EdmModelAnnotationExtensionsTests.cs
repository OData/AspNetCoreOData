//-----------------------------------------------------------------------------
// <copyright file="EdmModelAnnotationExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.Community.V1;
using Microsoft.OData.Edm.Vocabularies.V1;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm;

public class EdmModelAnnotationExtensionsTests
{
    private static IEdmModel _model = GetEdmModel();

    [Fact]
    public void GetAcceptableMediaTypes_ThrowsArgumentNull_Model()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetAcceptableMediaTypes(null), "model");
    }

    [Fact]
    public void GetAcceptableMediaTypes_ThrowsArgumentNull_Target()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => EdmCoreModel.Instance.GetAcceptableMediaTypes(null), "target");
    }

    [Fact]
    public void GetAcceptableMediaTypes_Works_Target()
    {
        // Arrange
        EdmModel model = new EdmModel();
        EdmEntityType entity = new EdmEntityType("NS", "entity");
        model.AddElement(entity);

        // Act
        IList<string> mediaTypes = model.GetAcceptableMediaTypes(entity);

        // Assert
        Assert.Null(mediaTypes);

        // Act
        EdmCollectionExpression collectionExp = new EdmCollectionExpression(
            new EdmStringConstant("application/octet-stream"),
            new EdmStringConstant("text/plain"));

        var annotation = new EdmVocabularyAnnotation(entity, CoreVocabularyModel.AcceptableMediaTypesTerm, collectionExp);
        annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
        model.SetVocabularyAnnotation(annotation);

        // Act
        mediaTypes = model.GetAcceptableMediaTypes(entity);

        // Assert
        Assert.Collection(mediaTypes,
            e =>
            {
                Assert.Equal("application/octet-stream", e);
            },
            e =>
            {
                Assert.Equal("text/plain", e);
            });
    }

    [Fact]
    public void GetAlternateKeysTest_WorksForCoreAlternateKeys()
    {
        // Arrange
        IEdmEntityType customer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");

        // 1) Act & Assert: Cannot find the property
        IEnumerable<IDictionary<string, IEdmPathExpression>> alternateKeys = _model.GetAlternateKeys(customer);

        // Assert
        Assert.NotNull(alternateKeys);
        IDictionary<string, IEdmPathExpression> alternateKeyDict = Assert.Single(alternateKeys);
        KeyValuePair<string, IEdmPathExpression> alternateKey = Assert.Single(alternateKeyDict);
        Assert.Equal("Title", alternateKey.Key);
        Assert.Equal("Title", alternateKey.Value.Path);
    }

    [Fact]
    public void GetAlternateKeysTest_WorksForCommunityAlternateKeys()
    {
        // Arrange
        IEdmEntityType customer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Company");

        // Act
        IDictionary<string, IEdmPathExpression>[] alternateKeys = _model.GetAlternateKeys(customer).ToArray();

        // Assert
        Assert.NotNull(alternateKeys);
        Assert.Equal(2, alternateKeys.Length);

        // 1)
        IDictionary<string, IEdmPathExpression> alternateKeyDict1 = alternateKeys[0];
        KeyValuePair<string, IEdmPathExpression> alternateKey1 = Assert.Single(alternateKeyDict1);
        Assert.Equal("Code", alternateKey1.Key);
        Assert.Equal("Code", alternateKey1.Value.Path);

        // 2)
        IDictionary<string, IEdmPathExpression> alternateKeyDict2 = alternateKeys[1];
        Assert.Equal(2, alternateKeyDict2.Count);
        KeyValuePair<string, IEdmPathExpression> alternateKey2 = alternateKeyDict2.First(a => a.Key == "City");
        Assert.Equal("Location/City", alternateKey2.Value.Path);

        alternateKey2 = alternateKeyDict2.First(a => a.Key == "Street");
        Assert.Equal("Location/Street", alternateKey2.Value.Path);
    }

    [Fact]
    public void GetClrEnumMemberAnnotation_ThrowsArugmentNull_ForInputParameters()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetClrEnumMemberAnnotation(null), "edmModel");

        model = new Mock<IEdmModel>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetClrEnumMemberAnnotation(null), "enumType");
    }

    [Fact]
    public void GetClrPropertyName_ThrowsArugmentNull_ForInputParameters()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetClrPropertyName(null), "edmModel");

        model = new Mock<IEdmModel>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetClrPropertyName(null), "edmProperty");
    }

    [Fact]
    public void GetDynamicPropertyDictionary_ThrowsArugmentNull_ForInputParameters()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetDynamicPropertyDictionary(null), "edmModel");

        model = new Mock<IEdmModel>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetDynamicPropertyDictionary(null), "edmType");
    }

    [Fact]
    public void GetModelName_ThrowsArugmentNull_Model()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetModelName(), "model");
    }

    [Fact]
    public void SetModelName_ThrowsArugmentNull_Model()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.SetModelName(null), "model");

        model = new Mock<IEdmModel>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => model.SetModelName(null), "name");
    }

    [Fact]
    public void GetModelName_CanCallSetModelName_UsingDefaultGuid()
    {
        // Arrange
        IEdmModel model = new EdmModel();

        // Act
        string modelName = model.GetModelName();

        // Assert
        Assert.NotNull(modelName);
        Assert.True(Guid.TryParse(modelName, out _));
    }

    [Fact]
    public void GetAndSetModelName_RoundTrip()
    {
        // Arrange
        string testName = "myName";
        IEdmModel model = new EdmModel();

        // Act
        model.SetModelName(testName);
        string name = model.GetModelName();

        // Assert
        Assert.Equal(testName, name);
    }

    [Fact]
    public void GetTypeMapper_ReturnsDefaultTypeMapper_IfNullModelOrWithoutTypeMapper()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        Assert.IsType<DefaultODataTypeMapper>(model.GetTypeMapper());

        // Arrange & Act & Assert
        model = EdmCoreModel.Instance;
        Assert.IsType<DefaultODataTypeMapper>(model.GetTypeMapper());

        // Arrange & Act & Assert
        model = new EdmModel();
        Assert.IsType<DefaultODataTypeMapper>(model.GetTypeMapper());
    }

    [Fact]
    public void SetTypeMapper_ThrowsArugmentNull_Model()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.SetTypeMapper(null), "model");

        model = new Mock<IEdmModel>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => model.SetTypeMapper(null), "mapper");
    }

    [Fact]
    public void GetAndSetTypeMapper_RoundTrip()
    {
        // Arrange
        IODataTypeMapper mapper = new Mock<IODataTypeMapper>().Object;
        IEdmModel model = new EdmModel();

        // Act
        model.SetTypeMapper(mapper);
        IODataTypeMapper actual = model.GetTypeMapper();

        // Assert
        Assert.Same(mapper, actual);
    }

    [Fact]
    public void GetAlternateKeys_ThrowsArugmentNull_ForInputParameters()
    {
        // Arrange & Act & Assert
        IEdmModel model = null;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetAlternateKeys(null), "model");

        model = new Mock<IEdmModel>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => model.GetAlternateKeys(null), "entityType");
    }

    private static IEdmModel GetEdmModel()
    {
        EdmModel model = new EdmModel();

        // complex type address
        EdmComplexType address = new EdmComplexType("NS", "Address");
        address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
        address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
        model.AddElement(address);

        EdmComplexType vipAddress = new EdmComplexType("NS", "VipAddress");
        vipAddress.AddStructuralProperty("ZipCode", EdmPrimitiveTypeKind.String);
        model.AddElement(vipAddress);

        EdmEntityType company = new EdmEntityType("NS", "Company");
        company.AddKeys(company.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        company.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Int32);
        company.AddStructuralProperty("Location", new EdmComplexTypeReference(address, isNullable: true));
        model.AddElement(company);
        AddComplexPropertyCommunityAlternateKey(model, company);

        // entity type 'Customer' with single alternate keys
        EdmEntityType customer = new EdmEntityType("NS", "Customer");
        customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        customer.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
        model.AddElement(customer);
        AddComplexPropertyCoreAlternateKey(model, customer);
        return model;
    }

    private static void AddComplexPropertyCommunityAlternateKey(EdmModel model, EdmEntityType entity)
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

        var annotation = new EdmVocabularyAnnotation(entity, coreAlternateTerm, new EdmCollectionExpression(alternateKey1, alternateKey2));

        annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
        model.SetVocabularyAnnotation(annotation);
    }

    private static void AddComplexPropertyCoreAlternateKey(EdmModel model, EdmEntityType entity)
    {
        List<IEdmExpression> propertyRefs = new List<IEdmExpression>();
        IEdmRecordExpression propertyRef = new EdmRecordExpression(
                new EdmPropertyConstructor("Alias", new EdmStringConstant("Title")),
                new EdmPropertyConstructor("Name", new EdmPropertyPathExpression("Title")));
        propertyRefs.Add(propertyRef);

        EdmRecordExpression alternateKeyRecord = new EdmRecordExpression(new EdmPropertyConstructor("Key", new EdmCollectionExpression(propertyRefs)));

        IEdmTerm coreAlternateTerm = CoreVocabularyModel.Instance.FindDeclaredTerm("Org.OData.Core.V1.AlternateKeys");
        Assert.NotNull(coreAlternateTerm);

        var annotation = new EdmVocabularyAnnotation(entity, coreAlternateTerm, new EdmCollectionExpression(alternateKeyRecord));

        annotation.SetSerializationLocation(model, EdmVocabularyAnnotationSerializationLocation.Inline);
        model.SetVocabularyAnnotation(annotation);
    }
}
