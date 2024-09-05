//-----------------------------------------------------------------------------
// <copyright file="DateTimeOffsetEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DateTimeOffsetSupport;

public class DateTimeOffsetEdmModel
{
    public static IEdmModel GetExplicitModel()
    {
        ODataModelBuilder builder = new ODataModelBuilder();
        var fileType = builder.EntityType<File>().HasKey(f => f.FileId);
        fileType.Property(f => f.Name);
        fileType.Property(f => f.CreatedDate);
        fileType.Property(f => f.DeleteDate);

        var files = builder.EntitySet<File>("Files");
        return builder.GetEdmModel();
    }

    public static IEdmModel GetConventionModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<File>("Files");
        return builder.GetEdmModel();
    }
}
