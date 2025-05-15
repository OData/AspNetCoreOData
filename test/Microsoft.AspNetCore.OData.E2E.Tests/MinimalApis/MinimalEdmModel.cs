//-----------------------------------------------------------------------------
// <copyright file="MinimalEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public class MinimalEdmModel
{
    private static IEdmModel _model;

    public static IEdmModel GetEdmModel()
    {
        if (_model == null)
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<MiniTodo>("Todos");
            builder.ComplexType<MiniTask>(); // by default to make it as complex type
            _model = builder.GetEdmModel();
        }

        return _model;
    }

    private static IEdmModel _entitySetModel;
    public static IEdmModel GetAllEntitySetEdmModel()
    {
        if (_entitySetModel == null)
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<MiniTodo>("Todos");
            builder.EntitySet<MiniTask>("Tasks");
            _entitySetModel = builder.GetEdmModel();
        }

        return _entitySetModel;
    }
}


