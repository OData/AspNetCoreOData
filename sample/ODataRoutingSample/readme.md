# ASP.NET Core OData (8.x) Sample

---
This is an ASP.NET Core OData 8.x sample project. From this sample, you can see a lot of ASP.NET Core OData 8.x usage.



## Static Routing table

If you run the sample and send the following request in a Web brower:

`~/$odata`, you will get the following (similar) routing table:

![image](https://user-images.githubusercontent.com/9426627/104256721-992da180-5430-11eb-846b-19b02756c084.png)


## OpenAPI/Swagger

If you run the sample and send the following request in a Web brower:

`/swagger`, you will get the following (simiar) swagger page:

![image](../../images/sample_swagger.png)

## Non-Edm model

Non-Edm model means there's no "Edm Model" configed for a route.
For example: the following routing doesn't have the Edm model associated.

```C#
~/api/Accounts
~/api/Accounts/{id}
```

Here's a sample:
`http://localhost:5000/api/accounts?$select=Name&$top=3`

you can get:
```json
[
    {
        "Name": "Warm"
    },
    {
        "Name": "Scorching"
    },
    {
        "Name": "Sweltering"
    }
]
```

Known issue: It seems there are some issues related to the complex property selection.

`http://localhost:5000/api/accounts?$select=HomeAddress`
`http://localhost:5000/api/accounts?$select=HomeAddress($select=City)`
