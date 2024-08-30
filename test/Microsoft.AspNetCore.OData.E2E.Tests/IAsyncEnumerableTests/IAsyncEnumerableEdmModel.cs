//-----------------------------------------------------------------------------
// <copyright file="IAsyncEnumerableEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IAsyncEnumerableTests;

public class IAsyncEnumerableEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Customer>("Customers");
        builder.EntitySet<Order>("Orders");
        IEdmModel model = builder.GetEdmModel();
        return model;
    }
}
