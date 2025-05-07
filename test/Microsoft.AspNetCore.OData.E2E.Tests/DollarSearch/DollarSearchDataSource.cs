//-----------------------------------------------------------------------------
// <copyright file="DollarSearchDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarSearch;

public class DollarSearchDataSource
{
    private static IList<SearchProduct> _products;
    private static IList<SearchCategory> _categories;
    private static IList<SearchTag> _tags;

    static DollarSearchDataSource()
    {
        GenerateProducts();
    }

    public static IList<SearchProduct> Products => _products;

    public static IList<SearchCategory> Category => _categories;

    public static IList<SearchTag> Tags => _tags;

    private static void GenerateProducts()
    {
        _products = new List<SearchProduct>
        {
            new SearchProduct { Id = 1, Name = "Sugar", Color = SearchColor.White, Price = 1.99, Qty = 10 }, // food
            new SearchProduct { Id = 2, Name = "Pencil", Color = SearchColor.Blue, Price = 3.99, Qty = 15 }, // office
            new SearchProduct { Id = 3, Name = "Coffee", Color = SearchColor.Brown, Price = 2.99, Qty = 15 }, // food
            new SearchProduct { Id = 4, Name = "Phone",  Color = SearchColor.Red, Price = 9.01, Qty = 20 }, // device
            new SearchProduct { Id = 5, Name = "Paper",  Color = SearchColor.White, Price = 6.99, Qty = 4 }, // office
            new SearchProduct { Id = 6, Name = "TV",  Color = SearchColor.Red, Price = 9.01, Qty = 20 }, // device
        };

        _categories = new List<SearchCategory>
        {
            new SearchCategory { Id = 1, Name = "Food" },
            new SearchCategory { Id = 2, Name = "Office" },
            new SearchCategory { Id = 3, Name = "Device" },
        };

        _tags = [
            new SearchTag { Id = 1, Name = "Telemetry", Description ="Data collected from product usage" },
            new SearchTag { Id = 2, Name = "Privacy", Description ="Indicates privacy-sensitive content" },
            new SearchTag { Id = 3, Name = "SDK", Description ="Related to software development kits" },
            new SearchTag { Id = 4, Name = "Deprecated", Description ="No longer in use" },
        ];

        _products[0].Category = _categories[0];
        _products[0].Tags = [_tags[0], _tags[2], _tags[3]];

        _products[2].Category = _categories[0];
        _products[2].Tags = [_tags[1], _tags[2], _tags[3]];

        _products[1].Category = _categories[1];
        _products[1].Tags = [_tags[1], _tags[2]];

        _products[4].Category = _categories[1];
        _products[4].Tags = [_tags[0], _tags[1]];

        _products[3].Category = _categories[2];
        _products[3].Tags = [_tags[2], _tags[3]];

        _products[5].Category = _categories[2];
        _products[5].Tags = [_tags[0], _tags[2]];
    }
}
