//-----------------------------------------------------------------------------
// <copyright file="ParameterAliasEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.ParameterAlias
{
    public static class ParameterAliasEdmModel
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ParameterAliasCustomer>("ParameterAliasCustomers");
            return builder.GetEdmModel();
        }
    }

    public class ParameterAliasCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
