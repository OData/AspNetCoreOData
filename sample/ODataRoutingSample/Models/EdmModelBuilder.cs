//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataRoutingSample.Models
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Person>("People").EntityType.HasKey(c => new { c.FirstName, c.LastName });

            // use the following codes to set the order and change the route template.
            // builder.EntityType<Person>().Property(c => c.FirstName).Order = 2;
            // builder.EntityType<Person>().Property(c => c.LastName).Order = 1;

            // function with optional parameters
            var functionWithOptional = builder.EntityType<Product>().Collection.Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<int>("minSalary");
            functionWithOptional.Parameter<int>("maxSalary").Optional();
            functionWithOptional.Parameter<string>("aveSalary").HasDefaultValue("129");

            // overload
            functionWithOptional = builder.EntityType<Product>().Collection.Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<int>("minSalary");
            functionWithOptional.Parameter<double>("name");

            // overload
            functionWithOptional = builder.EntityType<Product>().Collection.Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<string>("order");
            functionWithOptional.Parameter<string>("name");

            // function with only one parameter (optional)
            functionWithOptional = builder.EntityType<Product>().Collection.Function("GetOptional").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<string>("param").Optional();

            // unbound
            builder.Action("ResetData");

            // using attribute routing
            var unboundFunction = builder.Function("CalculateSalary").Returns<string>();
            unboundFunction.Parameter<int>("minSalary");
            unboundFunction.Parameter<int>("maxSalary").Optional();
            unboundFunction.Parameter<string>("wholeName").HasDefaultValue("abc");
            return builder.GetEdmModel();
        }

        public static IEdmModel GetEdmModelV1()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Organization>("Organizations");
            builder.EntitySet<Department>("Departments");
            builder.EntitySet<Company>("Companies");
            builder.EntitySet<Customer>("Customers");
            builder.Singleton<Customer>("Me");

            var function = builder.Function("RateByOrder");
            function.Parameter<int>("order");
            function.Returns<int>();

            var action = builder.EntityType<Customer>().Action("RateByName");
            action.Parameter<string>("name");
            action.Parameter<int>("age");
            action.Returns<string>();

            // bound action
            ActionConfiguration boundAction = builder.EntityType<Customer>().Action("BoundAction");
            boundAction.Parameter<int>("p1");
            boundAction.Parameter<Address>("p2");
            boundAction.Parameter<Color?>("color");
            boundAction.CollectionParameter<string>("p3");
            boundAction.CollectionParameter<Address>("p4");
            boundAction.CollectionParameter<Color?>("colors");

            // bound function for organization
            var productPrice = builder.EntityType<Organization>().Collection.
                Function("GetPrice").Returns<string>();
            productPrice.Parameter<string>("organizationId").Required();
            productPrice.Parameter<string>("partId").Required();

            productPrice = builder.EntityType<Organization>().Collection.
                Function("GetPrice2").ReturnsCollectionFromEntitySet<Organization>("Organizations");
            productPrice.Parameter<string>("organizationId").Required();
            productPrice.Parameter<string>("partId").Required();
            productPrice.IsComposable = true;

            // Add a composable function
            var getOrgByAccount =
                builder.EntityType<Organization>()
                .Collection
                .Function("GetByAccount")
                .ReturnsFromEntitySet<Organization>("Organizations");
            getOrgByAccount.Parameter<int>("accountId").Required();
            getOrgByAccount.IsComposable = true;

            builder.EntityType<Organization>().Action("MarkAsFavourite");

            // Add another composable function
            var getOrgByAccount2 =
                builder.EntityType<Organization>()
                .Collection
                .Function("GetByAccount2")
                .ReturnsCollectionFromEntitySet<Organization>("Organizations"); // be noted, it returns collection.
            getOrgByAccount2.Parameter<int>("accountId").Required();
            getOrgByAccount2.IsComposable = true;

            return builder.GetEdmModel();
        }

        public static IEdmModel GetEdmModelV2()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Order>("Orders");

            builder.Singleton<Order>("VipOrder");
            builder.Singleton<Category>("Categories");

            var functionWithComplexTypeParameter = builder.EntityType<Order>().Function("CanMoveToAddress").Returns<bool>();
            functionWithComplexTypeParameter.Parameter<Address>("address");

            var functionWithCollectionComplexTypeParameter = builder.EntityType<Order>().Collection.Function("CanMoveToManyAddress").Returns<bool>();
            functionWithCollectionComplexTypeParameter.CollectionParameter<Address>("addresses");

            // function with optional parameters
            var functionWithOptional = builder.EntityType<Order>().Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<int>("minSalary");
            functionWithOptional.Parameter<int>("maxSalary").Optional();
            functionWithOptional.Parameter<int>("aveSalary").HasDefaultValue("129");

            // Function 1
            var function = builder.Function("RateByOrder");
            function.Parameter<int>("order");
            function.Returns<int>();

            // Function 2
            function = builder.Function("CalcByOrder");
            function.Parameter<string>("name");
            function.Parameter<int>("order");
            function.Returns<int>();

            return builder.GetEdmModel();
        }

        public static IEdmModel GetEdmModelV3()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<DriverTenant>("tenants");
            builder.EntitySet<DriverDevice>("devices");
            builder.EntitySet<DriverFolder>("folders");
            builder.EntitySet<DriverPage>("pages");

            builder.EntitySet<TestEntity>("TestEntities");
            return builder.GetEdmModel();
        }
    }
}
