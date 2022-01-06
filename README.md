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

* [Attribute Routing in ASP.NET Core OData 8.0 RC](https://devblogs.microsoft.com/odata/attribute-routing-in-asp-net-core-odata-8-0-rc/)

* [Routing in ASP.NET Core OData 8.0 Preview](https://devblogs.microsoft.com/odata/routing-in-asp-net-core-8-0-preview/)

* [ASP.NET Core OData 8.0 Preview for .NET 5](https://devblogs.microsoft.com/odata/asp-net-odata-8-0-preview-for-net-5/)

* [API versioning extension with ASP.NET Core OData 8](https://devblogs.microsoft.com/odata/api-versioning-extension-with-asp-net-core-odata-8/)

* [Build formatter extensions in ASP.NET Core OData 8 and hooks in ODataConnectedService](https://devblogs.microsoft.com/odata/build-formatter-extensions-in-asp-net-core-odata-8-and-hooks-in-odataconnectedservice/)

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

That's it. 


## 3. Building, Testing, Debugging and Release

### 3.1 Building and Testing in Visual Studio

Visual Studio 2019 Preview is necessary to build the project.

### 3.2 One-click build and test script in command line
Coming soon.

### 3.3 Debug

The symbol package is uploaded to nuget symbol server. 

It supports source link debug. Remember to make `Enable Source Link support` checked if you debug using Visual Studio.

### 3.4 Nightly Builds

The nightly build process will upload a NuGet packages for ASP.NET Core OData to:

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

Any contribution, feature request, bug, issue are welcome.

### 5.2 Support

### Code of Conduct

This project has adopted the [.NET Foundation Contributor Covenant Code of Conduct](https://dotnetfoundation.org/about/code-of-conduct). For more information see the [Code of Conduct FAQ](https://dotnetfoundation.org/about/faq).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

AspNetCoreOData is a Copyright of &copy; .NET Foundation and other contributors. It is licensed under [MIT License](https://github.com/OData/AspNetCoreOData/blob/main/License.txt)
