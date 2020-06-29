# OData WebApi Authorization sample

This application demonstrates how to use the OData authorization extensions to apply permissions to OData endpoints based on the model capability restrictions.

The applicaiton defines a model with CRUD permission restrictions annotations on the `Customers` entity set and the
`GetTopCustomer` unbound function.

It uses a custom authentication handler that assumes a
user is always authenticated. This handler extracts the permissions from a header called `Permissions`, which
is a comma-separated list of allowed scopes.

Based on the model annotations, the:

| Endpoint                 | Required permissions
---------------------------|----------------------
`GET /odata/Customers`     | `Customers.Read`
`GET /odata/Customers/1`   | `Customers.ReadByKey`
`DELETE /odata/Customers/1`| `Customers.Delete`
`POST /odata/Customers`    | `Customers.Insert`
`GET /odata/GetTopCustomer`| `Customers.GetTop`

To test the app, run it and open Postman. In Postman
add a header called `Permissions` and any of the permissions
specified above in a comma-separated list (e.g. `Customers.Read, Customers.Insert`), then make requests to the endpoints above. If you hit an endpoint without adding its required permissions to the header, you should get a `403 Forbidden` error.
