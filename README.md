ASP.NET Core (.NET 5) OData
============= 
Component | Build  | Status 
--------|--------- |---------
ASP.NET Core OData|Rolling | [![Build status](https://identitydivision.visualstudio.com/OData/_apis/build/status/AspNetCoreOData/AspNetCoreOData-master-rolling)](https://identitydivision.visualstudio.com/OData/_build/latest?definitionId=1132) 
ASP.NET Core OData|Nightly | [![Build status](https://identitydivision.visualstudio.com/OData/_apis/build/status/AspNetCoreOData/AspNetCoreOData-master-nightly)](https://identitydivision.visualstudio.com/OData/_build/latest?definitionId=1169)

## 1. Introduction

This is the official ASP.NET Core OData repository.
[ASP.NET Core OData](https://www.nuget.org/packages/Microsoft.AspNetCore.OData/8.0.0-preview) is a server side library built upon ODataLib and ASP.NET Core.

Blogs:
1) [ASP.NET Core OData 8.0 Preview for .NET 5](https://devblogs.microsoft.com/odata/asp-net-odata-8-0-preview-for-net-5/)
2) [Routing in ASP.NET Core OData 8.0 Preview](https://devblogs.microsoft.com/odata/routing-in-asp-net-core-8-0-preview/)

## 2. Basic Usage

In the ASP.NET Core Web Application project, update your `Startup.cs` as below:

```C#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<BookStoreContext>(opt => opt.UseInMemoryDatabase("BookLists"));
        services.AddControllers();
        services.AddOData(opt => opt.AddModel("odata", GetEdmModel()));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
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

### 3.4 Nightly Builds

The nightly build process will upload a NuGet packages for ASP.NET Core OData to:

* https://www.myget.org/gallery/webapinetcore

To connect to webapinightly feed, use this feed URL:

* https://www.myget.org/F/webapinetcore

## 4. Documentation

* [ODataRoutingSample](https://github.com/OData/AspNetCoreOData/tree/master/sample/ODataRoutingSample): ASP.NET Core OData sample project in this repo.

* [ASP.NET OData 8.0 Preview for .NET 5](https://devblogs.microsoft.com/odata/asp-net-odata-8-0-preview-for-net-5/) : A blog introducing the project.

## 5. Community

### 5.1 Contribution

Any contribution, feature request, bug, issue are welcome.

### 5.2 Support

## Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
