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
    private static List<Product> products;
    private static List<Customer> customers;

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
                    { "DynamicCollectionValuedProperty", new int[] { 1, 2, 3 } },
                    { "DynamicMixedCollectionValuedProperty", new object[] { "a", 2, 3 } }
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
                    { "DynamicCollectionValuedProperty", new int[] { 2, 3, 4 } },
                    { "DynamicMixedCollectionValuedProperty", new object[] { 2, "b", 4 } }
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
                    { "DynamicCollectionValuedProperty", new int[] { 3, 4, 5 } },
                    { "DynamicMixedCollectionValuedProperty", new object[] { 3, 4, "c" } }
                }
            }
        };

        customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Properties = new Dictionary<string, object>
                {
                    {
                        "UntypedCollectionProperty",
                        new object[]
                        {
                            Color.Black,
                            2,
                            new Address { Street = "Broadway Street" },
                            "Sue",
                            new object[] { "x", "y", "z" },
                            null
                        }
                    }
                }
            },
            new Customer
            {
                Id = 2,
                Addresses = new List<Address>
                {
                    new Address
                    {
                        Street = "One Microsoft Way",
                        Properties = new Dictionary<string, object>
                        {
                            { "Floors", new [] { 7, 8, 9, 10, 11 } }
                        }
                    },
                    new Address
                    {
                        Street = "Park Avenue",
                        Properties = new Dictionary<string, object>
                        {
                            { "Floors", new [] { 5 } }
                        }
                    }
                },
                Properties = new Dictionary<string, object>
                {
                    { "UntypedCollectionProperty", new [] { 3, 4 } }
                }
            }
        };
    }

    public static IList<Person> People => people;

    public static List<Product> Products => products;

    public static List<Customer> Customers => customers;
}
