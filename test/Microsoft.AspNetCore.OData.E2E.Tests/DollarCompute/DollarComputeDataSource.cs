//-----------------------------------------------------------------------------
// <copyright file="DollarComputeDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute
{
    public class DollarComputeDataSource
    {
        private static IList<ComputeCustomer> _customers;

        static DollarComputeDataSource()
        {
            GenerateCustomers();
        }

        public static IList<ComputeCustomer> Customers => _customers;

        private static void GenerateCustomers()
        {
            _customers = new List<ComputeCustomer>
            {
                new ComputeCustomer { Id = 1, Name = "Peter", Age = 19, Price = 1.99, Qty = 10, Candys = new List<string>(){ "kit kat"} },
                new ComputeCustomer { Id = 2, Name = "Sam",   Age = 40, Price = 2.99, Qty = 15, Candys = new List<string>(){ "BasicBerry"} },
                new ComputeCustomer { Id = 3, Name = "John",  Age = 34, Price = 6.99, Qty = 4, Candys = new List<string>(){ "Snickers", "Hershey"} },
                new ComputeCustomer { Id = 4, Name = "Kerry", Age = 29, Price = 3.99, Qty = 15, Candys = new List<string>(){ "Snickers", "M&M"} },
                new ComputeCustomer { Id = 5, Name = "Alex",  Age = 08, Price = 9.01, Qty = 20, Candys = new List<string>(){ "M&M", "Twix"} },
            };
            int[] zipcode = { 98029, 32509, 98052, 88309, 12304 };

            for (int i = 1; i <= 5; i++)
            {
                _customers[i - 1].Location = new ComputeAddress
                {
                    Street = "Street " + i,
                    ZipCode = zipcode[i - 1]
                };

                _customers[i - 1].Sales = Enumerable.Range(0, i + 3)
                    .Select(idx => new ComputeSale { Id = 100 * i + idx, Amount = idx + i, Price = (3.1 + idx) * i, TaxRate = (0.1 + idx + i) / 10.0 }).ToList();
            }
        }
    }
}
