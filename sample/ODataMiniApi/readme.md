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

Check using `http://localhost:5177/schools/3`, you can see a new student added:

```json
[
  "schoolId": 3,
    "schoolName": "Earth University",
    "mailAddress": {
        "apartNum": 101,
        "city": "Belly",
        "street": "24TH ST",
        "zipCode": "98029"
    },
    "students": [
        ...
        {
            "studentId": 98,
            "firstName": "Sokuda",
            "lastName": "Yu",
            "favoriteSport": "Soccer",
            "grade": 7,
            "schoolId": 3,
            "birthDay": "1977-11-04"
        }
    ]
}
```

3) Patch `http://localhost:5177/odata/students/10`
Content-Type: application/json

```json
{

    "firstName": "Sokuda",
    "lastName": "Yu",
    "schoolId": 4
}
```

This will change the student, and also move the student from `Schools(1)` to `Schools(4)`

4) Delete `http://localhost:5177/odata/students/10`

This will delete the `Students(10)`


## OData CSDL metadata

I built one metadata endpoint to return the CSDL representation of 'customized' OData.

I use '$odata' to return the metadata.

Try: GET http://localhost:5177/customized/$odata, You can get CSDL XML representation:

```xml
<?xml version="1.0" encoding="utf-16"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
    <edmx:DataServices>
        <Schema Namespace="ODataMiniApi" xmlns="http://docs.oasis-open.org/odata/ns/edm">
            <EntityType Name="School">
                <Key>
                    <PropertyRef Name="SchoolId" />
                </Key>
                <Property Name="SchoolId" Type="Edm.Int32" Nullable="false" />
                <Property Name="SchoolName" Type="Edm.String" />
                <Property Name="MailAddress" Type="ODataMiniApi.Address" />
                <Property Name="Students" Type="Collection(ODataMiniApi.Student)" />
            </EntityType>
            <ComplexType Name="Address">
                <Property Name="ApartNum" Type="Edm.Int32" Nullable="false" />
                <Property Name="City" Type="Edm.String" />
                <Property Name="Street" Type="Edm.String" />
                <Property Name="ZipCode" Type="Edm.String" />
            </ComplexType>
            <ComplexType Name="Student">
                <Property Name="StudentId" Type="Edm.Int32" Nullable="false" />
                <Property Name="FirstName" Type="Edm.String" />
                <Property Name="LastName" Type="Edm.String" />
                <Property Name="FavoriteSport" Type="Edm.String" />
                <Property Name="Grade" Type="Edm.Int32" Nullable="false" />
                <Property Name="SchoolId" Type="Edm.Int32" Nullable="false" />
                <Property Name="BirthDay" Type="Edm.Date" Nullable="false" />
            </ComplexType>
        </Schema>
        <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
            <EntityContainer Name="Container">
                <EntitySet Name="Schools" EntityType="ODataMiniApi.School" />
            </EntityContainer>
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>
```

You can use 'Accept' request header or '$format' or 'format' query option to specify JSON or XML format, by default it's XML format.

Try: GET http://localhost:5177/customized/$odata, You can get CSDL XML representation:
Request Header:
Accept: application/json

You can get:

```json
{
    "$Version": "4.0",
    "$EntityContainer": "Default.Container",
    "ODataMiniApi": {
        "School": {
            "$Kind": "EntityType",
            "$Key": [
                "SchoolId"
            ],
            "SchoolId": {
                "$Type": "Edm.Int32"
            },
            "SchoolName": {
                "$Nullable": true
            },
            "MailAddress": {
                "$Type": "ODataMiniApi.Address",
                "$Nullable": true
            },
            "Students": {
                "$Collection": true,
                "$Type": "ODataMiniApi.Student",
                "$Nullable": true
            }
        },
        "Address": {
            "$Kind": "ComplexType",
            "ApartNum": {
                "$Type": "Edm.Int32"
            },
            "City": {
                "$Nullable": true
            },
            "Street": {
                "$Nullable": true
            },
            "ZipCode": {
                "$Nullable": true
            }
        },
        "Student": {
            "$Kind": "ComplexType",
            "StudentId": {
                "$Type": "Edm.Int32"
            },
            "FirstName": {
                "$Nullable": true
            },
            "LastName": {
                "$Nullable": true
            },
            "FavoriteSport": {
                "$Nullable": true
            },
            "Grade": {
                "$Type": "Edm.Int32"
            },
            "SchoolId": {
                "$Type": "Edm.Int32"
            },
            "BirthDay": {
                "$Type": "Edm.Date"
            }
        }
    },
    "Default": {
        "Container": {
            "$Kind": "EntityContainer",
            "Schools": {
                "$Collection": true,
                "$Type": "ODataMiniApi.School"
            }
        }
    }
}
```
