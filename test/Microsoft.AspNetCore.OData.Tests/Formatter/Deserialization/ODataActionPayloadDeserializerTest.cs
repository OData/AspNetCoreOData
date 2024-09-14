//-----------------------------------------------------------------------------
// <copyright file="ODataActionPayloadDeserializerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;

public class ODataActionPayloadDeserializerTest
{
    private static IEdmModel _model;
    private static IEdmEntityContainer _container;
    private static IODataDeserializerProvider _deserializerProvider;
    private static ODataActionPayloadDeserializer _deserializer;
    private const string _serviceRoot = "http://any/";

    static ODataActionPayloadDeserializerTest()
    {
        _model = GetModel();
        _container = _model.EntityContainer;

        _deserializerProvider = DeserializationServiceProviderHelper.GetServiceProvider().GetRequiredService<IODataDeserializerProvider>();
        _deserializer = new ODataActionPayloadDeserializer(_deserializerProvider);
    }

    [Fact]
    public void Ctor_ThrowsArgumentNull_DeserializerProvider()
    {
        ExceptionAssert.ThrowsArgumentNull(
            () => new ODataActionPayloadDeserializer(deserializerProvider: null),
            "deserializerProvider");
    }

    [Fact]
    public void Ctor_SetsProperty_DeserializerProvider()
    {
        // Arrange
        IODataDeserializerProvider deserializerProvider = new Mock<IODataDeserializerProvider>().Object;

        // Act
        var deserializer = new ODataActionPayloadDeserializer(deserializerProvider);

        // Assert
        Assert.Same(deserializerProvider, deserializer.DeserializerProvider);
    }

    [Fact]
    public void Ctor_SetsProperty_ODataPayloadKind()
    {
        // Arrange
        IODataDeserializerProvider deserializerProvider = new Mock<IODataDeserializerProvider>().Object;

        // Act
        var deserializer = new ODataActionPayloadDeserializer(deserializerProvider);

        // Assert
        Assert.Equal(ODataPayloadKind.Parameter, deserializer.ODataPayloadKind);
    }

