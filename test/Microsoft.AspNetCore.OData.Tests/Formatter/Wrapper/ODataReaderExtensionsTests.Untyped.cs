//-----------------------------------------------------------------------------
// <copyright file="ODataReaderExtensionsTests.Untyped.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Wrapper;

/// <summary>
/// Here contains reading test cases for 'Edm.Untyped' (Declared or undeclared)
/// </summary>
public partial class ODataReaderExtensionsTests
{
    private static readonly EdmModel UntypedModel = BuildUntypedModel();

    private static EdmModel BuildUntypedModel()
    {
        EdmModel model = new EdmModel();

        // Open Complex Type (Info)
        EdmComplexType info = new EdmComplexType("NS", "Info", null, false, isOpen: true);
        model.AddElement(info);

        // Open Entity Type (Person)
        /*
         <EntityType Name="Person" IsOpen="True" >
            <Property Name="ID" Type="Edm.Int32" />
            <Property Name="Name" Type="Edm.String" />
            <Property Name="Data" Type="Edm.Untyped" />
            <Property Name="Infos" Type="Collection(NS.Info)" />
         </EntityType>
         */
        EdmEntityType person = new EdmEntityType("NS", "Person", null, false, isOpen: true);
        model.AddElement(person);
        person.AddKeys(person.AddStructuralProperty("ID", EdmCoreModel.Instance.GetInt32(false)));
        person.AddStructuralProperty("Name", EdmCoreModel.Instance.GetString(true));
        person.AddStructuralProperty("Data", EdmCoreModel.Instance.GetUntyped(true));
        person.AddStructuralProperty("Infos", new EdmCollectionTypeReference
            (new EdmCollectionType(new EdmComplexTypeReference(info, false))));

        EdmEntityContainer defaultContainer = new EdmEntityContainer("NS", "Container");
        model.AddElement(defaultContainer);
        EdmEntitySet people = defaultContainer.AddEntitySet("People", person);
        return model;
    }

    public static TheoryDataSet<string, object> PrimitiveValueCases
    {
        get
        {
            var data = new TheoryDataSet<string, object>
            {
                { "42", 42 },
                { "9.19", (decimal)9.19 },
                { "3.1415926535897931", 3.1415926535897931m },
                { "true", true },
                { "false", false },
                { "\"false\"", "false" },
                { "\"a simple string\"", "a simple string" },

                 // looks like a Guid string, but it's string
                { "\"969D9BB3-1A1D-40C7-8716-4E29CFE02E81\"", "969D9BB3-1A1D-40C7-8716-4E29CFE02E81" },
                { "\"Red\"", "Red" }, // looks like a Enum string, but it's string
                { "null", null }
            };

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(PrimitiveValueCases))]
    public async Task ReadOpenResource_WithUntypedPrimitiveValue_WorksAsExpected(string value, object expect)
    {
        // Arrange
        string payload =
        "{" +
            "\"ID\": 17," +
            $"\"Data\": {value}," +
            $"\"Dynamic\": {value}" +
        "}";

        IEdmEntitySet people = UntypedModel.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people); // Guard

        // Act
        Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(people, people.EntityType);
        ODataItemWrapper item = await ReadUntypedPayloadAsync(payload, UntypedModel, func);

        // Assert
        Assert.NotNull(item);
        ODataResourceWrapper resourceWrapper = Assert.IsType<ODataResourceWrapper>(item);
        Assert.Empty(resourceWrapper.NestedResourceInfos);

