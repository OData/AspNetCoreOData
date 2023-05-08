using System;
using System.Collections.Generic;
using Microsoft.OData.AzureFunctions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.OData.AzureFunctionsSample
{
    public class EdmModelProvider : IEdmModelProvider
    {
        public EdmModelProvider()
        {
        }
        public IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            return builder.GetEdmModel();
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Postcode { get; set; }
        public List<Order> Orders { get; set; }
    }

    public class Order
    { 
        public int Id { get; set; }
        public string OrderName { get; set; }
        public DateTime TimeOrderMade { get; set; }
    }
}
