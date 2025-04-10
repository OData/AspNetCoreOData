//-----------------------------------------------------------------------------
// <copyright file="CustomersEndpoints.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace ODataMiniApi.Students;

/// <summary>
/// Add customers endpoint
/// </summary>
public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder app, IEdmModel model)
    {
        app.MapGet("/customers", (AppDb db) => db.Customers.Include(s => s.Orders))
           //.WithODa
            .WithODataResult() // default: built the complex type property by default?
            // If enable Query, define them as entity type
            // If no query, define them as complex type?
            .WithODataModel(model)
            .WithODataVersion(ODataVersion.V401)
            .WithODataBaseAddressFactory(c => new Uri("http://abc.com"));
            //.WithODataServices(c => c.AddSingleton<ODataMessageWriterSettings>;

        app.MapGet("v1/customers", (AppDb db, ODataQueryOptions<Customer> queryOptions) =>
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // This line seems required otherwise it will throw exception
            return queryOptions.ApplyTo(db.Customers.Include(s => s.Orders));
        })
            .WithODataResult()
            .WithODataModel(model)
        ;

        // Be noted, [ODataModelConfiguration] configures the 'Info' as complex type
        // So, in this case, the 'Info' property on 'Customer' is a structural property, not a navigation property.
        app.MapGet("v11/customers", (AppDb db, [ODataModelConfiguration] ODataQueryOptions<Customer> queryOptions) =>
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return queryOptions.ApplyTo(db.Customers.Include(s => s.Orders));
        });

        app.MapGet("v2/customers", (AppDb db) =>
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return db.Customers.Include(s => s.Orders);
        })
            .AddODataQueryEndpointFilter()
            .WithODataResult()
            //.WithODataModel(model)
            ;

        // To discuss? To provide the MapODataGet, MapODataPost, MapODataPatch....
        // It seems we cannot generate the Delegate easily.
        app.MapODataGet("v3/customers", (AppDb db) =>
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return db.Customers.Include(s => s.Orders);
        });
        return app;
    }

    // Scenarios using groups
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app, IEdmModel model)
    {
        var group = app.MapGroup("")
            .WithODataResult()
            .WithODataModel(model);

        group.MapGet("v0/orders", (AppDb db) => db.Orders)
            ;

        group.MapGet("v1/orders", (AppDb db, ODataQueryOptions<Order> queryOptions) =>
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // This line seems required otherwise it will throw exception
            return queryOptions.ApplyTo(db.Orders);
        })
          //  .WithODataResult()
          //  .WithODataModel(model)
            ;

        group.MapGet("v2/orders", (AppDb db) =>
        {
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return db.Orders;
        })
            .AddODataQueryEndpointFilter()
            //.WithODataResult()
            //.WithODataModel(model)
            ;

        return app;
    }

}