        Assert.Collection(resourceWrapper.Resource.Properties.OfType<ODataProperty>(),
            p =>
            {
                // Declared property with 'Edm.Int32'
                Assert.Equal("ID", p.Name);
                Assert.Equal(17, p.Value);
            },
            p =>
            {
                // Declared property with 'Edm.Untyped'
                Assert.Equal("Data", p.Name);
                Assert.Equal(expect, p.Value);
            },
            p =>
            {
                // Un-declared (dynamic) property
                Assert.Equal("Dynamic", p.Name);
                Assert.Equal(expect, p.Value);
            });
    }

    [Fact]
    public async Task ReadOpenResource_WithUntypedResourceValue_WorksAsExpected()
    {
        // Arrange
        string resourceValue = "{\"Key1\": \"Value1\", \"Key2\": true}";
        string payload =
        "{" +
            "\"ID\": 17," +
            $"\"Data\": {resourceValue}," +
            $"\"Dynamic\": {resourceValue}" +
        "}";

        IEdmEntitySet people = UntypedModel.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people); // Guard

        // Act
        Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(people, people.EntityType);
        ODataItemWrapper item = await ReadUntypedPayloadAsync(payload, UntypedModel, func);

        // Assert
        Assert.NotNull(item);
        ODataResourceWrapper resourceWrapper = Assert.IsType<ODataResourceWrapper>(item);

        ODataProperty property = Assert.IsType<ODataProperty>(Assert.Single(resourceWrapper.Resource.Properties));
        // Declared property with 'Edm.Int32'
        Assert.Equal("ID", property.Name);
        Assert.Equal(17, property.Value);

        Assert.Equal(2, resourceWrapper.NestedResourceInfos.Count);

        Action<ODataNestedResourceInfoWrapper, string> verifyAction = (p, n) =>
        {
            Assert.Equal(n, p.NestedResourceInfo.Name);
            ODataResourceWrapper nestedResourceWrapper = Assert.IsType<ODataResourceWrapper>(Assert.Single(p.NestedItems));
            Assert.Empty(nestedResourceWrapper.NestedResourceInfos);
            Assert.Collection(nestedResourceWrapper.Resource.Properties.OfType<ODataProperty>(),
                p =>
                {
                    Assert.Equal("Key1", p.Name);
                    Assert.Equal("Value1", p.Value);
                },
                p =>
                {
                    Assert.Equal("Key2", p.Name);
                    Assert.True((bool)p.Value);
                });
        };

        Assert.Collection(resourceWrapper.NestedResourceInfos,
            p =>
            {
                // Declared property with 'Edm.Untyped'
                verifyAction(p, "Data");
            },
            p =>
            {
                // Un-declared (dynamic) property
                verifyAction(p, "Dynamic");
            });
    }

    [Fact]
    public async Task ReadOpenResource_WithUntypedCollectionValue_WorksAsExpected()
    {
        // Arrange
        string collectionValue = "[" +
            "null," +
            "{\"Key1\": \"Value1\", \"Key2\": true}," +
            "\"A string item\"," +
            "42" +
        "]";

        string payload =
        "{" +
            "\"ID\": 17," +
            $"\"Data\": {collectionValue}," +
            $"\"Dynamic\": {collectionValue}" +
        "}";

        IEdmEntitySet people = UntypedModel.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people); // Guard

        // Act
        Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(people, people.EntityType);
        ODataItemWrapper item = await ReadUntypedPayloadAsync(payload, UntypedModel, func);

        // Assert
        Assert.NotNull(item);
        ODataResourceWrapper resourceWrapper = Assert.IsType<ODataResourceWrapper>(item);

        ODataProperty property = Assert.IsType<ODataProperty>(Assert.Single(resourceWrapper.Resource.Properties));
        // Declared property with 'Edm.Int32'
        Assert.Equal("ID", property.Name);
        Assert.Equal(17, property.Value);

        Assert.Equal(2, resourceWrapper.NestedResourceInfos.Count);

        Action<ODataNestedResourceInfoWrapper, string> verifyAction = (p, n) =>
        {
            Assert.Equal(n, p.NestedResourceInfo.Name);
            ODataResourceSetWrapper nestedResourceSetWrapper = Assert.IsType<ODataResourceSetWrapper>(Assert.Single(p.NestedItems));
            Assert.Equal(2, nestedResourceSetWrapper.Resources.Count);

            Assert.Collection(nestedResourceSetWrapper.Resources,
                p =>
                {
                    Assert.Null(p); // since it's a 'null' value in collection,
                                    // ODL reads it as ResourceStart, ResourceEnd with null item.
                },
                p =>
                {
                    Assert.Empty(p.NestedResourceInfos);
                    Assert.NotNull(p.Resource);
                    Assert.Collection(p.Resource.Properties.OfType<ODataProperty>(),
                        p =>
                        {
                            Assert.Equal("Key1", p.Name);
                            Assert.Equal("Value1", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("Key2", p.Name);
                            Assert.True((bool)p.Value);
                        });
                });

            Assert.Equal(4, nestedResourceSetWrapper.Items.Count);
            Assert.Collection(nestedResourceSetWrapper.Items,
                p =>
                {
                    Assert.Null(p); // since it's a 'null' value in collection,
                                    // ODL reads it as ResourceStart, ResourceEnd with null item.
                },
                p =>
                {
                    ODataResourceWrapper resourceWrapper = Assert.IsType<ODataResourceWrapper>(p);
                    Assert.Empty(resourceWrapper.NestedResourceInfos);
                    Assert.NotNull(resourceWrapper.Resource);
                    Assert.Collection(resourceWrapper.Resource.Properties.OfType<ODataProperty>(),
                        p =>
                        {
                            Assert.Equal("Key1", p.Name);
                            Assert.Equal("Value1", p.Value);
                        },
                        p =>
                        {
                            Assert.Equal("Key2", p.Name);
                            Assert.True((bool)p.Value);
                        });
                },
                p =>
                {
                    ODataPrimitiveWrapper primitiveWrapper = Assert.IsType<ODataPrimitiveWrapper>(p);
                    Assert.NotNull(primitiveWrapper.Value);
                    Assert.Equal("A string item", primitiveWrapper.Value.Value);
                },
                p =>
                {
                    ODataPrimitiveWrapper primitiveWrapper = Assert.IsType<ODataPrimitiveWrapper>(p);
                    Assert.NotNull(primitiveWrapper.Value);
                    Assert.Equal(42, primitiveWrapper.Value.Value);
                });
        };

        Assert.Collection(resourceWrapper.NestedResourceInfos,
            p =>
            {
                // Declared property with 'Edm.Untyped'
                verifyAction(p, "Data");
            },
            p =>
            {
                // Un-declared (dynamic) property
                verifyAction(p, "Dynamic");
            });
    }

    [Fact]
    public async Task ReadOpenResource_WithUntypedCollectionValue_OnNestedResourceInfo_WorksAsExpected()
    {
        // Arrange
        string collectionValue = "[" +
            "{}," +
            "{\"Key1\": \"Value1\"}," +
            "{\"Key2\": {}}," +
            "{\"Key3\": {\"@odata.type\":\"NS.Info\"}}," +
            "{\"Key4\": []}" +
        "]";

        string payload =
        "{" +
            $"\"Infos\": {collectionValue}" +
        "}";

        IEdmEntitySet people = UntypedModel.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people); // Guard

        // Act
        Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(people, people.EntityType);
        ODataItemWrapper item = await ReadUntypedPayloadAsync(payload, UntypedModel, func);

        // Assert
        Assert.NotNull(item);
        ODataResourceWrapper resourceWrapper = Assert.IsType<ODataResourceWrapper>(item);

        Assert.Empty(resourceWrapper.Resource.Properties);

        ODataNestedResourceInfoWrapper nestedResourceInfoWrapper = Assert.Single(resourceWrapper.NestedResourceInfos);
        Assert.Equal("Infos", nestedResourceInfoWrapper.NestedResourceInfo.Name);

        ODataResourceSetWrapper nestedResourceSetWrapper = Assert.IsType<ODataResourceSetWrapper>
            (Assert.Single(nestedResourceInfoWrapper.NestedItems));

        Assert.Collection(nestedResourceSetWrapper.Resources,
            p =>
            {
                Assert.Equal("NS.Info", p.Resource.TypeName);

                Assert.Empty(p.Resource.Properties);
                Assert.Empty(p.NestedResourceInfos);
            },
            p =>
            {
                Assert.Equal("NS.Info", p.Resource.TypeName);

                Assert.Empty(p.NestedResourceInfos);
                ODataProperty property = Assert.IsType<ODataProperty>(Assert.Single(p.Resource.Properties));
                Assert.Equal("Key1", property.Name);
                Assert.Equal("Value1", property.Value);
            },
            p =>
            {
                Assert.Equal("NS.Info", p.Resource.TypeName);
                Assert.Empty(p.Resource.Properties);

                ODataNestedResourceInfoWrapper nestedInfoWrapper = Assert.Single(p.NestedResourceInfos);
                Assert.Equal("Key2", nestedInfoWrapper.NestedResourceInfo.Name);

                ODataResourceWrapper nestedResourceWrapper = Assert.IsType<ODataResourceWrapper>(Assert.Single(nestedInfoWrapper.NestedItems));
                Assert.Equal("Edm.Untyped", nestedResourceWrapper.Resource.TypeName);
                Assert.Empty(nestedResourceWrapper.Resource.Properties);
                Assert.Empty(nestedResourceWrapper.NestedResourceInfos);
            },
            p =>
            {
                Assert.Equal("NS.Info", p.Resource.TypeName);
                Assert.Empty(p.Resource.Properties);

                ODataNestedResourceInfoWrapper nestedInfoWrapper = Assert.Single(p.NestedResourceInfos);
                Assert.Equal("Key3", nestedInfoWrapper.NestedResourceInfo.Name);

                ODataResourceWrapper nestedResourceWrapper = Assert.IsType<ODataResourceWrapper>(Assert.Single(nestedInfoWrapper.NestedItems));
                Assert.Equal("NS.Info", nestedResourceWrapper.Resource.TypeName);
                Assert.Empty(nestedResourceWrapper.Resource.Properties);
                Assert.Empty(nestedResourceWrapper.NestedResourceInfos);
            },
            p =>
            {
                Assert.Equal("NS.Info", p.Resource.TypeName);
                Assert.Empty(p.Resource.Properties);

                ODataNestedResourceInfoWrapper nestedInfoWrapper = Assert.Single(p.NestedResourceInfos);
                Assert.Equal("Key4", nestedInfoWrapper.NestedResourceInfo.Name);

                ODataResourceSetWrapper nestedResourceSetWrapper = Assert.IsType<ODataResourceSetWrapper>(Assert.Single(nestedInfoWrapper.NestedItems));
                Assert.Empty(nestedResourceSetWrapper.Resources);
            });
    }

    [Fact]
    public async Task ReadOpenResource_WithDeepUntypedCollectionValue_WorksAsExpected()
    {
        // Arrange
        const int Depth = 10;
        string collectionValue = "[42]";
        for (int i = 0; i < Depth; i++)
        {
            collectionValue = $"[42, {collectionValue}]";
        }
        // [42, [42, [42, [42, [42, [42, [42, [42, [42, [42, [42]]]]]]]]]]]

        string payload =
        "{" +
            $"\"Data\": {collectionValue}," +
            $"\"Dynamic\": {collectionValue}" +
        "}";

        IEdmEntitySet people = UntypedModel.EntityContainer.FindEntitySet("People");
        Assert.NotNull(people); // Guard

        // Act
        Func<ODataMessageReader, Task<ODataReader>> func = mr => mr.CreateODataResourceReaderAsync(people, people.EntityType);
        ODataItemWrapper item = await ReadUntypedPayloadAsync(payload, UntypedModel, func);

        // Assert
        Assert.NotNull(item);
        ODataResourceWrapper resourceWrapper = Assert.IsType<ODataResourceWrapper>(item);

        Assert.Empty(resourceWrapper.Resource.Properties);
        Assert.Equal(2, resourceWrapper.NestedResourceInfos.Count);

        Action<ODataNestedResourceInfoWrapper, string> verifyAction = (p, n) =>
        {
            Assert.Equal(n, p.NestedResourceInfo.Name);
            ODataResourceSetWrapper nestedResourceSetWrapper = Assert.IsType<ODataResourceSetWrapper>(Assert.Single(p.NestedItems));

            for (int i = 0; i <= Depth; ++i) // Why '<= Depth'? Because we have extra single primitive (42) in the deepest level.
            {
                Assert.Empty(nestedResourceSetWrapper.Resources);
                ODataPrimitiveWrapper primitiveWrapper;
                if (i == Depth)
                {
                    primitiveWrapper = Assert.IsType<ODataPrimitiveWrapper>(Assert.Single(nestedResourceSetWrapper.Items));
                }
                else
                {
                    Assert.Equal(2, nestedResourceSetWrapper.Items.Count);
                    primitiveWrapper = Assert.IsType<ODataPrimitiveWrapper>(nestedResourceSetWrapper.Items.First());
                    nestedResourceSetWrapper = Assert.IsType<ODataResourceSetWrapper>(nestedResourceSetWrapper.Items.Last());
                }

                Assert.NotNull(primitiveWrapper.Value);
                Assert.Equal(42, primitiveWrapper.Value.Value); // in the collection, ODL reads 42 as integer. It's different with 42 in untyped single value property
            }
        };

        Assert.Collection(resourceWrapper.NestedResourceInfos,
            p =>
            {
                // Declared property with 'Edm.Untyped'
                verifyAction(p, "Data");
            },
            p =>
            {
                // Un-declared (dynamic) property
                verifyAction(p, "Dynamic");
            });
    }

    private async Task<ODataItemWrapper> ReadUntypedPayloadAsync(string payload,
        IEdmModel edmModel, Func<ODataMessageReader, Task<ODataReader>> createReader,
        bool readUntypedAsString = false)
    {
        return await ReadPayloadAsync(payload, edmModel, createReader, ODataVersion.V4, readUntypedAsString);
    }
}
