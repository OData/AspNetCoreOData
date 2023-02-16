//-----------------------------------------------------------------------------
// <copyright file="AutoExpandEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AutoExpand
{
    public class AutoExpandEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntityType<SpecialOrder>();
            builder.EntityType<VipOrder>();
            builder.EntitySet<ChoiceOrder>("OrderChoices");
            builder.EntitySet<NormalOrder>("NormalOrders");
            builder.EntityType<DerivedOrder>();
            builder.EntityType<DerivedOrder2>();
            builder.EntitySet<OrderDetail>("OrderDetails");
            builder.EntitySet<People>("People");
            builder.EntitySet<Menu>("EnableQueryMenus");
            builder.EntitySet<Menu>("QueryOptionsOfTMenus");
            builder.EntitySet<Tab>("Tabs");
            builder.EntitySet<Item>("Items");
            builder.EntitySet<Note>("Notes");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }
    }
}
