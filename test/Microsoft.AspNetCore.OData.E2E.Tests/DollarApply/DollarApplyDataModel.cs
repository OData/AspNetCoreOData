//-----------------------------------------------------------------------------
// <copyright file="DollarApplyDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public decimal TaxRate { get; set; }
        public List<Sale> Sales { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Sale> Sales { get; set; }
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
        public decimal BaseSalary { get; set; }
        public Address Address { get; set; }
        public Company Company { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
    }

    public class Company
    {
        [Key]
        public string Name { get; set; }
        public Employee VP { get; set; }
    }
}
