# OData WebApi Authorization extensions

This library uses the permissions defined in the [capability annotations](https://github.com/oasis-tcs/odata-vocabularies/blob/master/vocabularies/Org.OData.Capabilities.V1.md) of the OData model to apply authorization policies
to an OData service based on `Microsoft.AspNetCore.OData`.

## Usage

In your `Startup.cs` file:

```c#
using Microsoft.AspNet.OData.Extensions
```
```c#
public void ConfigureServices(IServiceCollection services)
{
    // you need to register an authentication scheme/handler
    services.AddAuthentication().AddScheme(/* ... */);
    // you need to add AspNetCore authorization services
    services.AddAuthorization();

    // odata authorization services
    services.AddODataAuthorization();

    service.AddRouting();
}
```
```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();
    // OData register authorization middleware
    app.UseOdataAuthorization();

    app.UseEndpoints(endpoints => {
        endpoints.MapODataRoute("odata", "odata", GetEdmModel());
    });
}
```

### How to specify permission scopes?

By default, the library will try extract permissions from the
authenticated user's claims. Specifically, it will look for
claims with the key `Scope`. If your app is storing user scopes differently (e.g. using a different key), you can provider a scope finder delegate that returns a list of scopes from the current user:

```c#
services.AddODataAuthorization(options => {
    options.ScopeFinder = (context) => {
        var scopesClaim = context.User?.FindFirst("Permissions");
        return Task.FromResult(scopes.Value.Split(" ").AsEnumerable());
    };
})
```

For a complete working example, check [the sample application](../../sample/ODataAuthorizationSample).

## How permissions are applied

On each request, the library extract from the model the permissions restrictions that should apply to the route being accessed and creates an authorization policy based on those permissions. Deeper down the request pipeline, the AspNetCore filter-based authorization system will call the OData authorization handler to verify whether the current user's permissions match the ones required by the policy.

### CRUD operations on entity sets and singleton

For CRUD operations on entity sets and singleton, the permissions of the corresponding insert/update/delete/read restrictions are applied.

Endpoint                     | Restrictions applied
-----------------------------|----------------------
`GET Customers`              | `ReadRestrictions` of Customers entity set
`GET Customers(1)`            | `ReadByKeyRestrictions` of the `ReadRestrictions` of `Customers` (does not apply to singletons)
`DELETE Customers/1`         | `DeleteRestrictions` of `Customers`
`POST Customers`             | `InsertRestrictions` of `Customers`
`PUT Customers`              | `UpdateRestricitons` of `Customers`
`PATCH Customers`            | `UpdateRestrictions` of `Customers`

### Function and Action calls

The `OperationRestricitons` of the function or action are applied. For function and action imports, the `OperationRestrictions` of the underlying function/action are applied.

Endpoint                    | Restrictions applied
----------------------------|-----------------------
`GET Orders(1)/CalculateTax` | `OperationRestrictions` of `CalculateTax`
`POST SetTaxRate`            | `OperationRestrictions` of `SetTaxRate`

**Note**: If functions are overloaded, the operation restrictions of the specific overload being called will apply.

### Operations on properties

The `ReadRestrictions` or `UpdateRestrictions` of the entity or singleton whose property are being accessed are applied.

Endpoint                         | Restrictions applied
---------------------------------|----------------------
`GET Customers(1)/Address/City` | `ReadByKeyRestrictions` of `ReadRestrictions` of `Customers`
`GET TopProduct/Price`          | `ReadRestrictions` of `TopProduct`
`DELETE or PUT or POST Customers(1)/Email` | `UpdateRestrictions` of `Customers`

### Operations on navigation property links

These apply the `ReadRestrictions` and `UpdateRestrictions` of the entity/singleton that contains the navigation property where the link is read/added/removed/modified.

Endpoint                         | Restrictions applied
---------------------------------|----------------------
`GET Customers(1)/Orders/$ref` | `ReadByKeyRestrictions` of `ReadRestrictions` of `Customers`
`GET TopCustomer/Orders/$ref`          | `ReadRestrictions` of `TopProduct`
`DELETE or PUT or POST Customers(1)/Orders/$ref` | `UpdateRestrictions` of `Customers`

### Navigation properties

Navigation properties are treated as a the start of a new navigation source. In terms of permissions, the last navigation property in the route will be considered the entity set/singleton whose permissions should apply:

Endpoint                     | Restrictions applied
-----------------------------|--------------------------
`GET Customers(1)/Orders`    | `ReadRestrictions` of `Orders`
`GET Customers(1)/Orders(1)/Price`| `ReadByKeyRestrictions` from `ReadRestrictions` of `Orders`
`DELETE Customers(1)/Orders(1)` | `DeleteRestrictions` of `Orders`
`PUT Customers(1)/Orders(1)`   | `UpdateRestrictions` of `Orders`
`POST Customers(1)/Orders`     | `InsertRestrictions` of `Orders`


## Limitations
- Only supports AspNetCore APIs using endpoing routing, i.e. AspNetCore 3.1
- Does not support `NavigationRestrictions`
- Does not support [`RestrictedProperties`](https://github.com/oasis-tcs/odata-vocabularies/blob/master/vocabularies/Org.OData.Capabilities.V1.md#scopetype)
- Permissions are extracted from the model on each request, no caching is performed. It's not clear whether it's guaranteed that the model will not change after startup.