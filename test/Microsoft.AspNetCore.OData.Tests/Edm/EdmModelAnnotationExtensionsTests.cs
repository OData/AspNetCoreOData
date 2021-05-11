// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.Community.V1;
using Microsoft.OData.Edm.Vocabularies.V1;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class EdmModelAnnotationExtensionsTests
    {
        private static IEdmModel _model = GetEdmModel();

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
}
