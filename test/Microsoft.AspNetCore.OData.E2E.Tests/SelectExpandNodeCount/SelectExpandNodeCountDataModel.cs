//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SelectExpandNodeCount
{
    public class SelectExpandNodeCountItem
    {
        public int Id { get; set; }

        public string P1 { get; set; }

        public string P2 { get; set; }

        public string P3 { get; set; }

        public string P4 { get; set; }

        public string P5 { get; set; }

        public SelectExpandNodeCountItem Related { get; set; }
    }

    public static class SelectExpandNodeCountDataSource
    {
        private static readonly IList<SelectExpandNodeCountItem> items = CreateItems();

        public static IList<SelectExpandNodeCountItem> Items => items;

        private static IList<SelectExpandNodeCountItem> CreateItems()
        {
            SelectExpandNodeCountItem item3 = new SelectExpandNodeCountItem
            {
                Id = 3,
                P1 = "a3",
                P2 = "b3",
                P3 = "c3",
                P4 = "d3",
                P5 = "e3"
            };

            SelectExpandNodeCountItem item2 = new SelectExpandNodeCountItem
            {
                Id = 2,
                P1 = "a2",
                P2 = "b2",
                P3 = "c2",
                P4 = "d2",
                P5 = "e2",
                Related = item3
            };

            SelectExpandNodeCountItem item1 = new SelectExpandNodeCountItem
            {
                Id = 1,
                P1 = "a1",
                P2 = "b1",
                P3 = "c1",
                P4 = "d1",
                P5 = "e1",
                Related = item2
            };

            return new List<SelectExpandNodeCountItem> { item1, item2, item3 };
        }
    }
}
