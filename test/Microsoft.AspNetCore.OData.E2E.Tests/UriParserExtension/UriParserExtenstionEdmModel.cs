//-----------------------------------------------------------------------------
// <copyright file="UriParserExtenstionEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UriParserExtension
{
    public class UriParserExtenstionEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");

            builder.EntityType<Customer>().Function("CalculateSalary").Returns<int>().Parameter<int>("month");
            builder.EntityType<Customer>().Action("UpdateAddress");
            builder.EntityType<Customer>()
                .Collection.Function("GetCustomerByGender")
                .ReturnsCollectionFromEntitySet<Customer>("Customers")
                .Parameter<Gender>("gender");

            return builder.GetEdmModel();
        }
    }
}
