//-----------------------------------------------------------------------------
// <copyright file="Order.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace ODataRoutingSample.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public virtual Category Category { get; set; }
    }

    public class VipOrder : Order
    {
        public virtual Category VipCategory { get; set; }
    }
}