    [Fact]
    public async Task ReadAsync_ThrowsArgumentNull_MessageReader()
    {
        // Arrange
        ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(_deserializerProvider);

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => deserializer.ReadAsync(messageReader: null, type: typeof(ODataActionParameters), readContext: new ODataDeserializerContext()),
            "messageReader");
    }

    [Fact]
    public async Task ReadAsync_ThrowsArgumentNull_ReadContext()
    {
        // Arrange
        ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(_deserializerProvider);
        ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

        // Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => deserializer.ReadAsync(messageReader, typeof(ODataActionParameters), readContext: null),
            "readContext");
    }

    [Fact]
    public async Task ReadAsync_Throws_SerializationException_ODataPathMissing()
    {
        // Arrange
        ODataActionPayloadDeserializer deserializer = new ODataActionPayloadDeserializer(_deserializerProvider);
        ODataMessageReader messageReader = ODataTestUtil.GetMockODataMessageReader();

        // Act & Assert
        await ExceptionAssert.ThrowsAsync<SerializationException>(
            () => deserializer.ReadAsync(messageReader, typeof(ODataActionParameters), readContext: new ODataDeserializerContext()),
            "The operation cannot be completed because no ODataPath is available for the request.");
    }

    public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithPrimitiveParametersTest
    {
        get
        {
            return new TheoryDataSet<string, IEdmAction, ODataPath>
            {
                {"Primitive", GetBoundAction("Primitive"), CreateBoundPath("Primitive") },
                {"UnboundPrimitive", GetUnboundAction("UnboundPrimitive"), CreateUnboundPath("UnboundPrimitive")}
            };
        }
    }

    public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithComplexParametersTest
    {
        get
        {
            return new TheoryDataSet<string, IEdmAction, ODataPath>
            {
                {"Complex", GetBoundAction("Complex"), CreateBoundPath("Complex") },
                {"UnboundComplex", GetUnboundAction("UnboundComplex"), CreateUnboundPath("UnboundComplex")}
            };
        }
    }
    public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithEnumParametersTest
    {
        get
        {
            return new TheoryDataSet<string, IEdmAction, ODataPath>
            {
                {"Enum", GetBoundAction("Enum"), CreateBoundPath("Enum") },
                {"UnboundEnum", GetUnboundAction("UnboundEnum"), CreateUnboundPath("UnboundEnum")}
            };
        }
    }

    public static TheoryDataSet<IEdmAction, ODataPath> DeserializeWithEntityParametersTest
    {
        get
        {
            return new TheoryDataSet<IEdmAction, ODataPath>
            {
                { GetBoundAction("Entity"), CreateBoundPath("Entity") },
                { GetUnboundAction("UnboundEntity"), CreateUnboundPath("UnboundEntity")}
            };
        }
    }

    public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithPrimitiveCollectionsTest
    {
        get
        {
            return new TheoryDataSet<string, IEdmAction, ODataPath>
            {
                {"PrimitiveCollection", GetBoundAction("PrimitiveCollection"), CreateBoundPath("PrimitiveCollection") },
                {"UnboundPrimitiveCollection", GetUnboundAction("UnboundPrimitiveCollection"), CreateUnboundPath("UnboundPrimitiveCollection")}
            };
        }
    }

    public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithComplexCollectionsTest
    {
        get
        {
            return new TheoryDataSet<string, IEdmAction, ODataPath>
            {
                {"ComplexCollection", GetBoundAction("ComplexCollection"), CreateBoundPath("ComplexCollection") },
                {"UnboundComplexCollection", GetUnboundAction("UnboundComplexCollection"), CreateUnboundPath("UnboundComplexCollection")}
            };
        }
    }
    public static TheoryDataSet<string, IEdmAction, ODataPath> DeserializeWithEnumCollectionsTest
    {
        get
        {
            return new TheoryDataSet<string, IEdmAction, ODataPath>
            {
                {"EnumCollection", GetBoundAction("EnumCollection"), CreateBoundPath("EnumCollection") },
                {"UnboundEnumCollection", GetUnboundAction("UnboundEnumCollection"), CreateUnboundPath("UnboundEnumCollection")}
            };
        }
    }

    public static TheoryDataSet<IEdmAction, ODataPath> DeserializeWithEntityCollectionsTest
    {
        get
        {
            return new TheoryDataSet<IEdmAction, ODataPath>
            {
                { GetBoundAction("EntityCollection"), CreateBoundPath("EntityCollection") },
                { GetUnboundAction("UnboundEntityCollection"), CreateUnboundPath("UnboundEntityCollection")}
            };
        }
    }

    [Theory]
    [MemberData(nameof(DeserializeWithPrimitiveParametersTest))]
    public async Task Can_DeserializePayload_WithPrimitiveParameters(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const int Quantity = 1;
        const string ProductCode = "PCode";
        string body = "{" +
            string.Format(@" ""Quantity"": {0} , ""ProductCode"": ""{1}"" , ""Birthday"": ""2015-02-27"", ""BkgColor"": ""Red"", ""InnerColor"": null", Quantity, ProductCode) +
            "}";

        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(body));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext() { Path = path, Model = _model };

        // Act
        ODataActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
        IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

        // Assert
        Assert.NotNull(actionName);
        Assert.Same(expectedAction, action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Quantity"));
        Assert.Equal(Quantity, payload["Quantity"]);
        Assert.True(payload.ContainsKey("ProductCode"));
        Assert.Equal(ProductCode, payload["ProductCode"]);

        Assert.True(payload.ContainsKey("Birthday"));
        Assert.Equal(new Date(2015, 2, 27), payload["Birthday"]);

        Assert.True(payload.ContainsKey("BkgColor"));
        AColor bkgColor = Assert.IsType<AColor>(payload["BkgColor"]);
        Assert.Equal(AColor.Red, bkgColor);

        Assert.True(payload.ContainsKey("InnerColor"));
        Assert.Null(payload["InnerColor"]);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithComplexParametersTest))]
    public async Task Can_DeserializePayload_WithComplexParameters(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Quantity"": 1 , ""Address"": { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } }";
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext() { Path = path, Model = _model };

        // Act
        ODataActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
        IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

        // Assert
        Assert.NotNull(actionName);
        Assert.Same(expectedAction, action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Quantity"));
        Assert.Equal(1, payload["Quantity"]);
        Assert.True(payload.ContainsKey("Address"));
        MyAddress address = payload["Address"] as MyAddress;
        Assert.NotNull(address);
        Assert.Equal("1 Microsoft Way", address.StreetAddress);
        Assert.Equal("Redmond", address.City);
        Assert.Equal("WA", address.State);
        Assert.Equal(98052, address.ZipCode);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithPrimitiveCollectionsTest))]
    public async Task Can_DeserializePayload_WithPrimitiveCollections_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body =
            @"{ ""Name"": ""Avatar"", ""Ratings"": [ 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 ], ""Time"": [""01:02:03.0040000"", ""12:13:14.1150000""]}";
        int[] expectedRatings = new int[] { 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 };
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");

        ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;
        IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

        //Assert
        Assert.NotNull(actionName);
        Assert.Same(expectedAction, action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Name"));
        Assert.Equal("Avatar", payload["Name"]);
        Assert.True(payload.ContainsKey("Ratings"));
        IEnumerable<int> ratings = payload["Ratings"] as IEnumerable<int>;
        Assert.Equal(10, ratings.Count());
        Assert.True(expectedRatings.Zip(ratings, (expected, actual) => expected - actual).All(diff => diff == 0));

        Assert.True(payload.ContainsKey("Time"));
        IEnumerable<TimeOfDay> times = payload["Time"] as IEnumerable<TimeOfDay>;
        Assert.Equal(2, times.Count());
        Assert.Equal(new[] { new TimeOfDay(1, 2, 3, 4), new TimeOfDay(12, 13, 14, 115) }, times.ToList());
    }

    [Theory]
    [MemberData(nameof(DeserializeWithComplexCollectionsTest))]
    public async Task Can_DeserializePayload_WithComplexCollections_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Name"": ""Avatar"" , ""Addresses"": [{ ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 }] }";
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");

        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

        // Assert
        Assert.NotNull(actionName);
        Assert.Same(expectedAction, payload.Action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Name"));
        Assert.Equal("Avatar", payload["Name"]);
        Assert.True(payload.ContainsKey("Addresses"));
        IEnumerable<IEdmObject> addresses = payload["Addresses"] as EdmComplexObjectCollection;
        dynamic address = addresses.SingleOrDefault();
        Assert.NotNull(address);
        Assert.Equal("1 Microsoft Way", address.StreetAddress);
        Assert.Equal("Redmond", address.City);
        Assert.Equal("WA", address.State);
        Assert.Equal(98052, address.ZipCode);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithEnumCollectionsTest))]
    public async Task Can_DeserializePayload_WithEnumCollections_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Colors"": [ ""Red"", ""Green""] }";
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");

        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

        // Assert
        Assert.NotNull(actionName);
        Assert.Same(expectedAction, payload.Action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Colors"));
        EdmEnumObjectCollection colors = payload["Colors"] as EdmEnumObjectCollection;
        EdmEnumObject color = colors[0] as EdmEnumObject;
        Assert.NotNull(color);
        Assert.Equal("Red", color.Value);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithComplexParametersTest))]
    public async Task Can_DeserializePayload_WithComplexParameters_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Quantity"": 1 , ""Address"": { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } }";
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");

        ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, _model);

        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

        // Assert
        Assert.NotNull(actionName);
        Assert.NotNull(payload);
        Assert.Same(expectedAction, payload.Action);
        Assert.True(payload.ContainsKey("Quantity"));
        Assert.Equal(1, payload["Quantity"]);
        Assert.True(payload.ContainsKey("Address"));
        dynamic address = payload["Address"] as EdmComplexObject;
        Assert.IsType<EdmComplexObject>(address);
        Assert.Equal("1 Microsoft Way", address.StreetAddress);
        Assert.Equal("Redmond", address.City);
        Assert.Equal("WA", address.State);
        Assert.Equal(98052, address.ZipCode);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithEnumParametersTest))]
    public async Task Can_DeserializePayload_WithEnumParameters_InUntypedMode(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Color"": ""Red""}";
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");

        ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, settings, _model);

        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

        // Assert
        Assert.NotNull(actionName);
        Assert.NotNull(payload);
        Assert.Same(expectedAction, payload.Action);
        Assert.True(payload.ContainsKey("Color"));
        EdmEnumObject color = payload["Color"] as EdmEnumObject;
        Assert.IsType<EdmEnumObject>(color);
        Assert.Equal("Red", color.Value);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithPrimitiveCollectionsTest))]
    public async Task Can_DeserializePayload_WithPrimitiveCollectionParameters(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body =
            @"{ ""Name"": ""Avatar"", ""Ratings"": [ 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 ], ""Time"": [""01:02:03.0040000"", ""12:13:14.1150000""], ""Colors"": [ ""Red"", null, ""Green""] }";
        int[] expectedRatings = new int[] { 5, 5, 3, 4, 5, 5, 4, 5, 5, 4 };

        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);

        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model };

        // Act
        ODataActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
        IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

        // Assert
        Assert.NotNull(actionName);
        Assert.NotNull(payload);
        Assert.Same(expectedAction, action);
        Assert.True(payload.ContainsKey("Name"));
        Assert.Equal("Avatar", payload["Name"]);
        Assert.True(payload.ContainsKey("Ratings"));
        IEnumerable<int> ratings = payload["Ratings"] as IEnumerable<int>;
        Assert.Equal(10, ratings.Count());
        Assert.True(expectedRatings.Zip(ratings, (expected, actual) => expected - actual).All(diff => diff == 0));

        Assert.True(payload.ContainsKey("Time"));
        IEnumerable<TimeOfDay> times = payload["Time"] as IEnumerable<TimeOfDay>;
        Assert.Equal(2, times.Count());
        Assert.Equal(new[] { new TimeOfDay(1, 2, 3, 4), new TimeOfDay(12, 13, 14, 115) }, times.ToList());

        Assert.True(payload.ContainsKey("Colors"));
        IEnumerable<AColor?> colors = payload["Colors"] as IEnumerable<AColor?>;
        Assert.Equal("Red|null|Green", String.Join("|", colors.Select(e => e == null ? "null" : e.ToString())));
    }

    [Theory]
    [MemberData(nameof(DeserializeWithComplexCollectionsTest))]
    public async Task Can_DeserializePayload_WithComplexCollectionParameters(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Name"": ""Microsoft"", ""Addresses"": [ { ""StreetAddress"":""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } ] }";
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(Body));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model };

        // Act
        ODataActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context) as ODataActionParameters;

        // Assert
        Assert.NotNull(actionName);
        Assert.NotNull(expectedAction);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Name"));
        Assert.Equal("Microsoft", payload["Name"]);
        Assert.True(payload.ContainsKey("Addresses"));
        IList<MyAddress> addresses = (payload["Addresses"] as IEnumerable<MyAddress>).ToList();
        Assert.NotNull(addresses);
        Assert.Single(addresses);
        MyAddress address = addresses[0];
        Assert.NotNull(address);
        Assert.Equal("1 Microsoft Way", address.StreetAddress);
        Assert.Equal("Redmond", address.City);
        Assert.Equal("WA", address.State);
        Assert.Equal(98052, address.ZipCode);
    }

    private const string EntityPayload =
