// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;

namespace Microsoft.AspNet.OData.Authorization.Tests.Models
{
    public class TestModel
    {
        public static IEdmModel GetModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<RoutingCustomer>("RoutingCustomers");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<SalesPerson>("SalesPeople");
            builder.EntitySet<EmailAddress>("EmailAddresses");
            builder.EntitySet<üCategory>("üCategories");
            builder.Singleton<RoutingCustomer>("VipCustomer");
            builder.Singleton<Product>("MyProduct");
            builder.EntitySet<DateTimeOffsetKeyCustomer>("DateTimeOffsetKeyCustomers");
            builder.EntitySet<Destination>("Destinations");
            builder.EntitySet<Incident>("Incidents");
            builder.ComplexType<Dog>();
            builder.ComplexType<Cat>();
            builder.EntityType<SpecialProduct>();
            builder.ComplexType<UsAddress>();

            ActionConfiguration getRoutingCustomerById = builder.Action("GetRoutingCustomerById");
            getRoutingCustomerById.Parameter<int>("RoutingCustomerId");
            getRoutingCustomerById.ReturnsFromEntitySet<RoutingCustomer>("RoutingCustomers");

            ActionConfiguration getSalesPersonById = builder.Action("GetSalesPersonById");
            getSalesPersonById.Parameter<int>("salesPersonId");
            getSalesPersonById.ReturnsFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getAllVIPs = builder.Action("GetAllVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getAllVIPs, "RoutingCustomers");

            builder.EntityType<RoutingCustomer>().ComplexProperty<Address>(c => c.Address);
            builder.EntityType<RoutingCustomer>().Action("GetRelatedRoutingCustomers")
                .ReturnsCollectionFromEntitySet<RoutingCustomer>("RoutingCustomers");

            ActionConfiguration getBestRelatedRoutingCustomer = builder.EntityType<RoutingCustomer>()
                .Action("GetBestRelatedRoutingCustomer");
            ActionReturnsFromEntitySet<VIP>(builder, getBestRelatedRoutingCustomer, "RoutingCustomers");

            ActionConfiguration getVIPS = builder.EntityType<RoutingCustomer>().Collection.Action("GetVIPs");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPS, "RoutingCustomers");

            builder.EntityType<RoutingCustomer>().Collection.Action("GetProducts").ReturnsCollectionFromEntitySet<Product>("Products");
            builder.EntityType<RoutingCustomer>().Action("GetFavoriteProduct").ReturnsFromEntitySet<Product>("Products");
            builder.EntityType<VIP>().Action("GetSalesPerson").ReturnsFromEntitySet<SalesPerson>("SalesPeople");
            builder.EntityType<VIP>().Collection.Action("GetSalesPeople").ReturnsCollectionFromEntitySet<SalesPerson>("SalesPeople");

            ActionConfiguration getMostProfitable = builder.EntityType<VIP>().Collection.Action("GetMostProfitable");
            ActionReturnsFromEntitySet<VIP>(builder, getMostProfitable, "RoutingCustomers");

