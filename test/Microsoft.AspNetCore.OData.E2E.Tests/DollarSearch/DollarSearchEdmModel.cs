//-----------------------------------------------------------------------------
// <copyright file="DollarSearchEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarSearch;

public class DollarSearchEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<SearchProduct>("Products");
        builder.EntitySet<SearchCategory>("Categories");
        IEdmModel model = builder.GetEdmModel();
        return model;
    }
}
