//-----------------------------------------------------------------------------
// <copyright file="AutoExpandEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public static IEdmModel GetEdmModel1()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Singleton<Root>("Root");
            return builder.GetEdmModel();
        }
    }

    public class Root
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [AutoExpand]
        [Contained]
        public IEnumerable<Expandable1> E1s { get; set; }

        [AutoExpand]
        [Contained]
        public IEnumerable<Expandable2> E2s { get; set; }
    }

    public class Expandable1
    {
        [Key]
        public string Id { get; set; }
    }

    public class Expandable2
    {
        [Key]
        public string Id { get; set; }

        [AutoExpand]
        [Contained]
        public IEnumerable<Expandable1> E1s { get; set; }

        [Contained]
        public IEnumerable<Expendables3> E3s { get; set; }
    }

    public class Expendables3
    {
        [AutoExpand]
        [Contained]
        public IEnumerable<Expandable1> E1s { get; set; }
    }

}
