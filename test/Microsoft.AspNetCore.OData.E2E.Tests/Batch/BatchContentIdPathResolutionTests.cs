//-----------------------------------------------------------------------------
// <copyright file="BatchContentIdPathResolutionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class BatchContentIdPathResolutionTests : WebApiTestBase<BatchContentIdPathResolutionTests>
{
    private const string DefaultPrefix = "DefaultContentIdPath";
    private const string UnbufferedPrefix = "UnbufferedContentIdPath";
    private const string RootPrefix = "";
    private static IEdmModel edmModel;
    private static bool pathSensitiveMiddlewareCalled;

    public BatchContentIdPathResolutionTests(WebApiTestFixture<BatchContentIdPathResolutionTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(BatchContentIdPathItemsController), typeof(BatchContentIdPathControlController));

        edmModel = GetEdmModel();
        services.AddControllers().AddOData(options =>
        {
            options.EnableContinueOnErrorHeader = true;
            options.AddRouteComponents(RootPrefix, edmModel, new DefaultODataBatchHandler());
            options.AddRouteComponents(DefaultPrefix, edmModel, new DefaultODataBatchHandler());
            options.AddRouteComponents(UnbufferedPrefix, edmModel, new UnbufferedODataBatchHandler());
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/internal-export")
            {
                pathSensitiveMiddlewareCalled = true;
                context.Response.StatusCode = StatusCodes.Status200OK;
                return;
            }

            await next();
        });

        app.UseODataBatching();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    [Theory]
    [InlineData(DefaultPrefix)]
    [InlineData(UnbufferedPrefix)]
    public async Task ContentIdResolutionDispatchesInRootPath(string routePrefix)
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/" + routePrefix + @"/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""GET"",
                    ""url"": ""/" + routePrefix + @"/?$1"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(routePrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status200OK }, statuses);
    }

    [Theory]
    [InlineData(DefaultPrefix)]
    [InlineData(UnbufferedPrefix)]
    public async Task ContentIdResolutionRejectsOutsideGetAndContinuesWithInRootRequest(string routePrefix)
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/" + routePrefix + @"/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""GET"",
                    ""url"": ""/" + routePrefix + @"/?$1/../../../BatchContentIdPathControl/read"",
                    ""dependsOn"": [""1""]
                },
                {
                    ""id"": ""3"",
                    ""method"": ""GET"",
                    ""url"": ""/" + routePrefix + @"/?$1"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(routePrefix, payload, continueOnError: true);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status400BadRequest, StatusCodes.Status200OK }, statuses);
    }

    [Theory]
    [InlineData(DefaultPrefix)]
    [InlineData(UnbufferedPrefix)]
    public async Task ContentIdResolutionRejectsOutsidePatchAndContinuesWithInRootRequest(string routePrefix)
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/" + routePrefix + @"/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""PATCH"",
                    ""url"": ""/" + routePrefix + @"/?$1/../../../BatchContentIdPathControl/write"",
                    ""dependsOn"": [""1""],
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Value"": ""changed""
                    }
                },
                {
                    ""id"": ""3"",
                    ""method"": ""GET"",
                    ""url"": ""/" + routePrefix + @"/?$1"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(routePrefix, payload, continueOnError: true);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status400BadRequest, StatusCodes.Status200OK }, statuses);
    }

    [Theory]
    [InlineData(DefaultPrefix)]
    [InlineData(UnbufferedPrefix)]
    public async Task ContentIdResolutionRejectsOutsidePostWithoutChangingControlState(string routePrefix)
    {
        // Arrange
        BatchContentIdPathControlController.CurrentValue = "initial";
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/" + routePrefix + @"/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""POST"",
                    ""url"": ""/" + routePrefix + @"/?$1/../../../BatchContentIdPathControl/write"",
                    ""dependsOn"": [""1""],
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Value"": ""changed""
                    }
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(routePrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status400BadRequest }, statuses);
        Assert.Equal("initial", BatchContentIdPathControlController.CurrentValue);
    }

    [Fact]
    public async Task ContentIdResolutionRejectsNonODataEndpointWhenServiceIsRegisteredAtHostRoot()
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""GET"",
                    ""url"": ""/?$1/../../BatchContentIdPathControl/read"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(RootPrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status400BadRequest }, statuses);
    }

    [Fact]
    public async Task ContentIdResolutionAllowsODataEndpointWhenServiceIsRegisteredAtHostRoot()
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""GET"",
                    ""url"": ""/?$1"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(RootPrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status200OK }, statuses);
    }

    [Fact]
    public async Task DirectRequestRejectsNonODataEndpointWhenServiceIsRegisteredAtHostRoot()
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": ""/BatchContentIdPathControl/read""
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(RootPrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status400BadRequest }, statuses);
    }

    [Fact]
    public async Task ContentIdResolutionRejectsNonODataPostWhenServiceIsRegisteredAtHostRoot()
    {
        // Arrange
        BatchContentIdPathControlController.CurrentValue = "initial";
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""POST"",
                    ""url"": ""/?$1/../../BatchContentIdPathControl/write"",
                    ""dependsOn"": [""1""],
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Value"": ""changed""
                    }
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(RootPrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status400BadRequest }, statuses);
        Assert.Equal("initial", BatchContentIdPathControlController.CurrentValue);
    }

    [Theory]
    [InlineData(DefaultPrefix)]
    [InlineData(UnbufferedPrefix)]
    public async Task ContentIdResolutionRejectsNonODataEndpointWithinServiceRoot(string routePrefix)
    {
        // Arrange
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/" + routePrefix + @"/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""GET"",
                    ""url"": ""/" + routePrefix + @"/?$1/../../" + routePrefix + @"/BatchContentIdPathControl/read"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(routePrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status400BadRequest }, statuses);
    }

    [Fact]
    public async Task ContentIdResolutionDoesNotReachPathSensitiveMiddlewareRegisteredBeforeBatching()
    {
        // Arrange
        pathSensitiveMiddlewareCalled = false;
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/BatchContentIdPathItems"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {
                        ""Id"": 1,
                        ""Name"": ""Item 1""
                    }
                },
                {
                    ""id"": ""2"",
                    ""method"": ""GET"",
                    ""url"": ""/?$1/../../internal-export"",
                    ""dependsOn"": [""1""]
                }
            ]
        }";

        // Act
        IList<int> statuses = await SendBatchAsync(RootPrefix, payload);

        // Assert
        Assert.Equal(new[] { StatusCodes.Status201Created, StatusCodes.Status404NotFound }, statuses);
        Assert.False(pathSensitiveMiddlewareCalled);
    }

    private static IEdmModel GetEdmModel()
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<BatchContentIdPathItem>("BatchContentIdPathItems");
        return builder.GetEdmModel();
    }

    private async Task<IList<int>> SendBatchAsync(string routePrefix, string payload, bool continueOnError = false)
    {
        HttpClient client = CreateClient();
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/" + (string.IsNullOrEmpty(routePrefix) ? string.Empty : routePrefix + "/") + "$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Headers.TryAddWithoutValidation("OData-Version", "4.0");
        if (continueOnError)
        {
            request.Headers.TryAddWithoutValidation("Prefer", "odata.continue-on-error");
        }

        request.Content = new StringContent(payload);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        HttpResponseMessage response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var statuses = new List<int>();
        Stream responseStream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage responseMessage = new ODataMessageWrapper(responseStream, response.Content.Headers);
        using ODataMessageReader messageReader = new ODataMessageReader(responseMessage, new ODataMessageReaderSettings(), edmModel);
        ODataBatchReader batchReader = messageReader.CreateODataBatchReader();
        while (batchReader.Read())
        {
            if (batchReader.State == ODataBatchReaderState.Operation)
            {
                statuses.Add(batchReader.CreateOperationResponseMessage().StatusCode);
            }
        }

        return statuses;
    }
}

public class BatchContentIdPathItemsController : ODataController
{
    public ActionResult<BatchContentIdPathItem> Get(int key)
    {
        return Ok(new BatchContentIdPathItem { Id = key, Name = "Item " + key });
    }

    public ActionResult<BatchContentIdPathItem> Post([FromBody] BatchContentIdPathItem item)
    {
        return Created(new System.Uri($"{Request.Scheme}://{Request.Host}{Request.Path}({item.Id})"), item);
    }
}

[ApiController]
[Route("BatchContentIdPathControl")]
public class BatchContentIdPathControlController : ControllerBase
{
    public static string CurrentValue { get; set; } = "initial";

    [HttpGet("read")]
    [HttpGet("/DefaultContentIdPath/BatchContentIdPathControl/read")]
    [HttpGet("/UnbufferedContentIdPath/BatchContentIdPathControl/read")]
    public IActionResult Get()
    {
        return Ok("control-read");
    }

    [HttpPost("write")]
    public IActionResult Post([FromBody] BatchContentIdPathControlValue value)
    {
        CurrentValue = value.Value;
        return Ok();
    }
}

public class BatchContentIdPathItem
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
}

public class BatchContentIdPathControlValue
{
    public string Value { get; set; }
}
