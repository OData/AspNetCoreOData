//-----------------------------------------------------------------------------
// <copyright file="Product.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace ODataRoutingSample.Models;

public class Product
{
    public int Id { get; set; }

    public string Category { get; set; }

    public Color Color { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? UpdatedDate { get; set; }

    public virtual ProductDetail Detail { get; set; }
}

public class ProductDetail
{
    public string Id { get; set; }

    public string Info { get; set; }
}

public enum Color
{
    Red,

    Green,

    Blue
}
