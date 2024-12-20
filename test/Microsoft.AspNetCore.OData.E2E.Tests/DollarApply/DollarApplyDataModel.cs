//-----------------------------------------------------------------------------
// <copyright file="DollarApplyDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public decimal TaxRate { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Sale
    {
        
        public int Id { get; set; }
        public int Year { get; set; }
        public string Quarter { get; set; }
        public Customer Customer { get; set; }
        public Product Product { get; set; }
        public decimal Amount { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Salary { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; }
    }
}