"{" +
"\"Id\": 1, " +
"\"Customer\": {\"@odata.type\":\"#A.B.Customer\", \"Id\":109,\"Name\":\"Avatar\" } " +
// null can't work here, see: https://github.com/OData/odata.net/issues/99
// ",\"NullableCustomer\" : null " +  //
"}";

    [Theory]
    [MemberData(nameof(DeserializeWithEntityParametersTest))]
    public async Task Can_DeserializePayload_WithEntityParameters(IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(EntityPayload));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext() { Path = path, Model = _model };

        // Act
        ODataActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
        IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

        // Assert
        Assert.Same(expectedAction, action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Id"));
        Assert.Equal(1, payload["Id"]);

        Assert.True(payload.ContainsKey("Customer"));
        Customer customer = payload["Customer"] as Customer;
        Assert.NotNull(customer);
        Assert.Equal(109, customer.Id);
        Assert.Equal("Avatar", customer.Name);

        Assert.False(payload.ContainsKey("NullableCustomer"));
    }

    [Theory]
    [MemberData(nameof(DeserializeWithEntityParametersTest))]
    public async Task Can_DeserializePayload_WithEntityParameters_InUntypedMode(IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(EntityPayload));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

        // Assert
        Assert.NotNull(payload);
        Assert.Same(expectedAction, payload.Action);

        Assert.True(payload.ContainsKey("Id"));
        Assert.Equal(1, payload["Id"]);

        Assert.True(payload.ContainsKey("Customer"));
        dynamic customer = payload["Customer"] as EdmEntityObject;
        Assert.IsType<EdmEntityObject>(customer);

        Assert.Equal(109, customer.Id);
        Assert.Equal("Avatar", customer.Name);

        Assert.False(payload.ContainsKey("NullableCustomer"));
    }

    private const string EntityCollectionPayload =
