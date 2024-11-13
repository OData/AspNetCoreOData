//-----------------------------------------------------------------------------
// <copyright file="DollarFilterEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter
{
    public class DollarFilterEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Person>("People");
            builder.ComplexType<VendorAddress>();
            builder.ComplexType<VendorCity>();
            builder.ComplexType<NonOpenVendorAddress>();
            builder.EntitySet<Vendor>("Vendors");
            builder.EntitySet<Vendor>("BadVendors");
            builder.ComplexType<Address>();
            builder.ComplexType<NonOpenAddress>();
            builder.ComplexType<ContactInfo>();
            builder.ComplexType<PropertyIsNotCollectionContactInfo>();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Customer>("BadCustomers");
            builder.EntitySet<Product>("Products");
            builder.ComplexType<Fruit>();
            builder.EntitySet<Basket>("Baskets");
            builder.EntitySet<BasicType>("BasicTypes");
            builder.ComplexType<LiteralInfo>();
            builder.EnumType<Color>();

            return builder.GetEdmModel();
        }
    }
}
