# ODataAlternateKeySample
-------------------------

This sample illustrates how to use the Alternate key in ASP.NET Core OData 8.x.

Alternate key is an 'alternate' key compared to the declared key.

You can use `Org.OData.Core.V1.AlternateKey` term to define the alternate key vocabulary annotation, that's the recommended way.

For backward compatible, it also supports `OData.Community.Keys.V1.AlternateKeys` alternate key term.

For example, any customer can have a `Id` as his declared key, meanwhile, he can also have `SSN` as his identity.

The sample implements three alternate key scenarios:

1. Single alternate key
   - Using declared keys : ~/odata/Customers(3)
   - Using community alternate keys: ~/odata/Customers(SSN='SSN-3-103')
   - Using core alternate keys: ~/odata/Customers(CoreSN='SSN-3-103')

2. Multiple alternate keys
   - Using declared keys : ~/odata/Orders(2)
   - Using community alternate keys: ~/odata/Orders(Name='Order-2')
   - Using community alternate keys: ~/odata/Orders(Token=75036B94-C836-4946-8CC8-054CF54060EC)
   - Using core alternate keys: ~/odata/Orders(CoreName='Order-2')
   - Using core alternate keys: ~/odata/Orders(CoreToken=75036B94-C836-4946-8CC8-054CF54060EC)

3. Composition alternate keys
   - Using declared keys : ~/odata/People(2)
   - Using community alternate keys: ~/odata/People(c_or_r='USA',passport='9999')
   - Using core keys: ~/odata/People(core_c_r='USA',core_passport='9999')

You may notice that we should use the `alternateKeyAlias=alternateKeyValue` pattern to invoke the API. It's only supported using attribute routing.

## Verify the Model

OData uses the vocabulary annotation to specify the alternate key.

Send `GET http://localhost:5219/odata/$metadata`

You can get the following metadata.

1) <strong>Customer</strong> type has `OData.Community.Keys.V1.AlternateKeys` and `Org.OData.Core.V1.AlternateKey` annotation with one alternate key
2) <strong>Order</strong> type has `OData.Community.Keys.V1.AlternateKeys` and `Org.OData.Core.V1.AlternateKey` annotation with two alternate keys
3) <strong>Person</strong> type has `OData.Community.Keys.V1.AlternateKeys` and `Org.OData.Core.V1.AlternateKey` annotation with one composite alternate key

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
                <Property Name="Titles" Type="Collection(Edm.String)" />
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
                <Annotation Term="Org.OData.Core.V1.AlternateKeys">
                    <Collection>
                        <Record Type="Org.OData.Core.V1.AlternateKey">
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record Type="Org.OData.Core.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="CoreSN" />
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
                <Property Name="Price" Type="Edm.Decimal" Nullable="false" Scale="Variable" />
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
                <Annotation Term="Org.OData.Core.V1.AlternateKeys">
                    <Collection>
                        <Record>
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record>
                                        <PropertyValue Property="Alias" String="CoreName" />
                                        <PropertyValue Property="Name" PropertyPath="Name" />
                                    </Record>
                                </Collection>
                            </PropertyValue>
                        </Record>
                        <Record>
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record>
                                        <PropertyValue Property="Alias" String="CoreToken" />
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
                <Annotation Term="Org.OData.Core.V1.AlternateKeys">
                    <Collection>
                        <Record Type="Org.OData.Core.V1.AlternateKey">
                            <PropertyValue Property="Key">
                                <Collection>
                                    <Record Type="Org.OData.Core.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="core_c_r" />
                                        <PropertyValue Property="Name" PropertyPath="CountryOrRegion" />
                                    </Record>
                                    <Record Type="Org.OData.Core.V1.PropertyRef">
                                        <PropertyValue Property="Alias" String="core_passport" />
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
GET http://localhost:5219/odata/Customers(SSN='SSN-%25-2-102')
GET http://localhost:5219/odata/Customers(CoreSN='SSN-%25-2-102')
```

you can get the following result:
```json
{
    "@odata.context": "http://localhost:5219/odata/$metadata#Customers/$entity",
    "Id": 2,
    "Name": "Jerry",
    "SSN": "SSN-%25-2-102",
    "Titles": [
        "abc",
        null,
        "efg"
    ]
}
```

## Query using multiple alternate keys

Send one of the following requests

```C#
GET http://localhost:5219/odata/orders(3)
GET http://localhost:5219/odata/orders(Name='Order-3')
GET http://localhost:5219/odata/orders(Token=75036b94-c836-4946-8cc8-054cf54060ec)
GET http://localhost:5219/odata/Orders(CoreName='Order-3')
GET http://localhost:5219/odata/Orders(CoreToken=75036B94-C836-4946-8CC8-054CF54060EC)
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
GET http://localhost:5219/odata/People(c_or_r='USA',passport='9999')
GET http://localhost:5219/odata/People(core_c_r='USA',core_passport='9999')
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

Thanks!