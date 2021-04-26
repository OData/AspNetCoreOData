// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataMvcSample.Models
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.Singleton<Customer>("Me");

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

            return builder.GetEdmModel();
        }
    }
}
