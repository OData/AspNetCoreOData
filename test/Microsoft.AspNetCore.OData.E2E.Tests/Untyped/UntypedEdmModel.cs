//-----------------------------------------------------------------------------
// <copyright file="UntypedEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Untyped;

public class UntypedEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<InModelPerson>("People");
        builder.EntitySet<InModelPerson>("Managers");
        builder.ComplexType<InModelAddress>();
        builder.EnumType<InModelColor>();
        IEdmModel model = builder.GetEdmModel();
        return model;
    }
}
