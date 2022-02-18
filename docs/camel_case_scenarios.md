# Camel case scenarios

## how to enabel camel-case in $select

Given an OData service as following configuration (It's a non-Edm scenario):
```C#
builder.Services.AddControllers().AddOData(options => options.Select().Filter().OrderBy());
```

If you send request: 
```C#
https://localhost:7158/WeatherForecast
```

The response looks like:
```json
[
    {
        "date": "2022-02-08T14:56:26.7139132-08:00",
        "temperatureC": -3,
        "temperatureF": 27,
        "summary": "Mild"
    },
    
    ...
```

The property name is camel case. However, if you have an OData $select query as:

```
https://localhost:7158/WeatherForecast?$select=summary
```
The response looks like:
```json
[
  {
    "Summary": "Cool"
  },
  {
    "Summary": "Warm"
  }
...
]

```

The property name is not camel-case. 

## The root problem is that: 

$select will generate a select expand wrapper, which will output the selected property as a 'dictionary'. The key of dictionary is the property name, the value of dictionary is the selected property value.
In order to make the dictionary key as camel case, you have to config the 'DictionaryKeyPolicy` for JSON serializer.

Here's the configuration:

```C#
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    })
    .AddOData(options => options.Select().Filter().OrderBy());
```

Now, 
```
https://localhost:7158/WeatherForecast?$select=summary
```
has the following response:

```json
[
    {
        "summary": "Cool"
    },
    {
        "summary": "Warm"
    },
    {
        "summary": "Chilly"
    },
    {
        "summary": "Balmy"
    },
    {
        "summary": "Balmy"
    }
]
```
