//-----------------------------------------------------------------------------
// <copyright file="ODataFunctionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing
{
    public class ODataFunctionTests : WebApiTestBase<ODataFunctionTests>
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

        // following the Fixture convention.
        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = GetTypelessEdmModel();
            services.ConfigureControllers(typeof(FCustomersController));
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", edmModel).AddRouteComponents("attribute", edmModel));
        }

        public ODataFunctionTests(WebApiTestFixture<ODataFunctionTests> fixture)
           : base(fixture)
        {
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

            // Assert
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"@odata.context\":\"http://localhost/odata/$metadata#Edm.Boolean\",\"value\":true}", responseString);
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

            // Assert
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"@odata.context\":\"http://localhost/attribute/$metadata#Edm.Boolean\",\"value\":true}", responseString);
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

    public class FCustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            IEdmModel model = Request.GetModel();
            IEdmEntityType customerType = model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "Customer");

            EdmEntityObject customer = new EdmEntityObject(customerType);

            customer.TrySetPropertyValue("Id", 1);
            customer.TrySetPropertyValue("Tony", 1);

            EdmEntityObjectCollection customers =
                new EdmEntityObjectCollection(
                    new EdmCollectionTypeReference(new EdmCollectionType(customerType.ToEdmTypeReference(false))));
            customers.Add(customer);
            return Ok(customers);
        }

#region Bound Function using attribute routing
        [HttpGet("attribute/FCustomers({key})/NS.IntCollectionFunction(intValues={intValues})")]
        public bool IntCollectionFunctionOnAttriubte(int key, [FromODataUri] IEnumerable<int?> intValues)
        {
            return IntCollectionFunction(key, intValues);
        }

        [HttpGet("attribute/FCustomers({key})/NS.EnumFunction(color={color})")]
        public bool EnumFunctionOnAttribute(int key, [FromODataUri] EdmEnumObject color)
        {
            return EnumFunction(key, color);
        }

        [HttpGet("attribute/FCustomers({key})/NS.EnumCollectionFunction(colors={colors})")]
        public bool EnumCollectionFunctionOnAttribute(int key, [FromODataUri] EdmEnumObjectCollection colors)
        {
            return EnumCollectionFunction(key, colors);
        }

        [HttpGet("attribute/FCustomers({key})/NS.ComplexFunction(address={address})")]
        public bool ComplexFunctionOnAttribute(int key, [FromODataUri] EdmComplexObject address)
        {
            return ComplexFunction(key, address);
        }

        [HttpGet("attribute/FCustomers({key})/NS.ComplexCollectionFunction(addresses={addresses})")]
        public bool ComplexCollectionFunctionOnAttribute(int key, [FromODataUri] EdmComplexObjectCollection addresses)
        {
            return ComplexCollectionFunction(key, addresses);
        }

        [HttpGet("attribute/FCustomers({key})/NS.EntityFunction(customer={customer})")]
        public bool EntityFunctionOnAttribute(int key, [FromODataUri] EdmEntityObject customer)
        {
            return EntityFunction(key, customer);
        }

        [HttpGet("attribute/FCustomers({key})/NS.CollectionEntityFunction(customers={customers})")]
        public bool CollectionEntityFunctionOnAttribute(int key, [FromODataUri] EdmEntityObjectCollection customers)
        {
            return CollectionEntityFunction(key, customers);
        }

#endregion

