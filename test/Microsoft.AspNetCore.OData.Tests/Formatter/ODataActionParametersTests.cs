//-----------------------------------------------------------------------------
// <copyright file="ODataActionParametersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter;

public class ODataActionParametersTests
{
    [Fact]
    public async ValueTask BindAsync_ThrowsArgumentNull_ForInputs()
    {
        // Arrange & Act & Assert
        ArgumentNullException exception = await ExceptionAssert.ThrowsAsync<ArgumentNullException>(async () => await ODataActionParameters.BindAsync(null, null), "httpContext", false, true);
        Assert.Equal("The parameter cannot be null. (Parameter 'httpContext')", exception.Message);

        // Arrange & Act & Assert
        HttpContext httpContext = new DefaultHttpContext();
        await ExceptionAssert.ThrowsAsync<ArgumentNullException>(async () => await ODataActionParameters.BindAsync(httpContext, null));
        Assert.Equal("The parameter cannot be null. (Parameter 'parameter')", exception.Message);
    }

    [Fact]
    public async ValueTask BindAsync_Returns_ValidODataActionParameter()
    {
        // Arrange
        Mock<IODataDeserializerProvider> deserializerProviderMock = new Mock<IODataDeserializerProvider>();
        Mock<ODataActionPayloadDeserializer> mock = new Mock<ODataActionPayloadDeserializer>(deserializerProviderMock.Object);

        ODataActionParameters expectedParameters = new ODataActionParameters();

        mock.Setup(m => m.ReadAsync(It.IsAny<ODataMessageReader>(), typeof(ODataActionParameters), It.IsAny<ODataDeserializerContext>()))
            .ReturnsAsync(expectedParameters);

        HttpContext httpContext = new DefaultHttpContext();

        ODataMiniMetadata metadata = new ODataMiniMetadata();
        metadata.Model = EdmCoreModel.Instance;
        metadata.PathFactory = (c, t) => new ODataPath();
        metadata.BaseAddressFactory = c => new Uri("http://localhost/odata/");
        metadata.Services = services => services.AddSingleton<ODataActionPayloadDeserializer>(s => mock.Object);

        Endpoint endpoint = new Endpoint(
            (context) => Task.CompletedTask,
            new EndpointMetadataCollection([metadata]),
            "TestEndpoint");

        httpContext.SetEndpoint(endpoint);
        ParameterInfo parameter = typeof(ODataActionParametersTests).GetMethod("TestMethod", BindingFlags.NonPublic | BindingFlags.Static).GetParameters().First();

        ODataActionParameters actualParameter = await ODataActionParameters.BindAsync(httpContext, parameter);

        // Act & Assert
        Assert.Same(expectedParameters, actualParameter);
    }

    // This empty method is used to provide a parameter for the BindAsync test.
    private static void TestMethod(ODataActionParameters parameters) { }
}
