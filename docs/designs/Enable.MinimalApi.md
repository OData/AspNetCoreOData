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

Developers can call `WithODataModel(model)` to provide an Edm model (`Info` property is built as complex type property in the pre-built Edm Model) explicitly, in this case, the serialization of OData format can use that model directly.

Developers can also call `WithODataVersion(version)` to get different OData format. See below:

![image](https://github.com/user-attachments/assets/f6e9bac5-9f33-4bde-ba90-c57206139cf3)

Except the above extension methods, we want to provide more to enable developers to customize the OData format response:

* WithODataServices(lambda) : to config the services within dependency injection
* WithODataBaseAddressFactory(lambda): to config the base address for OData format, especially for the context URI
* WithODataPathFactory(lambda) : to config the related OData path for the request

For example:
![image](https://github.com/user-attachments/assets/b7f91007-0e6d-478d-a564-519671a441fb)

### Enable OData query explicity

We want to enable binding `ODataQueryOptions<T>` as route handler parameter and empower developers to call `ApplyTo()` explicitly as follows:

![image](https://github.com/user-attachments/assets/04ef199b-7985-4f95-ad0f-8a1a51ea2a70)

As mentioned, the model is built on the fly and all complex properties are built as navigation properties. That's why `$expand=info` is used.

Developers can combine the extension methods `WithOData*()` to get other payload. For example:

![image](https://github.com/user-attachments/assets/b3a5b34d-d82d-4e7b-99b2-9324370eec33)

In this case, developer should use `$select=info` to get the value since `info` is a complex type property in the pre-built Edm model.

### Enable OData query implicitly

We enable developers to call an extension method named `AddODataQueryEndpointFilter(...)` on the route handler to enable OData query functionality implicitly.

![image](https://github.com/user-attachments/assets/7bc9f9be-5cb9-471a-be3a-a6e7fb860eae)

Again, developer can combine other extensions method together to get other result, for example:

![image](https://github.com/user-attachments/assets/22c0d9d2-8666-4b2b-91df-c1bb0b19cb98)


### Use RouteHandlerGroup

If lots of same route handlers have the same metadata, developers can enable the metadata on the Group. 

```C#
var group = app.MapGroup("")
    .WithODataResult()
    .WithODataModel(model);

group.MapGet("v0/orders", (AppDb db) => db.Orders);

group.MapGet("v1/orders", (AppDb db, ODataQueryOptions<Order> queryOptions) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // This line seems required otherwise it will throw exception
    return queryOptions.ApplyTo(db.Orders);
});

group.MapGet("v2/orders", (AppDb db) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return db.Orders;
})
    .AddODataQueryEndpointFilter();
```

In this scenario, all three endpoints use the same 'Model' and return OData format response.

Developer can also call `WithOData*()` method on certain route handler to overwrite the metadata on the group.


## Design Details

### ODataResult

We can get OData format repsone by implementing a custom `IResult` type. 

We create a class named `ODataResult` as below

```C#
public interface IODataResult
{
    object Value { get; }
}

internal class ODataResult : IResult, IODataResult, IEndpointMetadataProvider
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ...
    }

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        // ... Think more: will we need this to do something? maybe no needed.
        // Maybe we don't need 'WithODataResult()' method and use this to update the metadata?
    }
}
```

Make `ODataResult` as internal class to hide the details and it maybe change later. Developer can call `WithODataResult()` extension methods to enable a route handler to return OData format response.

### WithODataResult()

`WithODataResult()` is an extension method as below:

```C#
public static TBuilder WithODataResult<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
{
    builder.AddEndpointFilter(async (invocationContext, next) =>
    {
        object result = await next(invocationContext);

        // If it's null or if it's already the ODataResult, simply do nothing
        if (result is null || result is ODataResult)
        {
            return result;
        }

        return new ODataResult(result);
    });

    // Add/update odata metadata
}
```

Be noted, `With` prefix means to add/update endpoint metadata.


### Other WithOData*() extensions

Other `WithOData*()` are similiar extension methods same as `WithODataResult()`, For example:

```C#
public static TBuilder WithODataModel<TBuilder>(this TBuilder builder, IEdmModel model) where TBuilder : IEndpointConventionBuilder
{
     // Add/update odata metadata
}
```

All of them are used to add/update certain part of OData metadata.

### OData metadata

We define a class named `ODataMiniMetadata` to hold the metadata used for OData functionalities.

All `WithOData*()` add `ODataMinimetadata` if it's non-existed and update part of its content.

```C#
public class ODataMiniMetadata
{
    public IEdmModel Model { get; set; }
    public bool IsODataFormat { get; set; }
    public Func<HttpContext, Type, ODataPath> PathFactory { get; set; }
    ......
}
```

###  Parameter binding for ODataQueryOptions< T >

In controller/action scenario, [model binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-9.0) is used to bind `ODataQueryOptions<T>` as action parameter. In Minimal API, there’s no model binding because it’s “MINIMAL”. To enable it as route handler parameter, we should 'customize' parameter binding for `ODataQueryOptions<T>`.
There are two ways to customize parameter binding:
1.	For route, query, and header binding sources, bind custom types by adding a static `TryParse` method for the type.
2.	Control the binding process by implementing a `BindAsync` method on a type.

For parameter binding in minimal API, see [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-9.0)

I prefer option #2 because it provides `HttpContext` and parameter information for more functionaliiesy. As a result, we add a static method in `ODataQueryOptions<T>` as:
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

In this usage, we can re-use most functionalities from the existing `ODataQueryOptions<T>` class.

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
We create a new interface as below:
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
-	In `ODataQueryEndpointFilter.InvokeAsync`, `OnFilterExecutingAsync` is called before route handler. In which, we will construct the `ODataQueryOptions<T>` and run OData query validation on it.
-	Call Route handler
-	`OnFilterExecutedAsync` is called after route handler. In which, we will use the `ODataQueryOptions<T>` to bind the OData query on the data.


We will provide extension methods as follows to empower developers to enable OData query filter easily:

```C#
public static RouteHandlerBuilder AddODataQueryEndpointFilter(this RouteHandlerBuilder builder, IODataQueryFilter queryFilter) =>
builder.AddEndpointFilter(queryFilter);

public static RouteHandlerBuilder AddODataQueryEndpointFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFilterType>(this RouteHandlerBuilder builder)
    where TFilterType : IODataQueryFilter =>
    builder.AddEndpointFilter<TFilterType>();

 // more for overloads 
```

### Edm model providing
So far, the Edm model is needed to do OData query binding. Especially, it's required to do the OData format response. 

There are two ways to provide the Edm model as mentioned.

- Build the Edm model on the fly (Edm model-less)
 
 If no `WithODataModel()` is called, we build the required Edm model on the fly.

 a) Without `ODataQueryOptions<T>`, we build the Edm model using the type of returned value.
 b) With `ODataQueryOptions<T>`, we build the Edm model using the generic type of `T`.
 
 Note: the model should be built only one time, and we should cache it to get better performance.

 If `ODataMiniMetadata` existed, the built model is cached on it. Otherwise, it is cached in the singleton service named `IEndpointModelMapper`. If we find a better place, we can replace this global/static cache.


- Use `WithODataModel()` extension

For example:

```C#
    app.MapGet("/customers", (ODataQueryOptions<Customers> options) => {
          // …
        }).WithODataModel(new EdmModel(…));
```


### OData Options/Configuration

## New AddOData()

As mentioned, we need configure minimal services used in OData minimal query services into service provider (DI).
The existing `AddOData(..)` is extension methods defined on IMvcBuilder or IMveBuilderCore. In minimal API, there are not used. So, we add new `AddOData()` extension methods on `IServiceCollection` directly:

```C#
public static IServiceCollection AddOData(this IServiceCollection services, Action<ODataMinOptions> setupAction)
{
    // add the minimal services used for OData query 
}
```

Where, `ODataMiniOptions` is used to config the global settings. 

```C#
public class ODataMiniOptions
{
    public DefaultQueryConfigurations QueryConfigurations { get => _queryConfigurations; }

    public ODataVersion Version { get; set; } = ODataVersionConstraint.DefaultODataVersion;

    public bool EnableNoDollarQueryOptions { get; set; } = true;

    public bool EnableCaseInsensitive { get; set; } = true;
}
```

So, developers can enable all OData query options like:

```C#
builder.Services.AddOData(q => q.EnableAll());
```

`DefaultQueryConfigurations` in `ODataMiniOptions` is global level configuration. Developer can config query options for a certain route handler by calling `WithODataOptions(lambda)`.

```C#
app.MapGet("/customer", (AppData db) => {....})
   . WithODataOptions(opt => ...);
```
The options on route handler has high priority.

### Each query configuration

OData Query configuration has the following types:
 
1) ODataValidatationSettings: Config for the query validatation. For example, is the query top value bigger than a certain value?
2) ODataQuerySettings: Config for query executing. For example, set the PageSize, etc.


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
    .WithODataModel(model)
    .AddODataQueryEndpointFilter(querySetup: q => q.PageSize = 3);
```


## Serialization

As mentioned, 

1) Without enable OData result, the serialization result is normal JSON payload.
2) `WithODataResult()` called, the result is OData JSON payload.


The exsiting serializers are used to do serialization OData JSON payload. Typically, we don't need to reinvite the wheel.

To support the Normal JSON payload after enabling OData query options, we need to config the JSON converter for the OData wrapper class, for example: `SelectAll<T>`

So, we will register the JSON converter into the `JsonOptions` when calling `AddOData()`.

## Deserialization 

I’d like to seek more scenarios for OData deserialization in Minimal API. (Brainstorm?)

One of such scenarios is to support ‘Delta<T>’ for Patch/Put request. 
If this is valid scenario, we can do same parameter binding for `Delta<T>` as `ODataQueryOptions<T>`. 
Of course, developers can get the same data using IDictionary<string, object> as a replacement for ‘Delta<T>’. 
Let’s have more discussion about it and it’s scope it out now.

## Customize the services
Developers may need to customize/extend the services used during OData query/serialization, for example, to implement the ISearchBinder by himself. 

We provide `WithODataService(lambda)` extension method to enable developer to customize the services.

The service provider is cached in the metadata if `ODataMiniMetadata ` existed.

## TBD



