// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace ODataRoutingSample.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Category { get; set; }

        public Color Color { get; set; }

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
