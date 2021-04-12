# A customized routing Sample

---
This is an ASP.NET Core OData 8.x customized routing sample project.

## Static Routing table

If you run the sample and send the following request in a Web brower:

`~/$odata`, you will get the routing table

## Usage

If you send request as: `http://localhost:5000/odata/Customer`

you can get:
```json
{
    "@odata.context": "http://localhost:5000/odata/$metadata#Collection(Edm.String)",
    "value": [
        "classname=Customer",
        "Customer",
        "Car",
        "School"
    ]
}
```

You can also try:
`http://localhost:5000/odata/car`
`http://localhost:5000/odata/school`

It works for case-insensitive entity set name.

For others, you will get 404.
