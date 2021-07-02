// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.E2E.Tests.Routing;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{

    public class FunctionTests : WebApiTestBase<FunctionTests>
    {

        private const string PrimitiveValues = "(intValues=@p)?@p=[1, 2, null, 7, 8]";

        private const string ComplexValue1 = "{\"@odata.type\":\"%23NS.Address\",\"Street\":\"NE 24th St.\",\"City\":\"Redmond\"}";
        private const string ComplexValue2 = "{\"@odata.type\":\"%23NS.SubAddress\",\"Street\":\"LianHua Rd.\",\"City\":\"Shanghai\", \"Code\":9.9}";

        private const string ComplexValue = "(address=@p)?@p=" + ComplexValue1;
        private const string CollectionComplex = "(addresses=@p)?@p=[" + ComplexValue1 + "," + ComplexValue2 + "]";

        private const string EnumValue = "(color=NS.Color'Red')";
        private const string CollectionEnum = "(colors=@p)?@p=['Red', 'Green']";

        private const string EntityValue1 = "{\"@odata.type\":\"%23NS.Customer\",\"Id\":91,\"Name\":\"John\",\"Location\":" + ComplexValue1 + "}";
        private const string EntityValue2 = "{\"@odata.type\":\"%23NS.SpecialCustomer\",\"Id\":92,\"Name\":\"Mike\",\"Location\":" + ComplexValue2 + ",\"Title\":\"883F50C5-F554-4C49-98EA-F7CACB41658C\"}";

        private const string EntityValue = "(customer=@p)?@p=" + EntityValue1;
        private const string CollectionEntity = "(customers=@p)?@p=[" + EntityValue1 + "," + EntityValue2 + "]";

        private const string EntityReference = "(customer=@p)?@p={\"@odata.id\":\"http://localhost/odata/FCustomers(8)\"}";

        private const string EntityReferences =
            "(customers=@p)?@p=[{\"@odata.id\":\"http://localhost/odata/FCustomers(81)\"},{\"@odata.id\":\"http://localhost/odata/FCustomers(82)/NS.SpecialCustomer\"}]";

        public FunctionTests(WebApiTestFixture<FunctionTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                IEdmModel edmModel = GetTypelessEdmModel();
                services.ConfigureControllers(typeof(FCustomersController));
                services.AddControllers().AddOData(opt => opt.AddModel("odata", edmModel).AddModel("attribute", edmModel));
            };
        }

        #region Bound Function

        public static TheoryDataSet<string> BoundFunctionRouteData
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { GetBoundFunction("IntCollectionFunction", PrimitiveValues) },

                    { GetBoundFunction("ComplexFunction", ComplexValue) },

                    { GetBoundFunction("ComplexCollectionFunction", CollectionComplex) },

                    { GetBoundFunction("EnumFunction", EnumValue) },

                    { GetBoundFunction("EnumCollectionFunction", CollectionEnum) },

                    { GetBoundFunction("EntityFunction", EntityValue) },
                    { GetBoundFunction("EntityFunction", EntityReference) },// reference

                    { GetBoundFunction("CollectionEntityFunction", CollectionEntity) },
                    { GetBoundFunction("CollectionEntityFunction", EntityReferences) },// references
                };
            }
        }

        private static string GetBoundFunction(string functionName, string parameter)
        {
            int key = 9;
            if (parameter.Contains("@odata.id"))
            {
                key = 8; // used to check the result
            }

            return "FCustomers(" + key + ")/NS." + functionName + parameter;
        }

        [Theory]
        [MemberData(nameof(BoundFunctionRouteData))]
        public async Task BoundFunctionWorks_WithParameters_ForTypeless(string odataPath)
        {
            // Arrange
            string requestUri = "odata/" + odataPath;
            HttpClient client = CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true}", responseContent);
        }

        [Theory]
        [MemberData(nameof(BoundFunctionRouteData))]
        public async Task BoundFunctionWorks_UsingAttributeRouting_WithParameters_ForTypeless(string odataPath)
        {
            // Arrange
            string requestUri = "attribute/" + odataPath;
            if (requestUri.Contains("@odata.id"))
            {
                requestUri = requestUri.Replace("http://localhost/odata", "http://localhost/attribute");
            }

            HttpClient client = CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);
            string responseContent = await response.Content.ReadAsStringAsync();
            Output.WriteLine(responseContent);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("{\"@odata.context\":\"http://localhost/attribute/$metadata#Edm.Boolean\",\"value\":true}", responseContent);
        }

        #endregion

        #region Unbound Function
        public static TheoryDataSet<string> UnboundFunctionRouteData
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    { GetUnboundFunction("UnboundIntCollectionFunction", PrimitiveValues) },

                    { GetUnboundFunction("UnboundComplexFunction", ComplexValue) },

                    { GetUnboundFunction("UnboundComplexCollectionFunction", CollectionComplex) },

                    { GetUnboundFunction("UnboundEnumFunction", EnumValue) },

                    { GetUnboundFunction("UnboundEnumCollectionFunction", CollectionEnum) },

                    { GetUnboundFunction("UnboundEntityFunction", EntityValue) },
                    { GetUnboundFunction("UnboundEntityFunction", EntityReference) },// reference

                    { GetUnboundFunction("UnboundCollectionEntityFunction", CollectionEntity) },
                    { GetUnboundFunction("UnboundCollectionEntityFunction", EntityReferences) }, // references
                };
            }
        }

        private static string GetUnboundFunction(string functionName, string parameter)
        {
            int key = 9;
            if (parameter.Contains("@odata.id"))
            {
                key = 8; // used to check the result
            }

            parameter = parameter.Insert(1, "key=" + key + ",");
            return functionName + parameter;
        }

        [Theory]
        [MemberData(nameof(UnboundFunctionRouteData))]
        public async Task UnboundFunctionWorks_WithParameters_WithAttributeRouting_ForTypeless(string odataPath)
        {
            // Arrange
            string requestUri = "odata/" + odataPath;
            HttpClient client = CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true}", responseString);
        }

        #endregion

        private static IEdmModel GetTypelessEdmModel()
        {
            EdmModel model = new EdmModel();

            // Enum type "Color"
            EdmEnumType colorEnum = new EdmEnumType("NS", "Color");
            colorEnum.AddMember(new EdmEnumMember(colorEnum, "Red", new EdmEnumMemberValue(0)));
            colorEnum.AddMember(new EdmEnumMember(colorEnum, "Blue", new EdmEnumMemberValue(1)));
            colorEnum.AddMember(new EdmEnumMember(colorEnum, "Green", new EdmEnumMemberValue(2)));
            model.AddElement(colorEnum);

            // complex type "Address"
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // derived complex type "SubAddress"
            EdmComplexType subAddress = new EdmComplexType("NS", "SubAddress", address);
            subAddress.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Double);
            model.AddElement(subAddress);

            // entity type "Customer"
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            customer.AddStructuralProperty("Location", new EdmComplexTypeReference(address, isNullable: true));
            model.AddElement(customer);

            // derived entity type special customer
            EdmEntityType specialCustomer = new EdmEntityType("NS", "SpecialCustomer", customer);
            specialCustomer.AddStructuralProperty("Title", EdmPrimitiveTypeKind.Guid);
            model.AddElement(specialCustomer);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            container.AddEntitySet("FCustomers", customer);

            EdmComplexTypeReference complexType = new EdmComplexTypeReference(address, isNullable: true);
            EdmCollectionTypeReference complexCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(complexType));

            EdmEnumTypeReference enumType = new EdmEnumTypeReference(colorEnum, isNullable: false);
            EdmCollectionTypeReference enumCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(enumType));

            EdmEntityTypeReference entityType = new EdmEntityTypeReference(customer, isNullable: false);
            EdmCollectionTypeReference entityCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(entityType));

            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);
            EdmCollectionTypeReference primitiveCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(intType));

            // bound functions
            BoundFunction(model, "IntCollectionFunction", "intValues", primitiveCollectionType, entityType);

            BoundFunction(model, "ComplexFunction", "address", complexType, entityType);

            BoundFunction(model, "ComplexCollectionFunction", "addresses", complexCollectionType, entityType);

            BoundFunction(model, "EnumFunction", "color", enumType, entityType);

            BoundFunction(model, "EnumCollectionFunction", "colors", enumCollectionType, entityType);

            BoundFunction(model, "EntityFunction", "customer", entityType, entityType);

            BoundFunction(model, "CollectionEntityFunction", "customers", entityCollectionType, entityType);

            // unbound functions
            UnboundFunction(container, "UnboundIntCollectionFunction", "intValues", primitiveCollectionType);

            UnboundFunction(container, "UnboundComplexFunction", "address", complexType);

            UnboundFunction(container, "UnboundComplexCollectionFunction", "addresses", complexCollectionType);

            UnboundFunction(container, "UnboundEnumFunction", "color", enumType);

            UnboundFunction(container, "UnboundEnumCollectionFunction", "colors", enumCollectionType);

            UnboundFunction(container, "UnboundEntityFunction", "customer", entityType);

            UnboundFunction(container, "UnboundCollectionEntityFunction", "customers", entityCollectionType);

            // bound to collection
            BoundToCollectionFunction(model, "BoundToCollectionFunction", "p", intType, entityType);

            model.SetAnnotationValue<BindableOperationFinder>(model, new BindableOperationFinder(model));
            return model;
        }

        private static void BoundFunction(EdmModel model, string funcName, string paramName, IEdmTypeReference edmType, IEdmEntityTypeReference bindingType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
            boundFunction.AddParameter("entity", bindingType);
            boundFunction.AddParameter(paramName, edmType);
            model.AddElement(boundFunction);
        }

        private static void BoundToCollectionFunction(EdmModel model, string funcName, string paramName, IEdmTypeReference edmType, IEdmEntityTypeReference bindingType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmCollectionTypeReference collectonType = new EdmCollectionTypeReference(new EdmCollectionType(bindingType));
            EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
            boundFunction.AddParameter("entityset", collectonType);
            boundFunction.AddParameter(paramName, edmType);
            model.AddElement(boundFunction);
        }

        private static void UnboundFunction(EdmEntityContainer container, string funcName, string paramName, IEdmTypeReference edmType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            var unboundFunction = new EdmFunction("NS", funcName, returnType, isBound: false, entitySetPathExpression: null, isComposable: true);
            unboundFunction.AddParameter("key", EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false));
            unboundFunction.AddParameter(paramName, edmType);
            container.AddFunctionImport(funcName, unboundFunction, entitySet: null);
        }
    }

}