// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Parser
{
    public class DefaultODataPathTemplateParserTests
    {
        private static IEdmTypeReference ReturnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
        private static IEdmTypeReference StringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
        private static IEdmTypeReference IntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

        private EdmModel _edmModel;
        private EdmEntityType _customerType;
        public DefaultODataPathTemplateParserTests()
        {
            _edmModel = GetEdmModel();
            _customerType = _edmModel.SchemaElements.OfType<EdmEntityType>().First(c => c.Name == "Customer");
        }

        [Fact]
        public void ParseODataUriTemplate_ForEntitySet()
        {
            // Arrange
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, "Customers", null);

            // Assert
            Assert.NotNull(path);
            ODataSegmentTemplate pathSegment = Assert.Single(path.Segments);
            EntitySetSegmentTemplate setSegment = Assert.IsType<EntitySetSegmentTemplate>(pathSegment);

            Assert.Equal("Customers", setSegment.Literal);
        }

        [Fact]
        public void ParseODataUriTemplate_ForSingleton()
        {
            // Arrange
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, "VipCustomer", null);

            // Assert
            Assert.NotNull(path);
            ODataSegmentTemplate pathSegment = Assert.Single(path.Segments);
            SingletonSegmentTemplate singletonSegment = Assert.IsType<SingletonSegmentTemplate>(pathSegment);

            Assert.Equal("VipCustomer", singletonSegment.Literal);
        }

        [Theory]
        [InlineData("Customers({idKey})")]
        [InlineData("Customers/{idKey}")]
        public void ParseODataUriTemplate_ForBasicKey(string template)
        {
            // Arrange
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, null);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Segments.Count);
            KeySegmentTemplate keySegment = Assert.IsType<KeySegmentTemplate>(path.Segments[1]);
            var keyMap = Assert.Single(keySegment.KeyMappings);

            Assert.Equal("Id", keyMap.Key);
            Assert.Equal("idKey", keyMap.Value);
        }

        [Theory]
        [InlineData("People(firstName={first},lastName={last})")]
        [InlineData("People(lastName={last},firstName={first})")]
        public void ParseODataUriTemplate_ForCompositeKeys(string template)
        {
            // Arrange
            // function with optional parameters
            EdmEntityType person = new EdmEntityType("NS", "Person");
            EdmStructuralProperty firstName = person.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            EdmStructuralProperty lastName = person.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);
            person.AddKeys(firstName, lastName);
            _edmModel.AddElement(person);

            EdmEntityContainer container = _edmModel.SchemaElements.OfType<EdmEntityContainer>().First();
            container.AddEntitySet("People", person);

            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, null);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Segments.Count);
            KeySegmentTemplate keySegment = Assert.IsType<KeySegmentTemplate>(path.Segments[1]);
            Assert.Equal(2, keySegment.KeyMappings.Count);

            Assert.Equal(new[] { "firstName", "lastName" }, keySegment.KeyMappings.Keys);
            Assert.Equal(new[] { "first", "last" }, keySegment.KeyMappings.Values);
        }

        [Theory]
        [InlineData("Customers({idKey})/Name", 3)]
        [InlineData("Customers/{idKey}/Name", 3)]
        [InlineData("VipCustomer/Name", 2)]
        public void ParseODataUriTemplate_ForProperty(string template, int count)
        {
            // Arrange
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, null);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(count, path.Segments.Count);
            PropertySegmentTemplate propertySegment = Assert.IsType<PropertySegmentTemplate>(path.Segments[count - 1]);

            Assert.Equal("Name", propertySegment.Literal);
        }

        [Theory]
        [InlineData("Customers({idKey})/Orders", 3)]
        [InlineData("Customers/{idKey}/Orders", 3)]
        [InlineData("VipCustomer/Orders", 2)]
        public void ParseODataUriTemplate_ForNavigationProperty(string template, int count)
        {
            // Arrange
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, null);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(count, path.Segments.Count);
            NavigationSegmentTemplate navigationSegment = Assert.IsType<NavigationSegmentTemplate>(path.Segments[count - 1]);

            Assert.Equal("Orders", navigationSegment.Literal);
        }

        [Theory]
        [InlineData("Customers({idKey})/Orders/$ref", 3)]
        [InlineData("Customers/{idKey}/Orders/$ref", 3)]
        [InlineData("VipCustomer/Orders/$ref", 2)]
        public void ParseODataUriTemplate_ForNavigationPropertyLink(string template, int count)
        {
            // Arrange
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, null);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(count, path.Segments.Count);
            NavigationLinkSegmentTemplate navigationLinkSegment = Assert.IsType<NavigationLinkSegmentTemplate>(path.Segments[count - 1]);

            Assert.Equal("Orders", navigationLinkSegment.Literal);
        }

        [Theory]
        [InlineData("Customers/NS.GetWholeSalary(minSalary={min},maxSalary={max})", 2)]
        [InlineData("Customers/NS.GetWholeSalary(minSalary={min},maxSalary={max},aveSalary={ave})", 3)]
        [InlineData("Customers/GetWholeSalary(minSalary={min},maxSalary={max})", 2)]
        [InlineData("Customers/GetWholeSalary(minSalary={min},maxSalary={max},aveSalary={ave})", 3)]
        public void ParseODataUriTemplate_ForFunctions(string template, int count)
        {
            // Arrange
            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", IntType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(_customerType, false))));
            getSalaray.AddParameter("minSalary", IntType);
            getSalaray.AddOptionalParameter("maxSalary", IntType);
            getSalaray.AddOptionalParameter("aveSalary", IntType, "129");
            _edmModel.AddElement(getSalaray);

            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();
            MockServiceProvider sp = null;
            if (!template.Contains("NS."))
            {
                sp = new MockServiceProvider(_edmModel);
            }

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, sp);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Segments.Count);
            FunctionSegmentTemplate functionSegment = Assert.IsType<FunctionSegmentTemplate>(path.Segments[1]);
            Assert.Equal(count, functionSegment.ParameterMappings.Count);

            if (count == 2)
            {
                Assert.Equal(new[] { "minSalary", "maxSalary" }, functionSegment.ParameterMappings.Keys);
                Assert.Equal(new[] { "min", "max" }, functionSegment.ParameterMappings.Values);
            }
            else
            {
                Assert.Equal(new[] { "minSalary", "maxSalary", "aveSalary" }, functionSegment.ParameterMappings.Keys);
                Assert.Equal(new[] { "min", "max", "ave" }, functionSegment.ParameterMappings.Values);
            }
        }

        [Theory]
        [InlineData("Customers/NS.SetWholeSalary")]
        [InlineData("Customers/SetWholeSalary")]
        public void ParseODataUriTemplate_ForActions(string template)
        {
            // Arrange
            // function with optional parameters
            EdmAction setSalaray = new EdmAction("NS", "SetWholeSalary", IntType, isBound: true, entitySetPathExpression: null);
            setSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(_customerType, false))));
            setSalaray.AddParameter("minSalary", IntType);
            setSalaray.AddOptionalParameter("maxSalary", IntType);
            setSalaray.AddOptionalParameter("aveSalary", IntType, "129");
            _edmModel.AddElement(setSalaray);

            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();
            MockServiceProvider sp = null;
            if (!template.Contains("NS."))
            {
                sp = new MockServiceProvider(_edmModel);
            }

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, sp);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(2, path.Segments.Count);
            ActionSegmentTemplate actionSegment = Assert.IsType<ActionSegmentTemplate>(path.Segments[1]);
            Assert.Equal("NS.SetWholeSalary", actionSegment.Literal);
        }

        [Theory]
        [InlineData("GetWholeSalaryImport(minSalary={min},maxSalary={max})", 2)]
        [InlineData("GetWholeSalaryImport(minSalary={min},maxSalary={max},aveSalary={ave})", 3)]
        public void ParseODataUriTemplate_ForFunctionImport(string template, int parameterCount)
        {
            // Arrange
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalaryImport", IntType, isBound: false, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("minSalary", IntType);
            getSalaray.AddOptionalParameter("maxSalary", IntType);
            getSalaray.AddOptionalParameter("aveSalary", IntType, "129");
            _edmModel.AddElement(getSalaray);

            EdmEntityContainer container = _edmModel.SchemaElements.OfType<EdmEntityContainer>().First();
            container.AddFunctionImport(getSalaray);

            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, template, null);

            // Assert
            Assert.NotNull(path);
            ODataSegmentTemplate segmentTemplate = Assert.Single(path.Segments);
            FunctionImportSegmentTemplate functionImportSegment = Assert.IsType<FunctionImportSegmentTemplate>(segmentTemplate);

            if (parameterCount == 2)
            {
                Assert.Equal(new[] { "minSalary", "maxSalary" }, functionImportSegment.ParameterMappings.Keys);
                Assert.Equal(new[] { "min", "max" }, functionImportSegment.ParameterMappings.Values);
            }
            else
            {
                Assert.Equal(new[] { "minSalary", "maxSalary", "aveSalary" }, functionImportSegment.ParameterMappings.Keys);
                Assert.Equal(new[] { "min", "max", "ave" }, functionImportSegment.ParameterMappings.Values);
            }
        }

        [Fact]
        public void ParseODataUriTemplate_ForActionImport()
        {
            // Arrange
            EdmAction setSalaray = new EdmAction("NS", "SetWholeSalaryImport", IntType, isBound: false, entitySetPathExpression: null);
            setSalaray.AddParameter("minSalary", IntType);
            setSalaray.AddOptionalParameter("maxSalary", IntType);
            setSalaray.AddOptionalParameter("aveSalary", IntType, "129");
            _edmModel.AddElement(setSalaray);

            EdmEntityContainer container = _edmModel.SchemaElements.OfType<EdmEntityContainer>().First();
            container.AddActionImport(setSalaray);

            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();

            // Act
            ODataPathTemplate path = parser.Parse(_edmModel, "SetWholeSalaryImport", null);

            // Assert
            Assert.NotNull(path);
            ODataSegmentTemplate segmentTemplate = Assert.Single(path.Segments);
            ActionImportSegmentTemplate actionImportSegment = Assert.IsType<ActionImportSegmentTemplate>(segmentTemplate);
            Assert.Equal("SetWholeSalaryImport", actionImportSegment.Literal);
        }

        private static EdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(customer);
            model.AddElement(vipCustomer);

            EdmEntityType order = new EdmEntityType("NS", "Order");
            order.AddKeys(order.AddStructuralProperty("OrderId", EdmPrimitiveTypeKind.Int32));
            model.AddElement(order);

            customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Orders",
                Target = order,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            entityContainer.AddEntitySet("Customers", customer);
            entityContainer.AddSingleton("VipCustomer", customer);
            model.AddElement(entityContainer);
            return model;
        }
    }
}
