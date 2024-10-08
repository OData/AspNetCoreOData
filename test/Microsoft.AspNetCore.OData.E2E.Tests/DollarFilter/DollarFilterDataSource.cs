//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class DollarFilterDataSource
{
    private static IList<Person> people;
    private static IList<Product> products;

    static DollarFilterDataSource()
    {
        people = new List<Person>
        {
            new Person { Id = 1, SSN = "a'bc" },
            new Person { Id = 2, SSN = "'def" },
            new Person { Id = 3, SSN = "xyz'" },
            new Person { Id = 4, SSN = "'pqr'" }
        };

        products = new List<Product>
        {
            new Product
            {
                Id = "accdx34g-3d16-473e-9251-378c68de859e",
                DeclaredSingleValuedProperty = "a",
                DeclaredCollectionValuedProperty = new List<int> { 1, 2, 3 },
                Properties = new Dictionary<string, object>
                {
                    { "DynamicSingleValuedProperty", "a" },
                    { "DynamicCollectionValuedProperty", new int[] { 1, 2, 3 } }
                }
            },
            new Product
            {
                Id = "abc8fh64-3d16-473e-9251-378c68de859f",
                DeclaredSingleValuedProperty = "b",
                DeclaredCollectionValuedProperty = new List<int> { 2, 3, 4 },
                Properties = new Dictionary<string, object>
                {
                    { "DynamicSingleValuedProperty", "b" },
                    { "DynamicCollectionValuedProperty", new int[] { 2, 3, 4 } }
                }
            },
            new Product
            {
                Id = "abc8fh64-3d16-473e-9251-378c68de859g",
                DeclaredSingleValuedProperty = "c",
                DeclaredCollectionValuedProperty = new List<int> { 3, 4, 5 },
                Properties = new Dictionary<string, object>
                {
                    { "DynamicSingleValuedProperty", "c" },
                    { "DynamicCollectionValuedProperty", new int[] { 3, 4, 5 } }
                }
            }
        };
    }

    public static IList<Person> People => people;

    public static IList<Product> Products => products;
}
