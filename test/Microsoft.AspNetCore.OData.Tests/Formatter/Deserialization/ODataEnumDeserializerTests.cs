//-----------------------------------------------------------------------------
// <copyright file="ODataEnumDeserializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;

public class ODataEnumDeserializerTests
{
    private static IEdmModel _edmModel = GetEdmModel();

    [Fact]
    public async Task ReadAsync_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        var deserializer = new ODataEnumDeserializer();
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => deserializer.ReadAsync(messageReader: null, type: null, readContext: null), "messageReader");

        // Arrange & Act & Assert
        ODataMessageReader messageReader = ODataFormatterHelpers.GetMockODataMessageReader();
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => deserializer.ReadAsync(messageReader, type: null, readContext: null), "type");

        // Arrange & Act & Assert
        await ExceptionAssert.ThrowsArgumentNullAsync(
            () => deserializer.ReadAsync(messageReader, typeof(Color), readContext: null), "readContext");
    }

    [Fact]
    public async Task ReadAsync_Works_ForEnumValue()
    {
        // Arrange
        string content = "{\"@odata.type\":\"#NS.Color\",\"value\":\"Blue\"}";

        ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
        ODataDeserializerContext readContext = new ODataDeserializerContext
        {
            Model = _edmModel,
            ResourceType = typeof(Color)
        };

        HttpRequest request = RequestFactory.Create("Post", "http://localhost/TestUri", opt => opt.AddRouteComponents("odata", _edmModel));

        // Act
        object value = await deserializer.ReadAsync(ODataTestUtil.GetODataMessageReader(request.GetODataMessage(content), _edmModel),
            typeof(Color), readContext);

        // Assert
        Color color = Assert.IsType<Color>(value);
        Assert.Equal(Color.Blue, color);
    }

    [Fact]
    public async Task ReadAsync_Works_ForRawValue()
    {
        // Arrange
        string content = "{\"value\":\"Blue\"}";

        ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
        ODataDeserializerContext readContext = new ODataDeserializerContext
        {
            Model = _edmModel,
            ResourceType = typeof(Color)
        };
        HttpRequest request = RequestFactory.Create("Post", "http://localhost/", _edmModel);

        // Act
        object value = await deserializer.ReadAsync(ODataTestUtil.GetODataMessageReader(request.GetODataMessage(content), _edmModel),
            typeof(Color), readContext);

        // Assert
        Color color = Assert.IsType<Color>(value);
        Assert.Equal(Color.Blue, color);
    }

    [Fact]
    public async Task ReadAsync_Works_ForUnType()
    {
        // Arrange
        string content = "{\"@odata.type\":\"#NS.Color\",\"value\":\"Blue\"}";

        ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
        ODataDeserializerContext readContext = new ODataDeserializerContext
        {
            Model = _edmModel,
            ResourceType = typeof(IEdmEnumObject)
        };
        HttpRequest request = RequestFactory.Create("Post", "http://localhost/", _edmModel);

        // Act
        object value = await deserializer.ReadAsync(ODataTestUtil.GetODataMessageReader(request.GetODataMessage(content), _edmModel),
            typeof(Color), readContext);

        // Assert
        EdmEnumObject color = Assert.IsType<EdmEnumObject>(value);
        Assert.NotNull(color);

        Assert.Equal("Blue", color.Value);
    }

    [Theory]
    [InlineData("{\"@odata.type\":\"#NS.level\",\"value\":\"veryhigh\"}", Level.High)]
    [InlineData("{\"@odata.type\":\"#NS.level\",\"value\":\"High\"}", Level.High)]
    [InlineData("{\"@odata.type\":\"#NS.level\",\"value\":\"low\"}", Level.Low)]
    [InlineData("{\"@odata.type\":\"#NS.level\",\"value\":\"Low\"}", Level.Low)]
    public async Task ReadAsync_Works_ForModelAlias(string content, Level expectedLevel)
    {
        // Arrange
        var builder = new ODataConventionModelBuilder();
        builder.EnumType<Level>().Namespace = "NS";
        IEdmModel model = builder.GetEdmModel();

        ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
        ODataDeserializerContext readContext = new ODataDeserializerContext
        {
            Model = model,
            ResourceType = typeof(Level)
        };

        HttpRequest request = RequestFactory.Create("Post", "http://localhost/", _edmModel);

        // Act
        object value = await deserializer.ReadAsync(ODataTestUtil.GetODataMessageReader(request.GetODataMessage(content), model),
            typeof(Level), readContext);

        // Assert
        Level level = Assert.IsType<Level>(value);
        Assert.Equal(expectedLevel, level);
    }

    [Theory]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"monday\"}", Day.Monday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday\"}", Day.Monday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"monday, tuesday\"}", Day.Monday | Day.Tuesday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, tuesday\"}", Day.Monday | Day.Tuesday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, Tuesday\"}", Day.Monday | Day.Tuesday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"monday, tuesday, thursday\"}", Day.Monday | Day.Tuesday | Day.Thursday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, Tuesday, thursday\"}", Day.Monday | Day.Tuesday | Day.Thursday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, tuesday, Thursday\"}", Day.Monday | Day.Tuesday | Day.Thursday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, Tuesday, Thursday\"}", Day.Monday | Day.Tuesday | Day.Thursday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, Friday\"}", Day.Monday | Day.Friday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, tuesday,  fri\"}", Day.Monday | Day.Tuesday | Day.Friday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, fri, Wednesday\"}", Day.Monday | Day.Friday | Day.Wednesday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"Monday, fri\"}", Day.Monday | Day.Friday)]
    [InlineData("{\"@odata.type\":\"#NS.day\",\"value\":\"monday, fri\"}", Day.Monday | Day.Friday)]
    public async Task ReadAsync_Works_ForModelAliasWithFlags(string content, Day expectedDay)
    {
        // Arrange
        var builder = new ODataConventionModelBuilder();
        builder.EnumType<Day>().Namespace = "NS";
        IEdmModel model = builder.GetEdmModel();

        ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
        ODataDeserializerContext readContext = new ODataDeserializerContext
        {
            Model = model,
            ResourceType = typeof(Day)
        };

        HttpRequest request = RequestFactory.Create("Post", "http://localhost/", _edmModel);

        // Act
        object value = await deserializer.ReadAsync(ODataTestUtil.GetODataMessageReader(request.GetODataMessage(content), model),
            typeof(Day), readContext);

        // Assert
        Day day = Assert.IsType<Day>(value);
        Assert.Equal(expectedDay, day);
    }

    [Fact]
    public void ReadInline_ThrowsArgumentNull_ForInputs()
    {
        // Arrange
        ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
        Mock<IEdmTypeReference> edmType = new Mock<IEdmTypeReference>();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadInline(new object(), edmType.Object, readContext: null), "readContext");
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EnumType<Color>().Namespace = "NS";
        return builder.GetEdmModel();
    }

    public enum Color
    {
        Red,
        Blue,
        Green
    }

    [DataContract(Name = "level")]
    public enum Level
    {
        [EnumMember(Value = "low")]
        Low,

        [EnumMember(Value = "veryhigh")]
        High
    }

    [Flags]
    [DataContract(Name = "day")]
    public enum Day
    {
        [EnumMember(Value = "monday")]
        Monday = 1,

        [EnumMember(Value = "tuesday")]
        Tuesday = 2,

        [EnumMember(Value = "wednesday")]
        Wednesday = 4,

        [EnumMember(Value = "thursday")]
        Thursday = 8,

        [EnumMember(Value = "fri")]
        Friday = 16
    }
}
