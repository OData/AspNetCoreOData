# ODataAlternateKeySample
-------------------------

This sample illustrates how to use the Alternate key in ASP.NET Core OData 8.x.

Alternate key is an 'alternate' key compared to the declared key.

For example, any customer can have a `Id` as his declared key, meanwhile, he can also have `SSN` as his identity.

The sample implements three alternate key scenarios:

1. Single alternate key
   - Using declared keys : ~/odata/Customers(3)
   - Using alternate keys: ~/odata/Customer(SSN='SSN-3-103')

2. Multiple alternate keys
   - Using declared keys : ~/odata/Orders(2)
   - Using alternate keys: ~/odata/Orders(Name='Order-2')
   - Using alternate keys: ~/odata/Orders(Token=75036B94-C836-4946-8CC8-054CF54060EC)

3. Composition alternate keys
   - Using declared keys : ~/odata/People(2)
   - Using alternate keys: ~/odata/People(CountryOrRegion='USA',Passport='9999')

You may noticed that you should use the `alternateKeyAlias=alternateKeyValue` pattern to invoke the API.

## Verify the Model

OData uses the vocabulary annotation to specify the alternate key.

Send `GET http://localhost:5219/odata/$metadata`

You can get the following metadata.

1) <strong>Customer</strong> type has `OData.Community.Keys.V1.AlternateKeys` annotation with one alternate key
2) <strong>Order</strong> type has `OData.Community.Keys.V1.AlternateKeys` annotation with two alternate keys
3) <strong>Person</strong> type has `OData.Community.Keys.V1.AlternateKeys` annotation with one alternate key, in which has two records

```xml
<?xml version="1.0" encoding="utf-8"?>
<edmx:Edmx Version="4.0" xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx">
    <edmx:DataServices>
        <Schema Namespace="ODataAlternateKeySample.Models" xmlns="http://docs.oasis-open.org/odata/ns/edm">
            <EntityType Name="Customer">
                <Key>
                    <PropertyRef Name="Id" />
                </Key>
                <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                <Property Name="Name" Type="Edm.String" />
                <Property Name="SSN" Type="Edm.String" />
                <Annotation Term="OData.Community.Keys.V1.AlternateKeys">
                    <Collection>
                        <Record Type="OData.Community.Keys.V1.AlternateKey">
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record Type="OData.Community.Keys.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="SSN" />
                                        <PropertyValue Property="Name" PropertyPath="SSN" />
                                    </Record>
                                </Collection>
                            </PropertyValue>
                        </Record>
                    </Collection>
                </Annotation>
            </EntityType>
            <EntityType Name="Order">
                <Key>
                    <PropertyRef Name="Id" />
                </Key>
                <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                <Property Name="Name" Type="Edm.String" />
                <Property Name="Token" Type="Edm.Guid" Nullable="false" />
                <Property Name="Price" Type="Edm.Decimal" Nullable="false" />
                <Property Name="Amount" Type="Edm.Int32" Nullable="false" />
                <Annotation Term="OData.Community.Keys.V1.AlternateKeys">
                    <Collection>
                        <Record Type="OData.Community.Keys.V1.AlternateKey">
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record Type="OData.Community.Keys.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="Name" />
                                        <PropertyValue Property="Name" PropertyPath="Name" />
                                    </Record>
                                </Collection>
                            </PropertyValue>
                        </Record>
                        <Record Type="OData.Community.Keys.V1.AlternateKey">
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record Type="OData.Community.Keys.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="Token" />
                                        <PropertyValue Property="Name" PropertyPath="Token" />
                                    </Record>
                                </Collection>
                            </PropertyValue>
                        </Record>
                    </Collection>
                </Annotation>
            </EntityType>
            <EntityType Name="Person">
                <Key>
                    <PropertyRef Name="Id" />
                </Key>
                <Property Name="Id" Type="Edm.Int32" Nullable="false" />
                <Property Name="Name" Type="Edm.String" />
                <Property Name="CountryOrRegion" Type="Edm.String" />
                <Property Name="Passport" Type="Edm.String" />
                <Annotation Term="OData.Community.Keys.V1.AlternateKeys">
                    <Collection>
                        <Record Type="OData.Community.Keys.V1.AlternateKey">
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record Type="OData.Community.Keys.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="c_or_r" />
                                        <PropertyValue Property="Name" PropertyPath="CountryOrRegion" />
                                    </Record>
                                    <Record Type="OData.Community.Keys.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="passport" />
                                        <PropertyValue Property="Name" PropertyPath="Passport" />
                                    </Record>
                                </Collection>
                            </PropertyValue>
                        </Record>
                    </Collection>
                </Annotation>
            </EntityType>
        </Schema>
        <Schema Namespace="Default" xmlns="http://docs.oasis-open.org/odata/ns/edm">
            <EntityContainer Name="Container">
                <EntitySet Name="Customers" EntityType="ODataAlternateKeySample.Models.Customer" />
                <EntitySet Name="Orders" EntityType="ODataAlternateKeySample.Models.Order" />
                <EntitySet Name="People" EntityType="ODataAlternateKeySample.Models.Person" />
            </EntityContainer>
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>
```

## Query using single alternate key

Send one of the following requests

```C#
GET http://localhost:5219/odata/Customers(2)
GET http://localhost:5219/odata/Customers(ssn='SSN-2-102')
```

you can get the following result:
```json
{
    "@odata.context": "http://localhost:5219/odata/$metadata#Customers/$entity",
    "Id": 2,
    "Name": "Jerry",
    "SSN": "SSN-2-102"
}
```

## Query using multiple alternate keys

Send one of the following requests

```C#
GET http://localhost:5219/odata/orders(3)
GET http://localhost:5219/odata/orders(Name='Order-3')
GET http://localhost:5219/odata/orders(Token=75036b94-c836-4946-8cc8-054cf54060ec)
```

you can get the following result:
```json
{
    "@odata.context": "http://localhost:5219/odata/$metadata#Orders/$entity",
    "Id": 3,
    "Name": "Order-3",
    "Token": "75036b94-c836-4946-8cc8-054cf54060ec",
    "Price": 24,
    "Amount": 37
}
```

## Query using composited alternate keys

Send one of the following requests

```C#
GET http://localhost:5219/odata/People(3)
GET ttp://localhost:5219/odata/People(c_or_r='USA',passport='9999')
```

you can get the following result:
```json
{
    "@odata.context": "http://localhost:5219/odata/$metadata#People/$entity",
    "Id": 3,
    "Name": "Mike",
    "CountryOrRegion": "USA",
    "Passport": "9999"
}
```
