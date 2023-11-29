//-----------------------------------------------------------------------------
// <copyright file="DollarComputeDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute
{
    public class ComputeCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public double Price { get; set; }

        public int Qty { get; set; }

        public IList<string> Candys { get; set; }

        public ComputeAddress Location { get; set; }

        public IList<ComputeSale> Sales { get; set; }

        public IDictionary<string, object> Dynamics { get; set; } = new Dictionary<string, object>();
    }

    public class ComputeAddress
    {
        public string Street { get; set; }

        public int ZipCode { get; set; }
    }

    public class ComputeSale
    {
        public int Id { get; set; }

        public int Amount { get; set; }

        public double TaxRate { get; set; }

        public double Price { get; set; }

        public IDictionary<string, object> Dynamics { get; set; } = new Dictionary<string, object>();
    }
}
