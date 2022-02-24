//-----------------------------------------------------------------------------
// <copyright file="Order.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataAlternateKeySample.Models
{
    /// <summary>
    /// Entity type with multiple alternate keys
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Guid Token { get; set; }

        public decimal Price { get; set; }

        public int Amount { get; set; }
    }
}
