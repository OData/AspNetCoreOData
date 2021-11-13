# Routing Overview

Routes in AspNetCoreOData are built by comparing your `Entity Data Model` (Edm Model)
with the public action methods exposed on a typical ASP.NET Core Controller.

## Initialize OData Routing

You initialize OData Routing by using the `AddOData` method inside the
`ConfigureServices` method of your `Startup` class. For example, given we have
the simplified code samples set up:

<details>
  <summary>Customer.cs</summary>

```C#
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string FavoriteColor { get; set; }
}
```

</details>

<details>
  <summary>CustomersController.cs</summary>

```C#
public class CustomersController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(GetCustomers());
    }

    private static IList<Customer> GetCustomers()
    {
        return new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "Jonier",
                FavoriteColor = 'Red',
            },
            new Customer
            {
                Id = 2,
                Name = "Sam",
                FavoriteColor = 'Blue',
            },
            new Customer
            {
                Id = 3,
                Name = "Peter",
                FavoriteColor = 'Green',
            }
        };
    }
}
```

</details>

<details>
  <summary>Startup.cs</summary>

```C#
. . .
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
. . .
public class Startup
{
    . . .
    public void ConfigureServices(IServiceCollection services)
    {
    . . .
    services.AddControllers()
                .AddOData(opt => opt.AddRouteComponents(GetEdmModel()))
    . . .
    }
    . . .
    private static IEdmModel GetEdmModel() {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Customer>("Customers");
        return builder.GetEdmModel();
    }
}
```

</details>

This will initialize OData routing tied to the Edm Model built from the
`GetEdmModel` method. It will then automatically match up the `Get()` method
to the `Customers` entity set defined in the model and build the two following
OData routes:

```text
GET ~/Customers
GET ~/Customers/$count
GET ~/$metadata
```

> **Note**: The second route there is a special built-in route that allows an API consumer
> to inspect the edm model exposed on this API. Making a get request to that route
> will serialize the edm model into an XML representation referred to as the
> `OData Common Schema Definition Language (CSDL) XML Representation` or often
> just referred to as `CSDL`. It comes as part of the OData standard and is common
> in all implementations of OData. You can [read more about it
> here](http://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html).

## Routing Convention

The way that OData matches Edm Models to routes is referred to as a `Routing Convention`. There are two types of routing conventions available which are:

1. [Convention Routing](./convention-routing.md) - Routes matching the Edm Model are discovered based on
   naming conventions.
2. [Attribute Routing](./attribute-routing.md) - Routes matching the Edm Model are discovered based on
   attributes annotating the controller class or controller methods.

Out of the box, we default to using `Convention Routing` this means that if I
rename the controller from `CustomersController` to `MyCustomersController`
OData will no longer be able to find a route for the `Customers` entity set I
defined in my Edm Model and I will end up with only the following route mounted:

```text
GET ~/$metadata
```

## Changing The Base Path

You can host the API at a different root path by passing a prefix as a string to
the `AddRouteComponents` method like this:

```C#
services.AddControllers()
            .AddOData(opt => opt.AddRouteComponents("v1", GetEdmModel()))
```

This will end up with the following routes getting mounted:

```text
GET ~/v1/Customers
GET ~/Customers/$count
GET ~/v1/$metadata
```

## Route Versioning

You can also specify multiple OData APIs hosted at different prefixes by
adding calling `AddRouteComponents` multiple times and adding the
`[ODataRouteComponent]` attribute to your controllers. For example:

```C#
services.AddControllers()
            .AddOData(opt => opt.AddRouteComponents("v1", GetEdmModel()))
            .AddOData(opt => opt.AddRouteComponents("v2", GetEdmModel2()))
```

<details>
  <summary>v1/CustomersController.cs</summary>

```C#
[ODataRouteComponent("v1")]
public class CustomersController : ODataController
{
    . . .
}
```

</details>

<details>
  <summary>v2/CustomersController.cs</summary>

```C#
[ODataRouteComponent("v2")]
public class CustomersController : ODataController
{
    . . .
}
```

</details>

This will end up with the following routes getting mounted:

```text
# API v1
GET ~/v1/Customers
GET ~/v1/$metadata

# API v2
GET ~/v2/Customers
GET ~/v2/$metadata
```
