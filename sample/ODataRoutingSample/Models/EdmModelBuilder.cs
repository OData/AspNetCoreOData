// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataRoutingSample.Models
{
    public static class EdmModelBuilder
    {
   //     private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Person>("People").EntityType.HasKey(c => new { c.FirstName, c.LastName });

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
    }
}
