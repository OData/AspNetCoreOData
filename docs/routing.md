# Introduction

I'd like to use this doc to list typical routing scenarios:

Let's use this model:

```C#
public static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Customer>("Customers");
    return builder.GetEdmModel();
}
```

## Mixing ASP.NET Routing and OData Routing in one controller

Be noted, we should avoid this scenario. It's better to create two controllers,
one for ASP.NET routing, the other for OData routing.

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

If "Customers" is the valid entity set of the Edm model.
There's two endpoints:
* `~/api/Customers`  -- non-odata
* `~/odata/Customers`  -- odata
