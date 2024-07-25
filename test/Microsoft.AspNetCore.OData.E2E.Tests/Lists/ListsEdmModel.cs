//-----------------------------------------------------------------------------
// <copyright file="ListsEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    internal class ListsEdmModel
    {
        public static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Product> Products = builder.EntitySet<Product>("Products");          

            builder.Namespace = typeof(Product).Namespace;

            var edmModel = builder.GetEdmModel();
            return edmModel;
        }

           }
}
