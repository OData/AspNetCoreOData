//-----------------------------------------------------------------------------
// <copyright file="DerivedTypesDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class VipCustomer : Customer
    {
        public string LoyaltyCardNo { get; set; }
    }

    public class GoldCustomer : VipCustomer
    {
        public List<Order> BulkOrders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
    }
}