#region Bound function using convention routing & Unbound function using Attribute routing
        // Here's the note:
        // [HttpGet] & [ODataModel] will create an odata convention routing for this method.
        // [HttpGet("odata/....")] will create an attribute routing.
        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundIntCollectionFunction(key={key},intValues={intValues})")]
        public bool IntCollectionFunction(int key, [FromODataUri] IEnumerable<int?> intValues)
        {
            Assert.NotNull(intValues);

            IList<int?> values = intValues.ToList();
            Assert.Equal(1, values[0]);
            Assert.Equal(2, values[1]);
            Assert.Null(values[2]);
            Assert.Equal(7, values[3]);
            Assert.Equal(8, values[4]);

            return true;
        }

        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundEnumFunction(key={key},color={color})")]
        public bool EnumFunction(int key, [FromODataUri] EdmEnumObject color)
        {
            Assert.NotNull(color);
            Assert.Equal("NS.Color", color.GetEdmType().FullName());
            Assert.Equal("0", color.Value);
            return true;
        }

        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundEnumCollectionFunction(key={key},colors={colors})")]
        public bool EnumCollectionFunction(int key, [FromODataUri] EdmEnumObjectCollection colors)
        {
            Assert.NotNull(colors);
            IList<IEdmEnumObject> results = colors.ToList();

            Assert.Equal(2, results.Count);

            // #1
            EdmEnumObject color = results[0] as EdmEnumObject;
            Assert.NotNull(color);
            Assert.Equal("NS.Color", color.GetEdmType().FullName());
            Assert.Equal("Red", color.Value);

            // #2
            EdmEnumObject color2 = results[1] as EdmEnumObject;
            Assert.NotNull(color2);
            Assert.Equal("NS.Color", color2.GetEdmType().FullName());
            Assert.Equal("Green", color2.Value);
            return true;
        }

        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundComplexFunction(key={key},address={address})")]
        public bool ComplexFunction(int key, [FromODataUri] EdmComplexObject address)
        {
            if (key == 99)
            {
                Assert.Null(address);
                return false;
            }

            Assert.NotNull(address);
            dynamic result = address;
            Assert.Equal("NS.Address", address.GetEdmType().FullName());
            Assert.Equal("NE 24th St.", result.Street);
            Assert.Equal("Redmond", result.City);
            return true;
        }

        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundComplexCollectionFunction(key={key},addresses={addresses})")]
        public bool ComplexCollectionFunction(int key, [FromODataUri] EdmComplexObjectCollection addresses)
        {
            Assert.NotNull(addresses);
            IList<IEdmComplexObject> results = addresses.ToList();

            Assert.Equal(2, results.Count);

            // #1
            EdmComplexObject complex = results[0] as EdmComplexObject;
            Assert.Equal("NS.Address", complex.GetEdmType().FullName());

            dynamic address = results[0];
            Assert.NotNull(address);
            Assert.Equal("NE 24th St.", address.Street);
            Assert.Equal("Redmond", address.City);

            // #2
            complex = results[1] as EdmComplexObject;
            Assert.Equal("NS.SubAddress", complex.GetEdmType().FullName());

            address = results[1];
            Assert.NotNull(address);
            Assert.Equal("LianHua Rd.", address.Street);
            Assert.Equal("Shanghai", address.City);
            Assert.Equal(9.9, address.Code);
            return true;
        }

        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundEntityFunction(key={key},customer={customer})")]
        public bool EntityFunction(int key, [FromODataUri] EdmEntityObject customer)
        {
            Assert.NotNull(customer);
            dynamic result = customer;
            Assert.Equal("NS.Customer", customer.GetEdmType().FullName());

            // entity call
            if (key == 9)
            {
                Assert.Equal(91, result.Id);
                Assert.Equal("John", result.Name);

                dynamic address = result.Location;
                EdmComplexObject addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.Address", addressObj.GetEdmType().FullName());
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);
            }
            else
            {
                // entity reference call
                Assert.Equal(8, result.Id);
                Assert.Equal("Id", String.Join(",", customer.GetChangedPropertyNames()));

                Assert.Equal("Name,Location", String.Join(",", customer.GetUnchangedPropertyNames()));
            }

            return true;
        }

        [HttpGet]
        [ODataRouteComponent("odata")]
        [HttpGet("odata/UnboundCollectionEntityFunction(key={key},customers={customers})")]
        public bool CollectionEntityFunction(int key, [FromODataUri] EdmEntityObjectCollection customers)
        {
            Assert.NotNull(customers);
            IList<IEdmEntityObject> results = customers.ToList();
            Assert.Equal(2, results.Count);

            // entities call
            if (key == 9)
            {
                // #1
                EdmEntityObject entity = results[0] as EdmEntityObject;
                Assert.NotNull(entity);
                Assert.Equal("NS.Customer", entity.GetEdmType().FullName());

                dynamic customer = results[0];
                Assert.Equal(91, customer.Id);
                Assert.Equal("John", customer.Name);

                dynamic address = customer.Location;
                EdmComplexObject addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.Address", addressObj.GetEdmType().FullName());
                Assert.Equal("NE 24th St.", address.Street);
                Assert.Equal("Redmond", address.City);

                // #2
                entity = results[1] as EdmEntityObject;
                Assert.Equal("NS.SpecialCustomer", entity.GetEdmType().FullName());

                customer = results[1];
                Assert.Equal(92, customer.Id);
                Assert.Equal("Mike", customer.Name);

                address = customer.Location;
                addressObj = Assert.IsType<EdmComplexObject>(address);
                Assert.Equal("NS.SubAddress", addressObj.GetEdmType().FullName());
                Assert.Equal("LianHua Rd.", address.Street);
                Assert.Equal("Shanghai", address.City);
                Assert.Equal(9.9, address.Code);

                Assert.Equal(new Guid("883F50C5-F554-4C49-98EA-F7CACB41658C"), customer.Title);
            }
            else
            {
                // entity references call
                int id = 81;
                foreach (IEdmEntityObject edmObj in results)
                {
                    EdmEntityObject entity = edmObj as EdmEntityObject;
                    Assert.NotNull(entity);
                    Assert.Equal("NS.Customer", entity.GetEdmType().FullName());

                    dynamic customer = entity;
                    Assert.Equal(id++, customer.Id);
                    Assert.Equal("Id", String.Join(",", customer.GetChangedPropertyNames()));
                    Assert.Equal("Name,Location", String.Join(",", customer.GetUnchangedPropertyNames()));
                }
            }

            return true;
        }
#endregion
    }
}
