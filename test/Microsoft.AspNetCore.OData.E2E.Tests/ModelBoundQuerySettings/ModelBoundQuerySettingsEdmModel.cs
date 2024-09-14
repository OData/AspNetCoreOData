//-----------------------------------------------------------------------------
// <copyright file="ModelBoundQuerySettingsEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ModelBoundQuerySettings;

public class ModelBoundQuerySettingsEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Author>("Authors");
        builder.EntitySet<Book>("Books");

        return builder.GetEdmModel();
    }

    public static IEdmModel GetEdmModelByModelBoundAPI()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Author>("Authors").EntityType.Filter("Books");
        builder.EntitySet<Book>("Books").EntityType.Filter("BookId");

        return builder.GetEdmModel();
    }
}
