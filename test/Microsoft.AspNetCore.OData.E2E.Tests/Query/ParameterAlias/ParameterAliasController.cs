//-----------------------------------------------------------------------------
// <copyright file="ParameterAliasController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.ParameterAlias
{
    public class ParameterAliasCustomersController : ODataController
    {
        private static readonly ParameterAliasCustomer[] Customers =
        {
        new ParameterAliasCustomer { Id = 1, Name = "Customer 1" },
        new ParameterAliasCustomer { Id = 2, Name = "Customer 2" },
        new ParameterAliasCustomer { Id = 3, Name = "Customer 3" }
    };

        [EnableQuery]
        public IQueryable<ParameterAliasCustomer> Get()
        {
            return Customers.AsQueryable();
        }
    }
}
