using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Results.MinimalAPIResults;
using MinimalApisTestProj.Models;

namespace MinimalApisTestProj.EndPoints
{
    public class CustomerEndPoints
    {
        public static List<Customer> Customers = new()
        {
            new() { Id = 1, Name = "Customer1" },
            new() { Id = 2, Name = "Customer2" }
        };


        public static ODataResult GetAllCustomers()
        {
            return Results.Extensions.ODResult(Customers.AsQueryable());
        }

        public static ODataResult PostCustomer(ODataBinding<Customer> customer)
        {
            Customers.Add(customer.Value);

            return Results.Extensions.ODResult(Customers.AsQueryable()); 
        }

        public static IQueryable<Customer> GetCustomers()
        {
            return Customers.AsQueryable();
        }
    }
}
