//-----------------------------------------------------------------------------
// <copyright file="Product.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using System;

namespace ODataRoutingSample.Models
{
    [EnableQuery]
    public class Product
    {
        public Guid CRMRecordID { get; set; }

        public string ProductName { get; set; }

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
}