            ActionConfiguration getVIPRoutingCustomers = builder.EntityType<SalesPerson>().Action("GetVIPRoutingCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPRoutingCustomers, "RoutingCustomers");

            ActionConfiguration getVIPRoutingCustomersOnCollection = builder.EntityType<SalesPerson>().Collection.Action("GetVIPRoutingCustomers");
            ActionReturnsCollectionFromEntitySet<VIP>(builder, getVIPRoutingCustomersOnCollection, "RoutingCustomers");

            builder.EntityType<VIP>().HasRequired(v => v.RelationshipManager);
            builder.EntityType<ImportantProduct>().HasRequired(ip => ip.LeadSalesPerson);

            // function bound to an entity
            FunctionConfiguration topProductId = builder.EntityType<Product>().Function("TopProductId");
            topProductId.Returns<int>();

            FunctionConfiguration topProductIdByCity = builder.EntityType<Product>().Function("TopProductIdByCity");
            topProductIdByCity.Parameter<string>("city");
            topProductIdByCity.Returns<string>();

            FunctionConfiguration topProductIdByCityAndModel = builder.EntityType<Product>().Function("TopProductIdByCityAndModel");
            topProductIdByCityAndModel.Parameter<string>("city");
            topProductIdByCityAndModel.Parameter<int>("model");
            topProductIdByCityAndModel.Returns<string>();

            FunctionConfiguration optionFunctions = builder.EntityType<Product>().Collection.Function("GetCount").Returns<int>();
            optionFunctions.Parameter<double>("minSalary");
            optionFunctions.Parameter<double>("maxSalary").Optional();
            optionFunctions.Parameter<double>("aveSalary").Optional().HasDefaultValue("1200.99");

            // function bound to a collection of entities
            FunctionConfiguration topProductOfAll = builder.EntityType<Product>().Collection.Function("TopProductOfAll");
            topProductOfAll.Returns<string>();

            FunctionConfiguration topProductOfAllByCity = builder.EntityType<Product>().Collection.Function("TopProductOfAllByCity");
            topProductOfAllByCity.Parameter<string>("city");
            topProductOfAllByCity.Returns<string>();

            FunctionConfiguration copyProductByCity = builder.EntityType<Product>().Function("CopyProductByCity");
            copyProductByCity.Parameter<string>("city");
            copyProductByCity.Returns<string>();

            FunctionConfiguration topProductOfAllByCityAndModel = builder.EntityType<Product>().Collection.Function("TopProductOfAllByCityAndModel");
            topProductOfAllByCityAndModel.Parameter<string>("city");
            topProductOfAllByCityAndModel.Parameter<int>("model");
            topProductOfAllByCityAndModel.Returns<string>();

            // Function bound to the base entity type and derived entity type
            builder.EntityType<RoutingCustomer>().Function("GetOrdersCount").Returns<string>();
            builder.EntityType<VIP>().Function("GetOrdersCount").Returns<string>();

            // Overloaded function only bound to the base entity type with one paramter
            var getOrderCount = builder.EntityType<RoutingCustomer>().Function("GetOrdersCount");
            getOrderCount.Parameter<int>("factor");
            getOrderCount.Returns<string>();

            // Function only bound to the derived entity type
            builder.EntityType<SpecialVIP>().Function("GetSpecialGuid").Returns<string>();

            // Function bound to the collection of the base and the derived entity type
            builder.EntityType<RoutingCustomer>().Collection.Function("GetAllEmployees").Returns<string>();
            builder.EntityType<VIP>().Collection.Function("GetAllEmployees").Returns<string>();

            // Unbound function
            builder.Function("UnboundFunction").ReturnsCollection<int>().IsComposable = true;

            // Action only bound to the derived entity type
            builder.EntityType<SpecialVIP>().Action("ActionBoundToSpecialVIP");

            // Action only bound to the derived entity type
            builder.EntityType<SpecialVIP>().Collection.Action("ActionBoundToSpecialVIPs");

            // Function only bound to the base entity collection type
            builder.EntityType<RoutingCustomer>().Collection.Function("FunctionBoundToRoutingCustomers").Returns<int>();

            // Function only bound to the derived entity collection type
            builder.EntityType<VIP>().Collection.Function("FunctionBoundToVIPs").Returns<int>();

            // Bound function with multiple parameters
            var functionBoundToProductWithMultipleParamters = builder.EntityType<Product>().Function("FunctionBoundToProductWithMultipleParamters");
            functionBoundToProductWithMultipleParamters.Parameter<int>("P1");
            functionBoundToProductWithMultipleParamters.Parameter<int>("P2");
            functionBoundToProductWithMultipleParamters.Parameter<string>("P3");
            functionBoundToProductWithMultipleParamters.Returns<int>();

            // Overloaded bound function with no parameter
            builder.EntityType<Product>().Function("FunctionBoundToProduct").Returns<int>();

            // Overloaded bound function with one parameter
            builder.EntityType<Product>().Function("FunctionBoundToProduct").Returns<int>().Parameter<int>("P1");

            // Overloaded bound function with multiple parameters
            var functionBoundToProduct = builder.EntityType<Product>().Function("FunctionBoundToProduct").Returns<int>();
            functionBoundToProduct.Parameter<int>("P1");
            functionBoundToProduct.Parameter<int>("P2");
            functionBoundToProduct.Parameter<string>("P3");

            // Unbound function with one parameter
            var unboundFunctionWithOneParamters = builder.Function("UnboundFunctionWithOneParamters");
            unboundFunctionWithOneParamters.Parameter<int>("P1");
            unboundFunctionWithOneParamters.ReturnsFromEntitySet<RoutingCustomer>("RoutingCustomers");
            unboundFunctionWithOneParamters.IsComposable = true;

            // Unbound function with multiple parameters
            var functionWithMultipleParamters = builder.Function("UnboundFunctionWithMultipleParamters");
            functionWithMultipleParamters.Parameter<int>("P1");
            functionWithMultipleParamters.Parameter<int>("P2");
            functionWithMultipleParamters.Parameter<string>("P3");
            functionWithMultipleParamters.Returns<int>();

            // Overloaded unbound function with no parameter
            builder.Function("OverloadUnboundFunction").Returns<int>();

            // Overloaded unbound function with one parameter
            builder.Function("OverloadUnboundFunction").Returns<int>().Parameter<int>("P1");

            // Overloaded unbound function with multiple parameters
            var overloadUnboundFunction = builder.Function("OverloadUnboundFunction").Returns<int>();
            overloadUnboundFunction.Parameter<int>("P1");
            overloadUnboundFunction.Parameter<int>("P2");
            overloadUnboundFunction.Parameter<string>("P3");

            var functionWithComplexTypeParameter =
                builder.EntityType<RoutingCustomer>().Function("CanMoveToAddress").Returns<bool>();
            functionWithComplexTypeParameter.Parameter<Address>("address");

            var functionWithCollectionOfComplexTypeParameter =
                builder.EntityType<RoutingCustomer>().Function("MoveToAddresses").Returns<bool>();
            functionWithCollectionOfComplexTypeParameter.CollectionParameter<Address>("addresses");

            var functionWithCollectionOfPrimitiveTypeParameter =
                builder.EntityType<RoutingCustomer>().Function("CollectionOfPrimitiveTypeFunction").Returns<bool>();
            functionWithCollectionOfPrimitiveTypeParameter.CollectionParameter<int>("intValues");

            var functionWithEntityTypeParameter =
                builder.EntityType<RoutingCustomer>().Function("EntityTypeFunction").Returns<bool>();
            functionWithEntityTypeParameter.EntityParameter<Product>("product");

            var functionWithCollectionEntityTypeParameter =
                builder.EntityType<RoutingCustomer>().Function("CollectionEntityTypeFunction").Returns<bool>();
            functionWithCollectionEntityTypeParameter.CollectionEntityParameter<Product>("products");

            return builder.GetEdmModel();
        }

