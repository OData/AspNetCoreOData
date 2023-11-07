//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ODataRoutingSample.Models
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModelV1()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Organization>("Organizations");
            builder.EntitySet<Department>("Departments");
            builder.EntitySet<ForwardingPolicy>("ForwardingPolicies");

            builder.EntitySet<Policy>("Policies");

            var function = builder.Function("RateByOrder");
            function.Parameter<int>("order");
            function.Returns<int>();

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
    }
}
