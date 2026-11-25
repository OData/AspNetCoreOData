//-----------------------------------------------------------------------------
// <copyright file="DerivedInterfacesDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.E2E.Tests.DerivedInterfaces
{
    public class Customer : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IOrder Order { get; set; }
    }

    public class VipOrder : IOrder
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public string LoyaltyCardNo { get; set; }
    }

    public interface IOrder : IIdentifiable
    {
        public string Product { get; set; }
    }

    public interface IIdentifiable
    {
        public int Id { get; set; }
    }
}
