# Routing Pitfalls

This is a collection of common pitfalls an OData developer should be aware
of when building an API using this project.

## Mixing ASP.NET Routing and OData Routing in one controller

### Scenario Edm Model

```C#
public static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Customer>("Customers");
    return builder.GetEdmModel();
}
```

### Incorrect Code ❌

```C#
[ApiController]
[Route("api/[controller]")]
public class CustomersController : Controller
{
    [HttpGet]
    EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Expand)]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {}
}
```

Since "Customers" is a valid entity set in the Edm model, you will wind up with
two routable endpoints exposed:

1. `~/api/Customers` -- A non-odata endpoint
2. `~/odata/Customers` -- An odata endpoint

### Correct Code ✅

```C#
public class CustomersController : ODataController
{
    [HttpGet]
    EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Expand)]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {}
}
```
