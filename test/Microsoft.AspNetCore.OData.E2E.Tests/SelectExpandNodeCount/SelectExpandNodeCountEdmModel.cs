//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SelectExpandNodeCount
{
    public static class SelectExpandNodeCountEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SelectExpandNodeCountItem>("LimitedProjectionItems");
            builder.EntitySet<SelectExpandNodeCountItem>("DefaultProjectionItems");
            builder.EntitySet<SelectExpandNodeCountItem>("LevelsProjectionItems");
            return builder.GetEdmModel();
        }
    }
}
