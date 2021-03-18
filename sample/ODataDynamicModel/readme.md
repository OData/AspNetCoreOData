# ODataDynamicModel
-----------------------

This sample shows how to dynamically create an EDM model, and bind it into Web API OData pipeline.

---

## ASP.NET Core

It's an ASP.NET Core Web Application depending on `Microsoft.AspNetCore.OData` nuget package.

When it runs, you can use any client tool (for example `POSTMAN`) to file request:

### Query Metdata

```C#
GET http://localhost:5000/odata/mydatasource/$metadata
```
You will get:

```xml
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
    <edmx:DataServices>
        <Schema Namespace="ns" xmlns="http://docs.oasis-open.org/odata/ns/edm">
            <EntityType Name="Product">
                <Key>
                    <PropertyRef Name="ID" />
                </Key>
                <Property Name="Name" Type="Edm.String" />
                <Property Name="ID" Type="Edm.Int32" />
                <NavigationProperty Name="DetailInfo" Type="ns.DetailInfo" Nullable="false" />
            </EntityType>
            <EntityType Name="DetailInfo">
                <Key>
                    <PropertyRef Name="ID" />
                </Key>
                <Property Name="ID" Type="Edm.Int32" />
                <Property Name="Title" Type="Edm.String" />
            </EntityType>
            <EntityContainer Name="container">
                <EntitySet Name="Products" EntityType="ns.Product">
                    <NavigationPropertyBinding Path="DetailInfo" Target="DetailInfos" />
                </EntitySet>
                <EntitySet Name="DetailInfos" EntityType="ns.Product" />
            </EntityContainer>
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>
```

Or
```C#
GET http://localhost:5000/odata/anotherdatasource/$metadata
```

you will get:
```xml
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
    <edmx:DataServices>
        <Schema Namespace="ns" xmlns="http://docs.oasis-open.org/odata/ns/edm">
            <EntityType Name="Student">
                <Key>
                    <PropertyRef Name="ID" />
                </Key>
                <Property Name="Name" Type="Edm.String" />
                <Property Name="ID" Type="Edm.Int32" />
                <NavigationProperty Name="School" Type="ns.School" Nullable="false" />
            </EntityType>
            <EntityType Name="School">
                <Key>
                    <PropertyRef Name="ID" />
                </Key>
                <Property Name="ID" Type="Edm.Int32" />
                <Property Name="CreatedDay" Type="Edm.DateTimeOffset" />
            </EntityType>
            <EntityContainer Name="container">
                <EntitySet Name="Students" EntityType="ns.Student">
                    <NavigationPropertyBinding Path="School" Target="Schools" />
                </EntitySet>
                <EntitySet Name="Schools" EntityType="ns.Student" />
            </EntityContainer>
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>
```

### Query Entities

```C#
GET http://localhost:5000/odata/mydatasource/Products
```
You will get:
```json
{
    "@odata.context": "http://localhost:5000/odata/mydatasource/$metadata#Products",
    "value": [
        {
            "Name": "abc",
            "ID": 1
        },
        {
            "Name": "def",
            "ID": 2
        }
    ]
}
```
Or,

```C#
GET http://localhost:5000/odata/anotherdatasource/Schools
```
You will get:
```json
{
    "@odata.context": "http://localhost:5000/odata/anotherdatasource/$metadata#Schools",
    "value": [
        {
            "Name": "Foo",
            "ID": 100
        },
        {
            "Name": "Bar",
            "ID": 101
        }
    ]
}
```

### Query Property

For example the structural property:

```C#
GET http://localhost:5000/odata/mydatasource/Products(1)/Name
```

You will get:
```json
{
    "@odata.context": "http://localhost:5000/odata/mydatasource/$metadata#Products(1)/Name",
    "value": "abc"
}
```

Or navigation property:

```C#
GET http://localhost:5000/odata/anotherdatasource/Students(1)/School
```
You will get:

```json
{
    "@odata.context": "http://localhost:5000/odata/anotherdatasource/$metadata#Schools/ns.School/$entity",
    "ID": 99,
    "CreatedDay": "2016-01-19T01:02:03Z"
}
```
