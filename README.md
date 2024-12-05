# ASP.NET Core OData 8.x
---

Component | Build  | Status 
--------|--------- |---------
ASP.NET Core OData|Rolling | [![Build status](https://identitydivision.visualstudio.com/OData/_apis/build/status/AspNetCoreOData/AspNetCoreOData-main-rolling)](https://identitydivision.visualstudio.com/OData/_build/latest?definitionId=1132)
ASP.NET Core OData|Nightly | [![Build status](https://identitydivision.visualstudio.com/OData/_apis/build/status/AspNetCoreOData/AspNetCoreOData-main-nightly)](https://identitydivision.visualstudio.com/OData/_build/latest?definitionId=1169)
.NET Foundation|Release|[![Build status](https://dev.azure.com/dotnet/OData/_apis/build/status/AspNetCoreOData/AspNetCoreOData-main-Yaml-release?branchName=main)](https://dev.azure.com/dotnet/OData/_apis/build/status/AspNetCoreOData/AspNetCoreOData-main-Yaml-release?branchName=main)

## 1. Introduction

**Be noted**:  Switch to use "main" as default branch. 1/6/2022

This is the official ASP.NET Core OData repository.
[ASP.NET Core OData](https://www.nuget.org/packages/Microsoft.AspNetCore.OData/8.0.0) is a server side library built upon ODataLib and ASP.NET Core.

**Blogs**:

* [$compute and $search in ASP.NET Core OData 8](https://devblogs.microsoft.com/odata/compute-and-search-in-asp-net-core-odata-8/)

* [API versioning extension with ASP.NET Core OData 8](https://devblogs.microsoft.com/odata/api-versioning-extension-with-asp-net-core-odata-8/)

* [Build formatter extensions in ASP.NET Core OData 8 and hooks in ODataConnectedService](https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/)

* [Attribute Routing in ASP.NET Core OData 8.0 RC](https://devblogs.microsoft.com/odata/attribute-routing-in-asp-net-core-odata-8-0-rc/)

* [Routing in ASP.NET Core OData 8.0 Preview](https://devblogs.microsoft.com/odata/routing-in-asp-net-core-8-0-preview/)

* [ASP.NET Core OData 8.0 Preview for .NET 5](https://devblogs.microsoft.com/odata/asp-net-odata-8-0-preview-for-net-5/)


#### **Documentation**:

For comprehensive documentation, please refer to the following links:
- [ASP.NET Core OData Overview](https://learn.microsoft.com/odata/webapi-8/overview)
- [Getting Started](https://learn.microsoft.com/odata/webapi-8/getting-started)
- [Fundamentals Overview](https://learn.microsoft.com/odata/webapi-8/fundamentals/overview)
- [Tutorials](https://learn.microsoft.com/odata/webapi-8/tutorials/basic-crud)
- [OData Dev Blogs](https://devblogs.microsoft.com/odata/)
- [OData.org](https://www.odata.org/blog/)

**Example**:
* [ODataRoutingSample](https://github.com/OData/AspNetCoreOData/tree/main/sample/ODataRoutingSample): ASP.NET Core OData sample project in this repo.
  
   - **`~/$odata`** gives a static routing table page of the service
   
   - **`~/swagger`** gives a swagger/openapi page
 
   - Append **`~/$openapi`** to each route gives a raw openapi OData page, for example, **`~/v1/$openapi`**
   
   Please go to [sample](./sample) folder see more samples.
   
 **Solution**:
 * [AspNetCoreOData.sln](AspNetCoreOData.sln):
 
   - Includes **Microsoft.AspNetCore.OData** project, Unit Test, E2E Test & Samples
   
 * [AspNetCoreOData.NewtonsoftJson.sln](AspNetCoreOData.NewtonsoftJson.sln)
 
   - Includes **Microsoft.AspNetCore.OData.NewtonsoftJson** project, Unit Test, E2E Test & Samples

## 2. Basic Usage

### Microsoft.AspNetCore.OData Package Installation
Using .NET CLI:
```bash
dotnet add package Microsoft.AspNetCore.OData
```

Using Package Manager:
```bash
Install-Package Microsoft.AspNetCore.OData
```

In the ASP.NET Core Web Application project, update your `Startup.cs` as below:

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<BookStoreContext>(opt => opt.UseInMemoryDatabase("BookLists"));
        services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Send "~/$odata" to debug routing if enable the following middleware
        // app.UseODataRouteDebug();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private static IEdmModel GetEdmModel()
    {
        // …
    }
}
```

If you work with `Program.cs`, update as below. Refer to the [Getting Started Guide](https://learn.microsoft.com/odata/webapi-8/getting-started).
```c#
// Program.cs
using Lab01.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntityType<Order>();
modelBuilder.EntitySet<Customer>("Customers");

builder.Services.AddControllers().AddOData(
    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
        "odata",
        modelBuilder.GetEdmModel()));

var app = builder.Build();

app.UseRouting();

app.MapControllers();

app.Run();

static IEdmModel GetEdmModel()
{
    // …
}
```

That's it. 


## 3. Building, Testing, Debugging and Release

### 3.1 Building and Testing in Visual Studio

Visual Studio 2022 is required to build the source project in order to support the `DateOnly` and `TimeOnly` types, which were introduced in .NET 6.

### 3.2 One-click build and test script in command line
Coming soon.

### 3.3 Debug

The symbol package is uploaded to nuget symbol server. 

It supports source link debug. Remember to check `Enable Source Link support` if you debug using Visual Studio.

### 3.4 Nightly Builds

The nightly build process will upload NuGet packages for ASP.NET Core OData to:

* https://www.myget.org/gallery/webapinetcore

To connect to webapinightly feed, use this feed URL:

* https://www.myget.org/F/webapinetcore/api/v3/index.json (Your NuGet V3 feed URL (Visual Studio 2015+)

* https://www.myget.org/F/webapinetcore/api/v2 Your NuGet V2 feed URL (Visual Studio 2012+)

## 4. Documentation

* [ODataRoutingSample](https://github.com/OData/AspNetCoreOData/tree/main/sample/ODataRoutingSample): ASP.NET Core OData sample project in this repo.

* [ASP.NET OData 8.0 Preview for .NET 5](https://devblogs.microsoft.com/odata/asp-net-odata-8-0-preview-for-net-5/): A blog introducing the project.

* [Our docs folder](./docs): Our current documentation

## 5. Community

### 5.1 Contribution

Any contributions, feature requests, bugs and issues are welcome.

### 5.2 Reporting Security Issues

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) <secure@microsoft.com>. You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the MSRC PGP key, can be found in the [Security TechCenter](https://www.microsoft.com/msrc/faqs-report-an-issue). You can also find these instructions in this repo's [SECURITY.md](./SECURITY.md).

### 5.3 Support

- Issues: Report issues on [Github issues](https://github.com/OData/AspNetCoreOData/issues).
- Questions: Ask questions on [Stack Overflow](http://stackoverflow.com/questions/ask?tags=odata).
- Feedback: Please send mails to [odatafeedback@microsoft.com](mailto:odatafeedback@microsoft.com).
- Team blog: Please visit [https://devblogs.microsoft.com/odata/](https://devblogs.microsoft.com/odata/) and [http://www.odata.org/blog/](http://www.odata.org/blog/).

### Code of Conduct

This project has adopted the [.NET Foundation Contributor Covenant Code of Conduct](https://dotnetfoundation.org/about/policies/code-of-conduct). For more information see the [Code of Conduct FAQ](https://dotnetfoundation.org/about/faq).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

AspNetCoreOData is a Copyright of &copy; .NET Foundation and other contributors. It is licensed under [MIT License](https://github.com/OData/AspNetCoreOData/blob/main/License.txt)
