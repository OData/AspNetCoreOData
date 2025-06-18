//-----------------------------------------------------------------------------
// <copyright file="DollarComputeEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute;

public class DollarComputeEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<ComputeCustomer>("Customers");
        builder.EntitySet<ComputeSale>("Sales");

        builder.EntitySet<ComputeShopper>("Shoppers");
        IEdmModel model = builder.GetEdmModel();
        return model;
    }
}
