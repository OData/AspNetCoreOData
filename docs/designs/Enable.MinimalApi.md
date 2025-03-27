# Enable OData Minimal API (Design Draft)
See issue: https://github.com/OData/AspNetCoreOData/issues/578

## Problem
Minimal APIs are a simplified approach for building HTTP APIs fast with ASP.NET Core. Customers can build fully functioning REST endpoints with minimal code and configuration, especially without controller, action and even the formatters.
OData works with controller/action, but OData developers also want to enable OData query on minimal API pattern.

## Prerequisites

Here’s the basic minimal API service app:
```C#
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build(); 
app.MapGet("/customers", () => new Customer[] { …. });
app.Run();
```
This service provides a Web API as:

`GET {host}/customers`

This API returns a collection of customers, then OData developers want to enable OData functionalities on this API, for example:

`GET {host}/customers?$select=name&$top=1`

### Route handlers
The lambda expression in preceding `MapGet` is called <strong>Route Handler</strong>. They are methods that execute when the route matches. Route handlers can be:

- A lambda expression, 
- A local function, 
- An instance method
- A static method
- Or a RequestDelegate
  
where:
```C#
public delegate Task RequestDelegate(HttpContext context);
```
Route handlers can be synchronous or asynchronous.

## Scenarios

### Enable OData response

We enable developers to call an extension method named `WithODataResult()` on the route handler to get 'OData format' response:

![image](https://github.com/user-attachments/assets/9203a3e5-baa1-4168-a5f1-2556d792bfc2)

<strong>Be noted, </strong>It only contains the 'Id' and 'Name' in the OData format. This is because an Edm model is built on the fly since we don't provide the model explicity. In this case, all other properties are built as navigation properties by default.

Developers can call `WithODataModel(model)` to provide an Edm model explicitly, in this case, the serialization of OData format can use that model directly.

Developers can also call `WithODataVersion(version)` to get different OData format. See below:

![image](https://github.com/user-attachments/assets/f6e9bac5-9f33-4bde-ba90-c57206139cf3)

Except the above extension methods, we want to provide more to enable developers to customize the OData format response:

* WithODataServices(lambda) : to config the services within dependency injection
* WithODataBaseAddressFactory(lambda): to config the base address for OData format, especially for the context URI
* WithODataPathFactory(lambda) : to config the related OData path for the request

For example:
![image](https://github.com/user-attachments/assets/b7f91007-0e6d-478d-a564-519671a441fb)



## Scenarios
We want to support the following scenarios for OData Minimal API:

1. Enable binding `ODataQueryOptions<T>` as route handler parameter and Empower developers to call ApplyTo manually.
For example,
```C#
app.MapGet("/customers", (ODataQueryOptions<Customer> queryOptions) =>
{
      var data = "new Customer[] { …. }");
      return queryOptions.ApplyTo(data);
}
```
In this usage, we can re-use most functionalities from the existing `ODataQueryOptions<T>` class.

2. Empower customers to do <strong>query filter</strong> implicitly.
For example:
```C#
app.MapGet("/customers", [EnableQuery] () => "new Customer[] { …. }");
```
In this case, we can reuse EnableQueryAttribute.
Unfortunately, it seems this filter mechanism is not supported in minimal API.
On the contrary, we will use the ‘Endpoint Filter’ to achieve similar functionality. Below is the sample codes, for Endpoint Filters more details [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters?view=aspnetcore-9.0).
```C#
app.MapGet("/customers",  () => "new Customer[] { …. }").AddODataQueryFilter(…);
```

3. Provide a HttpRequest-less `ODataQueryOptions<T>` (Without using IEdmModel, using CLR type directly) and customers can use it in the Delegate to generate the Linq Expression and apply to the data.

```C#
app.MapGet("/customers", (HttpRequest request) =>
{
      var data = "new Customer[] { …. }");
      var query = ODataQueryBuilder.BuildFromString<Customer>(request.QueryString);
      return query.ApplyTo(data);
}
```
I prefer this solution, but we haven’t finished the 'independent' `ODataQueryBuilder` from query string. Moreover, this can be designed and implemented individually, so I’d skip this functionality and leave it for later.

## ODataQueryOptions< T >
To enable `ODataQueryOptions<T>` as route handler parameter in Minimal API, we should figure out two parts:
1.	How to get parameter binding for `ODataQueryOptions<T>`.
2.	How to provide the Edm model related.
   
### Parameter binding
In controller/action scenario, [model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0) is used to bind `ODataQueryOptions<T>` as action parameter. In Minimal API, there’s no model binding because it’s “MINIMAL”. To enable it as route handler parameter, we should ‘customize’ parameter binding for `ODataQueryOptions<T>`.
There are two ways to customize parameter binding:
1.	For route, query, and header binding sources, bind custom types by adding a static `TryParse` method for the type.
2.	Control the binding process by implementing a `BindAsync` method on a type.

For parameter binding in minimal API, see [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-9.0)

I prefer option #2 because it provides `HttpContext` and parameter information for more functionality. Therefore, we will add a static method in `ODataQueryOptions<T>` as:
```C#
public class ODataQueryOptions<T> : ODataQueryOptions
{
    ……
   public static ValueTask<ODataQueryOptions<T>> BindAsync(HttpContext context, ParameterInfo parameter)
   {
            // 1) Get or generate Edm model
           //  2) Build ODataQueryOptions<T> and return
   }
}
```

### Edm model providing
So far, the Edm model is needed to do OData query binding. There are two ways to provide the Edm model.
- From the Global configuration (Model Based)

  For example:
  we use the `AddRouteComponents` based on `ODataOptions` to config the Edm model.

```C#
      builder.Services.AddOData(opt => opt.EnableQueryFeatures()
          .AddRouteComponents("customized", EdmModelBuilder.GetEdmModel())
```

  Then, we use an extension method to config the Edm model as:

```C#
    app.MapGet("/customers", (ODataQueryOptions<Customers> options) => {
          // …
       }).UseOData("customized");
```

    Cons: 
        1. First, we need the “prefix” twice to map the route handler and Edm Model. 
        2. Second, it could be confusing with the existing controller/action mode.

- Build the Edm model on the fly (Edm model-less)
 Use the generic type of `T` in the `ODataQueryOptions<T>` to build the Edm model on the fly.
 Note: the model should be built only one time, and we should cache it to get better performance.

- Provide the model for certain endpoints.
   For example, to provide the directly for a certain endpoint as:

```C#
    app.MapGet("/customers", (ODataQueryOptions<Customers> options) => {
          // …
        }).UseModel(new EdmModel(…));
```

I prefer second and third pattern and keep `AddRouteComponents` for controller-action only.

### OData Query Endpoint Filter

Minimal API filters allow developers to implement business logic that supports:
-	Running code before and after the endpoint handler.
-	Inspecting and modifying parameters provided during an endpoint handler invocation.
-	Intercepting the response behavior of an endpoint handler

So, we can do the OData query using the minimal API filter. More details:
1)	We can do OData query validation before the endpoint handler. (It is powerful).
2)	We can apply the OData query on the data after the endpoint handler.
   
Basically, that’s the same logic comparing to `EnableQueryAttribute`. But, minimal API has its own filter logic/pipeline.

#### EndpointFilter vs EndpointFilterFactory
There are two ways to add filter on route handler.

1)	`AddEndpointFilter`
   