        public static IEdmModel GetModelWithPermissions()
        {
            var model = GetModel() as EdmModel;
            AddPermissions(model);
            return model;
        }

        static void AddPermissions(EdmModel model)
        {
            var readRestrictions = "Org.OData.Capabilities.V1.ReadRestrictions";
            var insertRestrictions = "Org.OData.Capabilities.V1.InsertRestrictions";
            var updateRestrictions = "Org.OData.Capabilities.V1.UpdateRestrictions";
            var deleteRestrictions = "Org.OData.Capabilities.V1.DeleteRestrictions";
            var operationRestrictions = "Org.OData.Capabilities.V1.OperationRestrictions";

            var product = model.FindDeclaredType("Microsoft.AspNet.OData.Test.Routing.Product") as IEdmEntityType;
            var products = model.FindDeclaredEntitySet("Products");
            var myProduct = model.FindDeclaredSingleton("MyProduct");
            var customers = model.FindDeclaredEntitySet("RoutingCustomers");
            var vipCustomer = model.FindDeclaredSingleton("VipCustomer");
            var salesPeople = model.FindDeclaredEntitySet("SalesPeople");
            var productFunction = model.SchemaElements.OfType<IEdmOperation>()
                .First(o => o.Name == "FunctionBoundToProduct" && o.Parameters.Count() == 1);
            var productFunction2 = model.SchemaElements.OfType<IEdmOperation>()
                .First(o => o.Name == "FunctionBoundToProduct" && o.Parameters.Count() == 2);
            var productFunction3 = model.SchemaElements.OfType<IEdmOperation>()
                .First(o => o.Name == "FunctionBoundToProduct" && o.Parameters.Count() == 4);
            var topProduct = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "TopProductOfAll");
            var getProducts = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetProducts");
            var getFavoriteProduct = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetFavoriteProduct");
            var getSalesPerson = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetSalesPerson");
            var getSalesPeople = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetSalesPeople");
            var getVIPRoutingCustomers = model.SchemaElements.OfType<IEdmOperation>()
                .First(o => o.Name == "GetVIPRoutingCustomers" && o.IsBound && !o.Parameters.First().Type.IsCollection());
            var getVIPRoutingCustomersBoundToCollection = model.SchemaElements.OfType<IEdmOperation>()
                .First(o => o.Name == "GetVIPRoutingCustomers" && o.IsBound && o.Parameters.First().Type.IsCollection());
            var getRoutingCustomerById = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "GetRoutingCustomerById");
            var unboundFunction = model.SchemaElements.OfType<IEdmOperation>().First(o => o.Name == "UnboundFunction");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                products,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    PermissionsHelper.CreatePermissionProperty(new string[] { "Product.Read", "Product.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", PermissionsHelper.CreatePermission(new[] { "Product.ReadByKey" })))));


            PermissionsHelper.AddPermissionsTo(model, products, insertRestrictions, "Product.Insert");
            PermissionsHelper.AddPermissionsTo(model, products, deleteRestrictions, "Product.Delete");
            PermissionsHelper.AddPermissionsTo(model, products, updateRestrictions, "Product.Update");

            PermissionsHelper.AddPermissionsTo(model, myProduct, readRestrictions, "MyProduct.Read");
            PermissionsHelper.AddPermissionsTo(model, myProduct, deleteRestrictions, "MyProduct.Delete");
            PermissionsHelper.AddPermissionsTo(model, myProduct, updateRestrictions, "MyProduct.Update");

            PermissionsHelper.AddPermissionsTo(model, productFunction, operationRestrictions, "Product.Function");
            PermissionsHelper.AddPermissionsTo(model, productFunction2, operationRestrictions, "Product.Function2");
            PermissionsHelper.AddPermissionsTo(model, productFunction3, operationRestrictions, "Product.Function3");
            PermissionsHelper.AddPermissionsTo(model, topProduct, operationRestrictions, "Product.Top");
            PermissionsHelper.AddPermissionsTo(model, getRoutingCustomerById, operationRestrictions, "GetRoutingCustomerById");
            PermissionsHelper.AddPermissionsTo(model, unboundFunction, operationRestrictions, "UnboundFunction");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                customers,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    PermissionsHelper.CreatePermissionProperty(new string[] { "Customer.Read", "Customer.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", PermissionsHelper.CreatePermission(new[] { "Customer.ReadByKey" })))));

            PermissionsHelper.AddPermissionsTo(model, customers, insertRestrictions, "Customer.Insert");

            PermissionsHelper.AddPermissionsTo(model, vipCustomer, readRestrictions, "VipCustomer.Read");

            model.AddVocabularyAnnotation(new EdmVocabularyAnnotation(
                salesPeople,
                model.FindTerm(readRestrictions),
                new EdmRecordExpression(
                    PermissionsHelper.CreatePermissionProperty(new string[] { "SalesPerson.Read", "SalesPerson.ReadAll" }),
                    new EdmPropertyConstructor("ReadByKeyRestrictions", PermissionsHelper.CreatePermission(new[] { "SalesPerson.ReadByKey" })))));

            PermissionsHelper.AddPermissionsTo(model, getProducts, operationRestrictions, "Customer.GetProducts");
            PermissionsHelper.AddPermissionsTo(model, getFavoriteProduct, operationRestrictions, "Customer.GetFavoriteProduct");
            PermissionsHelper.AddPermissionsTo(model, getSalesPerson, operationRestrictions, "Customer.GetSalesPerson");
            PermissionsHelper.AddPermissionsTo(model, getSalesPeople, operationRestrictions, "Customer.GetSalesPeople");
            PermissionsHelper.AddPermissionsTo(model, getVIPRoutingCustomers, operationRestrictions, "SalesPerson.GetVip");
            PermissionsHelper.AddPermissionsTo(model,
                getVIPRoutingCustomersBoundToCollection,
                operationRestrictions,
                "SalesPerson.GetVipOnCollection");
        }

        public static ActionConfiguration ActionReturnsFromEntitySet<TEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TEntityType : class
        {
            action.NavigationSource = CreateOrReuseEntitySet<TEntityType>(builder, entitySetName);
            action.ReturnType = builder.GetTypeConfigurationOrNull(typeof(TEntityType));
            return action;
        }

        public static ActionConfiguration ActionReturnsCollectionFromEntitySet<TElementEntityType>(ODataModelBuilder builder, ActionConfiguration action, string entitySetName) where TElementEntityType : class
        {
            Type clrCollectionType = typeof(IEnumerable<TElementEntityType>);
            action.NavigationSource = CreateOrReuseEntitySet<TElementEntityType>(builder, entitySetName);
            IEdmTypeConfiguration elementType = builder.GetTypeConfigurationOrNull(typeof(TElementEntityType));
            action.ReturnType = new CollectionTypeConfiguration(elementType, clrCollectionType);
            return action;
        }

        public static EntitySetConfiguration CreateOrReuseEntitySet<TElementEntityType>(ODataModelBuilder builder, string entitySetName) where TElementEntityType : class
        {
            EntitySetConfiguration entitySet = builder.EntitySets.SingleOrDefault(s => s.Name == entitySetName);

            if (entitySet == null)
            {
                builder.EntitySet<TElementEntityType>(entitySetName);
                entitySet = builder.EntitySets.Single(s => s.Name == entitySetName);
            }
            else
            {
                builder.EntityType<TElementEntityType>();
            }
            return entitySet;
        }

        public class RoutingCustomer
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<Product> Products { get; set; }
            public Address Address { get; set; }
            public Pet Pet { get; set; }
        }

        public class EmailAddress
        {
            [Key]
            public string Value { get; set; }
            public string Text { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
        }

        public class UsAddress : Address
        {
            public string Country { get; set; }
        }

        public class Pet
        {
            public string Name { get; set; }
            public DateTimeOffset Birth { get; set; }
        }

        public class Dog : Pet
        {
            public bool CanBark { get; set; }
            public int RunSpeed { get; set; }
        }

        public class Cat : Pet
        {
            public bool CanMeow { get; set; }
            public int ClimbHeight { get; set; }
        }

        public class Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public List<string> Tags { get; set; }
            public virtual List<RoutingCustomer> RoutingCustomers { get; set; }
        }

        public class SpecialProduct : Product
        {
            public int Value { get; set; }
        }

        public class SalesPerson
        {
            public SalesPerson()
            {
                this.DynamicProperties = new Dictionary<string, object>();
            }

            public int ID { get; set; }
            public string Name { get; set; }
            public virtual List<VIP> ManagedRoutingCustomers { get; set; }
            public virtual List<ImportantProduct> ManagedProducts { get; set; }
            public IDictionary<string, object> DynamicProperties { get; set; }
        }

        public class VIP : RoutingCustomer
        {
            public virtual SalesPerson RelationshipManager { get; set; }
            public string Company { get; set; }
        }

        public class SpecialVIP : VIP
        {
            public Guid SpecialGuid { get; set; }
        }

        public class ImportantProduct : Product
        {
            public virtual SalesPerson LeadSalesPerson { get; set; }
        }

        public class üCategory
        {
            public int ID { get; set; }
        }

        public class DateTimeOffsetKeyCustomer
        {
            public DateTimeOffset ID { get; set; }
        }

        public class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual List<DestinationGroup> Parents { get; set; }
        }

        public class DestinationGroup : Destination
        {
            public int GroupLocation { get; set; }
        }

        public class Incident
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }
    }
}