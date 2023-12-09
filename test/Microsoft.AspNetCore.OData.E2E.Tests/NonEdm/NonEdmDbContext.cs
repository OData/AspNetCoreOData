//-----------------------------------------------------------------------------
// <copyright file="NonEdmDbContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NonEdm
{
    public class NonEdmDbContext
    {
        private static IList<Customer> _customers;

        public static IList<Customer> GetCustomers()
        {
            if (_customers == null)
            {
                Generate();
            }
            return _customers;
        }

        private static void Generate()
        {
            _customers = Enumerable.Range(1, 10).Select(e =>
                new Customer
                {
                    Id = e,
                    Name = "Customer #" + e,
                    Gender = e%2 == 0 ? Gender.Female : Gender.Male,
                }).ToList();
        }
    }

}
