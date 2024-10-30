//-----------------------------------------------------------------------------
// <copyright file="DollarFilterEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

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

        return builder.GetEdmModel();
    }
}
