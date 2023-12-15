# ASP.NET Core OData (8.x) Minimal API Sample

---
This is an ASP.NET Core OData 8.x minimal API project. 

Minimal APIs are a simplified approach for building fast HTTP APIs with ASP.NET Core. You can build fully functioning REST endpoints with minimal code and configuration. Skip traditional scaffolding and avoid unnecessary controllers by fluently declaring API routes and actions. 



## Basic endpoints

1) GET `http://localhost:5177/schools`

```json
[
    {
        "schoolId": 1,
        "schoolName": "Mercury Middle School",
        "mailAddress": {
            "apartNum": 241,
            "city": "Kirk",
            "street": "156TH AVE",
            "zipCode": "98051"
        },
        "students": null
    },
    {
        "schoolId": 2,
        ...
    }
    ...
]    
```

2) GET `http://localhost:5177/schools?$expand=mailaddress&$select=schoolName&$top=2`

```json
[
    {
        "MailAddress": {
            "ApartNum": 241,
            "City": "Kirk",
            "Street": "156TH AVE",
            "ZipCode": "98051"
        },
        "SchoolName": "Mercury Middle School"
    },
    {
        "MailAddress": {
            "ApartNum": 543,
            "City": "AR",
            "Street": "51TH AVE PL",
            "ZipCode": "98043"
        },
        "SchoolName": "Venus High School"
    }
]
```

3) GET `http://localhost:5177/schools/5?$expand=students($top=1)&$select=schoolName`

```json
{
    "Students": [
        {
            "StudentId": 50,
            "FirstName": "David",
            "LastName": "Padron",
            "FavoriteSport": "Tennis",
            "Grade": 77,
            "SchoolId": 5,
            "BirthDay": "2015-12-03"
        }
    ],
    "SchoolName": "Jupiter College"
}
```

4) GET `http://localhost:5177/customized/schools?$select=schoolName,mailAddress&$orderby=schoolName&$top=1`

This endpoint uses the `OData model` from configuration, which builds `Address` as complex type, so we can use `$select`

```json
[
    {
        "SchoolName": "Earth University",
        "MailAddress": {
            "ApartNum": 101,
            "City": "Belly",
            "Street": "24TH ST",
            "ZipCode": "98029"
        }
    }
]
```

## Student CRUD endpoints

I grouped student endpoints under `odata` group intentionally.

1) GET `http://localhost:5177/odata/students?$select=lastName&$top=3`

```json
[
    {
        "LastName": "Alex"
    },
    {
        "LastName": "Eaine"
    },
    {
        "LastName": "Rorigo"
    }
]
```

2) POST `http://localhost:5177/odata/students`  with the following body:
Content-Type: application/json

```json
{

    "firstName": "Sokuda",
    "lastName": "Yu",
    "favoriteSport": "Soccer",
    "grade": 7,
    "schoolId": 3,
    "birthDay": "1977-11-04"
}
```