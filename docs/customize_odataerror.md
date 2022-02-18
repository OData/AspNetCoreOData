# How to customize the error

Related issue: https://github.com/OData/AspNetCoreOData/issues/473

## Problem

Customer wants to customize the output error. 

## Solution

Customer can customize the error serializer to change the output error.

## Codes

Create the serializer provider as follows:

```C#
public class MySerializerProvider : ODataSerializerProvider
{
    public MySerializerProvider(IServiceProvider sp) : base(sp)
    {
    }

    public override IODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
    {
        if (type == typeof(ODataError) || type == typeof(SerializableError))
        {
            return new MyErrorSerializer();
        }

        return base.GetODataPayloadSerializer(type, request);
    }
}
```

Create MyErrorSerializer as follows:
```C#
public class MyErrorSerializer : ODataErrorSerializer
{
    public override Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
    {
        if (graph is SerializableError error)
        {
            ODataError oDataError = error.CreateODataError();
            oDataError.InnerError = null;  // you can change or add more things to the inner error

            return base.WriteObjectAsync(oDataError, typeof(ODataError), messageWriter, writeContext);
        }
        else if (graph is ODataError oDataError)
        {
            oDataError.InnerError = null;  // you can change or add more things to the inner error
            return base.WriteObjectAsync(oDataError, typeof(ODataError), messageWriter, writeContext);
        }

        return base.WriteObjectAsync(graph, type, messageWriter, writeContext);
    }
}
```

Register it in startup
```C#
 services.AddControllers()
   .AddOData(opt => opt.AddRouteComponents("odata", EdmModelBuilder.BuildBookModel(), service => service.AddSingleton<IODataSerializerProvider, MySerializerProvider>()));
```

## Test

```C#
http://localhost:1059/ai/students?$filter= status eq 'new'
```

will get the following error:

```json
{
    "error": {
        "code": "",
        "message": "The query specified in the URI is not valid. The string 'new' is not a valid enumeration type constant.",
        "details": []
    }
}
```