which registers a standard endpoint filter directly onto a route handler

2)	`AddEndpointFilterFactory`
   
which allows you to create a filter dynamically based on the context of the endpoint using a factory function, enabling more flexible filter application based on the endpoint's details like its handler signature.

So far, a standard endpoint filter is enough for OData query.

#### IODataQueryEndpointFilter

The below design is based on the standard endpoint filter.
We will create a new interface as below:
```C#
public interface IODataQueryEndpointFilter : IEndpointFilter
{
    ValueTask OnFilterExecutingAsync(ODataQueryFilterInvocationContext context);

    ValueTask<object> OnFilterExecutedAsync(object responseValue, ODataQueryFilterInvocationContext context);
}
```
Where:

•	`IEndpointFilter` is an interface from Minimal API as the following definition:

```C#
public interface IEndpointFilter
{
    ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next);
}
```

•	`DataQueryFilterInvocationContext` is a simple wrapper to wrap the following information:

```C#
public class ODataQueryFilterInvocationContext
{
      public MethodInfo MethodInfo { get; init; }

      public EndpointFilterInvocationContext InvocationContext   { get; init; }
}
```
We will provide a default `IODataQueryEndpointFilter` implementation as below:

```C#
public class ODataQueryEndpointFilter : IODataQueryEndpointFilter
{
    // ...
    public virtual async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next)
    {
        // ...
        var odataFilterContext = new ODataQueryFilterInvocationContext { MethodInfo = methodInfo, InvocationContext = invocationContext };

        await OnFilterExecutingAsync(odataFilterContext);

        // calling into next filter or the route handler.
        var result = await next(invocationContext);

        var finalResult = await OnFilterExecutedAsync(result, odataFilterContext);

        return finalResult;
    }
}
```

in which the logic is:

-	Minimal API request pipeline will call ‘InvokeAsync’ on the filter one by one.
-	In ODataQueryFilter.InvokeAsync, OnFilterExecutingAsync is called before route handler. In which, we will construct the ODataQueryOptions<T> and run OData query validation on it.
-	Call Route handler
-	OnFilterExecutedAsync is called after route handler. In which, we will use the ODataQueryOptions<T> to bind the OData query on the data.

### Extension methods
We will provide extension methods as follows to empower developers to enable OData query filter easily:

```C#
public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder, IODataQueryFilter queryFilter) =>
builder.AddEndpointFilter(queryFilter);

public static RouteHandlerBuilder AddODataQueryEndpointFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this RouteHandlerBuilder builder)
    where TFilterType : IODataQueryFilter =>
    builder.AddEndpointFilter<TFilterType>();

 // more for overloads 
```

#### Query configuration

OData Query configuration has the following types:

1) ModelBound Query settings: Config the query functionalities on the C# types and properties. (Skip in this design)
2) DefaultQueryConfigurations: Config whether the certain query options is enabled or disabled. 
3) ODataValidatationSettings: Config for the query validatation. For example, is the query top value bigger than a certain value?
4) ODataQuerySettings: Config for query executing. For example, set the PageSize, etc.

Why do we have them? I don't know. :(

`DefaultQueryConfigurations` is global level configuration, see the section about 'AddOData()'.
For `ODataValidatationSettings` and `ODataQuerySettings`, I have the following overload for the extensions:

```C#
public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder,
    Action<ODataValidationSettings> validationSetup = default,
    Action<ODataQuerySettings> querySetup = default)
```
and
```C#
public static RouteGroupBuilder AddODataQueryEndpointFilter(this RouteGroupBuilder builder,
    Action<ODataValidationSettings> validationSetup = default,
    Action<ODataQuerySettings> querySetup = default)
```

So, developers can use the Action to config the settings as:

```C#
app.MapGet("/myschools", (AppDb db) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return db.Schools;
})
    .WithModel(model)
    .AddODataQueryEndpointFilter(querySetup: q => q.PageSize = 3);
```


### Edm model providing for endpoind filter

Same as `ODataQueryOptions<T>` parameter binding, Endpoint filter needs the Edm model to the OData query.
We have a couple ways

a)	Build the Edm model on the fly (Edm model-less)

It’s by fault if no model provided. We will build the Edm model using the return type of route handler.

b)	Provide the model for certain endpoints.

We will add extension methods to accept the Edm model associated with the endpoint filter. 

```C#
public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder, IODataQueryFilter queryFilter, IEdmModel model) =>
builder.AddEndpointFilter(queryFilter);
```

c)	Use  ‘UseModel(IEdmModel)’ for Endpoint Filter.

I prefer not to do ‘b’ since we can achieve the same thing using ‘c’.

## Serialization

The result of OData query will be serialized as normal JSON payload. Be noted, it doesn’t contain the OData control metadata, for example @odata.context.

I think that’s true because:

- It’s minimal API, developers only need the data with query functionalities.
- More important, Minimal API removes the formatters because and just because it’s minimal.
  
In this case, we must add the JsonConverter to all OData query related classes, for example: SelectSome<T>.


## Deserialization 

I’d like to seek more scenarios for OData deserialization in Minimal API. (Brainstorm?)

One of such scenarios is to support ‘Delta<T>’ for Patch/Put request. 
If this is valid?? scenario, we can do same parameter binding for `Delta<T>` as `ODataQueryOptions<T>`. 
Of course, developers can get the same data using IDictionary<string, object> as a replacement for ‘Delta<T>’. 
Let’s have more discussion about it and it’s scope it out now.

## Customize the services
Developers may need to customize/extend the services used during OData query, for example, to implement the ISearchBinder by himself. 

We can do it:

-	Allow to config the OData related services into global DI, then we can retrieve it if have. There’s no harm since we only use it if the global DI has it. Otherwise, use the default services.
-	Allow to config the OData related services into the Endpoint metadata. Then we can retrieve it from metadata if have. 

I’d prefer to use the DI not use the metadata Since metadata provides the ‘data’, the DI provides the services. We can have more discussion about this.

## About ODataOptions

`DataOptions` are used widely in controller/action pattern for OData. But, in the minimal API pattern, I’d not use it to config OData query because:

1)	Some configurations in ODataOptions are used to config the ODat routing related. In minimal API, they are useless.
2)	Some APIs in ODataOptions are used to config the endpoint, for example ‘AddRouteComponents’, but now, it’s not used again in minimal API.
3)	DefaultQueryConfigurations in ODataOptions is used to config whether the OData query options are enabled or not. So far, it’s still needed in minimal API. So, we’d provide similar configuration in the new ‘AddOData’ extension method.

## New AddOData()

As mentioned, we need configure minimal services used in OData minimal query services into service provider (DI).
The existing `AddOData(..)` is extension methods defined on IMvcBuilder or IMveBuilderCore. In minimal API, there are not used. So, we will add new `AddOData()` extension methods on `IServiceCollection` directly:

```C#
public static IServiceCollection AddOData(this IServiceCollection services, Action<DefaultQueryConfigurations> setupAction)
{
    // add the minimal services used for OData query 
}
```
