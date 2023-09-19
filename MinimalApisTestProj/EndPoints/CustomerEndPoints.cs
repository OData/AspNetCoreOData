using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results.MinimalAPIResults;
using Microsoft.Extensions.Options;
using MinimalApisTestProj.Models;

namespace MinimalApisTestProj.EndPoints
{
    public static class CustomerEndPoints
    {
        public static List<Customer> Customers = new()
        {
            new() { Id = 1, Name = "Customer1" },
            new() { Id = 2, Name = "Customer2" }
        };

        public static ODataResult GetAllCustomers()
        {
            return Results.Extensions.ODataQuery(Customers.AsQueryable());
        }

        public static ODataResult GetCustomer(int id)
        {
            var customer = Customers.Where(c => c.Id == id);

            return Results.Extensions.ODataQuery(customer);
        }

        public static ODataResult PostCustomer(ODataBinding<Customer> customer)
        {
            Customers.Add(customer.Value);

            return Results.Extensions.ODataQuery(Customers.AsQueryable());
        }

        public static ODataResult PatchCustomer(int id, ODataBinding<Delta<Customer>> customer)
        {
            var original = Customers.FirstOrDefault(a => a.Id == id);

            customer.Value.Patch(original);

            return Results.Extensions.ODataQuery(original);
        }

        public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("customers", GetAllCustomers)
                .AddEndpointFilter<EnableQueryFilter>();

            builder.MapGet("customers/{id:int}", GetCustomer);

            builder.MapGet("customers({id:int})", GetCustomer);

            builder.MapPost("customers", PostCustomer);

            builder.MapPatch("customers({id:int})", PatchCustomer);
            builder.MapPatch("customers/{id:int}", PatchCustomer);

            return builder;
        }
    }

}
