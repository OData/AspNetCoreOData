using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataRoutingSample.Models
{
    public static class EdmModelBuilder
    {
   //     private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");

            // function with optional parameters
            var functionWithOptional = builder.EntityType<Product>().Collection.Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<int>("minSalary");
            functionWithOptional.Parameter<int>("maxSalary").Optional();
            functionWithOptional.Parameter<int>("aveSalary").HasDefaultValue("129");

            // overload
            functionWithOptional = builder.EntityType<Product>().Collection.Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<int>("minSalary");
            functionWithOptional.Parameter<string>("name");

            // overload
            functionWithOptional = builder.EntityType<Product>().Collection.Function("GetWholeSalary").ReturnsCollectionFromEntitySet<Order>("Orders");
            functionWithOptional.Parameter<string>("order");
            functionWithOptional.Parameter<string>("name");

            builder.Action("ResetData");

            return builder.GetEdmModel();
        }

        public static IEdmModel GetEdmModelV1()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.Singleton<Customer>("Me");

            var function = builder.Function("RateByOrder");
            function.Parameter<int>("order");
            function.Returns<int>();

            var action = builder.EntityType<Customer>().Action("RateByName");
            action.Parameter<string>("name");
            action.Parameter<int>("age");
            action.Returns<string>();

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
