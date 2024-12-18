# Microsoft.AspNetCore.OData

The `Microsoft.AspNetCore.OData` library enables you to create OData endpoints in your ASP.NET Core applications. It provides support for OData query syntax, routing, and model binding, making it easier to build OData services.

## Installation

You can install the `Microsoft.AspNetCore.OData` package via NuGet:

```sh
dotnet add package Microsoft.AspNetCore.OData
```

Or via the NuGet Package Manager Console:

```sh
Install-Package Microsoft.AspNetCore.OData
```

## Getting Started

### Creating an OData Service

Here's a simple example of how to create an OData service using `Microsoft.AspNetCore.OData`:

1. **Create an ASP.NET Core Application**:
- Open Visual Studio and create a new ASP.NET Core Web API project.

2. **Add the `Microsoft.AspNetCore.OData` Package**:
- Install the package using the instructions above.

3. **Define Your Models**:
- Create your data models. For example:

```csharp
namespace MyODataApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

4. **Add an OData Controller**:
- Create a controller to handle OData requests:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using MyODataApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace MyODataApp.Controllers
{
    public class ProductsController : ODataController
    {
        private static List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.0M },
            new Product { Id = 2, Name = "Product 2", Price = 20.0M }
        };

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(products);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            var product = products.FirstOrDefault(p => p.Id == key);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
```

5. **Configure OData in `Startup.cs`**:
- Configure OData routes and services:

- If you work with `Program.cs`, update as below. Refer to the [Getting Started Guide](https://learn.microsoft.com/odata/webapi-8/getting-started).

```csharp
// using statements

var builder = WebApplication.CreateBuilder(args);

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntityType<Order>();
modelBuilder.EntitySet<Customer>("Customers");

builder.Services.AddControllers().AddOData(
    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
        "odata",
        GetEdmModel()));

var app = builder.Build();

// Send "~/$odata" to debug routing if enable the following middleware
// app.UseODataRouteDebug();

app.UseRouting();

app.MapControllers();

app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Product>("Products");
    return builder.GetEdmModel();
}
```

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace MyODataApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOData(opt => opt.AddModel("odata", GetEdmModel()).Filter().Select().Expand().OrderBy().Count().SetMaxTop(100));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Send "~/$odata" to debug routing if enable the following middleware
            // app.UseODataRouteDebug();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Select().Expand().Filter().OrderBy().Count().MaxTop(100);
                endpoints.MapODataRoute("odata", "odata", GetEdmModel());
            });
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            return builder.GetEdmModel();
        }
    }
}
```

6. **Run Your Application**:

- Start your application and navigate to `/odata/Products` to see your OData service in action.

## Documentation

For comprehensive documentation, please refer to the following links:
- [ASP.NET Core OData Overview](https://learn.microsoft.com/odata/webapi-8/overview)
- [Getting Started](https://learn.microsoft.com/odata/webapi-8/getting-started)
- [Fundamentals Overview](https://learn.microsoft.com/odata/webapi-8/fundamentals/overview)
- [Tutorials](https://learn.microsoft.com/odata/webapi-8/tutorials/basic-crud)
- [OData Dev Blogs](https://devblogs.microsoft.com/odata/)
- [OData.org](https://www.odata.org/blog/)

**Blogs**:

* [$compute and $search in ASP.NET Core OData 8](https://devblogs.microsoft.com/odata/compute-and-search-in-asp-net-core-odata-8/)
* [API versioning extension with ASP.NET Core OData 8](https://devblogs.microsoft.com/odata/api-versioning-extension-with-asp-net-core-odata-8/)
* [Build formatter extensions in ASP.NET Core OData 8 and hooks in ODataConnectedService](https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/)
* [Attribute Routing in ASP.NET Core OData 8.0 RC](https://devblogs.microsoft.com/odata/attribute-routing-in-asp-net-core-odata-8-0-rc/)
* [Routing in ASP.NET Core OData 8.0 Preview](https://devblogs.microsoft.com/odata/routing-in-asp-net-core-8-0-preview/)
* [ASP.NET Core OData 8.0 Preview for .NET 5](https://devblogs.microsoft.com/odata/asp-net-odata-8-0-preview-for-net-5/)

## Community

### Contribution

Any contributions, feature requests, bugs and issues are welcome.

### Reporting Security Issues

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) <secure@microsoft.com>. You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the MSRC PGP key, can be found in the [Security TechCenter](https://www.microsoft.com/msrc/faqs-report-an-issue).

### Support

- Issues: Report issues on [Github issues](https://github.com/OData/AspNetCoreOData/issues).
- Questions: Ask questions on [Stack Overflow](http://stackoverflow.com/questions/ask?tags=odata).
- Feedback: Please send mails to [odatafeedback@microsoft.com](mailto:odatafeedback@microsoft.com).
- Team blog: Please visit [https://devblogs.microsoft.com/odata/](https://devblogs.microsoft.com/odata/) and [http://www.odata.org/blog/](http://www.odata.org/blog/).

### Code of Conduct

This project has adopted the [.NET Foundation Contributor Covenant Code of Conduct](https://dotnetfoundation.org/about/policies/code-of-conduct). For more information see the [Code of Conduct FAQ](https://dotnetfoundation.org/about/faq).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

AspNetCoreOData is a Copyright of &copy; .NET Foundation and other contributors. It is licensed under [MIT License](https://github.com/OData/AspNetCoreOData/blob/main/License.txt)
