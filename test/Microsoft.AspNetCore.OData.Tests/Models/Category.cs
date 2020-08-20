// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Tests.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public Product Product { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public IEnumerable<Product> EnumerableProducts { get; set; }

        public IQueryable<Product> QueryableProducts { get; set; }
    }

    public class DerivedCategory : Category
    {
        public string DerivedCategoryName { get; set; }
    }
}