"{" +
"\"Id\": 1, " +
"\"Customers\": [" +
    "{\"@odata.type\":\"#A.B.Customer\", \"Id\":109,\"Name\":\"Avatar\" }, " +
    // null can't work. see: https://github.com/OData/odata.net/issues/100
    // "null," +
    "{\"@odata.type\":\"#A.B.Customer\", \"Id\":901,\"Name\":\"Robot\" } " +
    "]" +
"}";

    [Theory]
    [MemberData(nameof(DeserializeWithEntityCollectionsTest))]
    public async Task Can_DeserializePayload_WithEntityCollectionParameters(IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(EntityCollectionPayload));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext() { Path = path, Model = _model };

        // Act
        ODataActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context) as ODataActionParameters;
        IEdmAction action = ODataActionPayloadDeserializer.GetAction(context);

        // Assert
        Assert.Same(expectedAction, action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Id"));
        Assert.Equal(1, payload["Id"]);

        IList<Customer> customers = (payload["Customers"] as IEnumerable<Customer>).ToList();
        Assert.NotNull(customers);
        Assert.Equal(2, customers.Count);
        Customer customer = customers[0];
        Assert.NotNull(customer);
        Assert.Equal(109, customer.Id);
        Assert.Equal("Avatar", customer.Name);

        customer = customers[1];
        Assert.NotNull(customer);
        Assert.Equal(901, customer.Id);
        Assert.Equal("Robot", customer.Name);
    }

    [Theory]
    [MemberData(nameof(DeserializeWithEntityCollectionsTest))]
    public async Task Can_DeserializePayload_WithEntityCollectionParameters_InUntypedMode(IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        ODataMessageWrapper message = new ODataMessageWrapper(await GetStringAsStreamAsync(EntityCollectionPayload));
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message as IODataRequestMessage, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model, ResourceType = typeof(ODataUntypedActionParameters) };

        // Act
        ODataUntypedActionParameters payload = await _deserializer.ReadAsync(reader, typeof(ODataUntypedActionParameters), context) as ODataUntypedActionParameters;

        // Assert
        Assert.Same(expectedAction, payload.Action);
        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("Id"));
        Assert.Equal(1, payload["Id"]);

        IEnumerable<IEdmObject> customers = payload["Customers"] as EdmEntityObjectCollection;
        Assert.Equal(2, customers.Count());
        dynamic customer = customers.First();
        Assert.NotNull(customer);
        Assert.Equal(109, customer.Id);
        Assert.Equal("Avatar", customer.Name);

        customer = customers.Last();
        Assert.NotNull(customer);
        Assert.Equal(901, customer.Id);
        Assert.Equal("Robot", customer.Name);
    }

    [Fact]
    public void GetAction_ThrowsSerializationException_ForNonAction()
    {
        // Arrange
        ODataPath path = new ODataPath(MetadataSegment.Instance);
        ODataDeserializerContext context = new ODataDeserializerContext() { Path = path };

        // Act & Assert
        ExceptionAssert.Throws<SerializationException>(() => ODataActionPayloadDeserializer.GetAction(context),
            "The last segment of the request URI '$metadata' was not recognized as an OData action.");
    }

    [Theory]
    [MemberData(nameof(DeserializeWithPrimitiveParametersTest))]
    public void ThrowsAsync_ODataException_When_Parameter_Notfound(string actionName, IEdmAction expectedAction, ODataPath path)
    {
        // Arrange
        const string Body = @"{ ""Quantity"": 1 , ""ProductCode"": ""PCode"", ""MissingParameter"": 1 }";
        IODataRequestMessageAsync message = new ODataMessageWrapper(GetStringAsStreamAsync(Body).Result);
        message.SetHeader("Content-Type", "application/json");
        ODataMessageReader reader = new ODataMessageReader(message, new ODataMessageReaderSettings(), _model);
        ODataDeserializerContext context = new ODataDeserializerContext { Path = path, Model = _model };

        // Act & Assert
        Assert.NotNull(expectedAction);
        ExceptionAssert.Throws<ODataException>(() =>
        {
            ODataActionParameters payload = _deserializer.ReadAsync(reader, typeof(ODataActionParameters), context).Result as ODataActionParameters;
        }, "The parameter 'MissingParameter' in the request payload is not a valid parameter for the operation '" + actionName + "'.");
    }

    private static ODataPath CreateBoundPath(string actionName)
    {
        string path = String.Format("Customers(1)/A.B.{0}", actionName);
        ODataPath odataPath = new ODataUriParser(_model, new Uri(_serviceRoot), new Uri(path, UriKind.Relative)).ParsePath();
        Assert.NotNull(odataPath); // Guard
        return odataPath;
    }

    private static ODataPath CreateUnboundPath(string actionName)
    {
        string path = string.Format("{0}", actionName);
        ODataPath odataPath = new ODataUriParser(_model, new Uri(_serviceRoot), new Uri(path, UriKind.Relative)).ParsePath();
        Assert.NotNull(odataPath); // Guard
        return odataPath;
    }

    public static IEdmAction GetBoundAction(string actionName)
    {
        IEdmAction action = _model.SchemaElements.OfType<IEdmAction>().SingleOrDefault(a => a.Name == actionName);
        Assert.NotNull(action); // Guard
        return action;
    }

    public static IEdmAction GetUnboundAction(string actionName)
    {
        IEdmActionImport actionImport = _container.OperationImports().SingleOrDefault(a => a.Name == actionName) as IEdmActionImport;
        Assert.NotNull(actionImport); // Guard
        return actionImport.Action;
    }

    private static IEdmModel GetModel()
    {
        //var config = RoutingConfigurationFactory.CreateWithTypes(typeof(Customer));
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        builder.ContainerName = "C";
        builder.Namespace = "A.B";
        EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;

        // bound actions
        ActionConfiguration primitive = customer.Action("Primitive");
        primitive.Parameter<int>("Quantity");
        primitive.Parameter<string>("ProductCode");
        primitive.Parameter<Date>("Birthday");
        primitive.Parameter<AColor>("BkgColor");
        primitive.Parameter<AColor?>("InnerColor");

        ActionConfiguration complex = customer.Action("Complex");
        complex.Parameter<int>("Quantity");
        complex.Parameter<MyAddress>("Address");

        ActionConfiguration enumType = customer.Action("Enum");
        enumType.Parameter<AColor>("Color");

        ActionConfiguration primitiveCollection = customer.Action("PrimitiveCollection");
        primitiveCollection.Parameter<string>("Name");
        primitiveCollection.CollectionParameter<int>("Ratings");
        primitiveCollection.CollectionParameter<TimeOfDay>("Time");
        primitiveCollection.CollectionParameter<AColor?>("Colors");

        ActionConfiguration complexCollection = customer.Action("ComplexCollection");
        complexCollection.Parameter<string>("Name");
        complexCollection.CollectionParameter<MyAddress>("Addresses");

        ActionConfiguration enumCollection = customer.Action("EnumCollection");
        enumCollection.CollectionParameter<AColor>("Colors");

        ActionConfiguration entity = customer.Action("Entity");
        entity.Parameter<int>("Id");
        entity.EntityParameter<Customer>("Customer");
        entity.EntityParameter<Customer>("NullableCustomer");

        ActionConfiguration entityCollection = customer.Action("EntityCollection");
        entityCollection.Parameter<int>("Id");
        entityCollection.CollectionEntityParameter<Customer>("Customers");

        // unbound actions
        ActionConfiguration unboundPrimitive = builder.Action("UnboundPrimitive");
        unboundPrimitive.Parameter<int>("Quantity");
        unboundPrimitive.Parameter<string>("ProductCode");
        unboundPrimitive.Parameter<Date>("Birthday");
        unboundPrimitive.Parameter<AColor>("BkgColor");
        unboundPrimitive.Parameter<AColor?>("InnerColor");

        ActionConfiguration unboundComplex = builder.Action("UnboundComplex");
        unboundComplex.Parameter<int>("Quantity");
        unboundComplex.Parameter<MyAddress>("Address");

        ActionConfiguration unboundEnum = builder.Action("UnboundEnum");
        unboundEnum.Parameter<AColor>("Color");

        ActionConfiguration unboundPrimitiveCollection = builder.Action("UnboundPrimitiveCollection");
        unboundPrimitiveCollection.Parameter<string>("Name");
        unboundPrimitiveCollection.CollectionParameter<int>("Ratings");
        unboundPrimitiveCollection.CollectionParameter<TimeOfDay>("Time");
        unboundPrimitiveCollection.CollectionParameter<AColor?>("Colors");

        ActionConfiguration unboundComplexCollection = builder.Action("UnboundComplexCollection");
        unboundComplexCollection.Parameter<string>("Name");
        unboundComplexCollection.CollectionParameter<MyAddress>("Addresses");

        ActionConfiguration unboundEnumCollection = builder.Action("UnboundEnumCollection");
        unboundEnumCollection.CollectionParameter<AColor>("Colors");

        ActionConfiguration unboundEntity = builder.Action("UnboundEntity");
        unboundEntity.Parameter<int>("Id");
        unboundEntity.EntityParameter<Customer>("Customer").Nullable = false;
        unboundEntity.EntityParameter<Customer>("NullableCustomer");

        ActionConfiguration unboundEntityCollection = builder.Action("UnboundEntityCollection");
        unboundEntityCollection.Parameter<int>("Id");
        unboundEntityCollection.CollectionEntityParameter<Customer>("Customers");

        return builder.GetEdmModel();
    }

    private static async Task<Stream> GetStringAsStreamAsync(string body)
    {
        Stream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        await writer.WriteAsync(body);
        await writer.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    private class DerivedODataActionParameters : ODataActionParameters
    {
    }
}

public enum AColor
{
    Red,
    Blue,
    Green
}

public class MyAddress
{
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public int ZipCode { get; set; }
}
