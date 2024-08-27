//-----------------------------------------------------------------------------
// <copyright file="DefaultODataBatchHandlerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class DefaultODataBatchHandlerTest
    {
        private static IServiceProvider _serviceProvider = BuildServiceProvider();

        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act & Assert
            Assert.False(batchHandler.ContinueOnError);
            Assert.NotNull(batchHandler.MessageQuotas);
            Assert.Null(batchHandler.PrefixName);
        }

        [Fact]
        public async Task CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            HttpContext context = new DefaultHttpContext();
            context.ODataFeature().Services = _serviceProvider;
            HttpRequest request = context.Request;

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.CreateResponseMessageAsync(null, request), "responses");
        }

        [Fact]
        public async Task CreateResponseMessageAsync_Throws_IfRequestIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.CreateResponseMessageAsync(new ODataBatchResponseItem[0], null), "request");
        }

        [Fact]
        public async Task CreateResponseMessageAsync_ReturnsODataBatchContent()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            HttpRequest request = RequestFactory.Create(opt => opt.AddRouteComponents("odata", EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = "odata";
            HttpContext httpContext = request.HttpContext;
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.Body = new MemoryStream();
            ODataBatchResponseItem[] responses = new ODataBatchResponseItem[]
            {
                new OperationResponseItem(httpContext)
            };

            // Act
            await batchHandler.CreateResponseMessageAsync(responses, request);

            // Assert
            string responseString = httpContext.Response.ReadBody();
            Assert.Contains("200 OK", responseString);
        }

        [Fact]
        public async Task ProcessBatchAsync_Throws_IfContextIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.ProcessBatchAsync(null, null), "context");
        }


        [Theory]
        [InlineData(true, null, false)]
        [InlineData(true, "odata.continue-on-error", true)]
        [InlineData(true, "odata.continue-on-error=true", true)]
        [InlineData(true, "odata.continue-on-error=false", false)]
        [InlineData(true, "continue-on-error", true)]
        [InlineData(true, "continue-on-error=true", true)]
        [InlineData(true, "continue-on-error=false", false)]
        [InlineData(false, null, true)]
        [InlineData(false, "odata.continue-on-error", true)]
        [InlineData(false, "odata.continue-on-error=true", true)]
        [InlineData(false, "odata.continue-on-error=false", true)]
        [InlineData(false, "continue-on-error", true)]
        [InlineData(false, "continue-on-error=true", true)]
        [InlineData(false, "continue-on-error=false", true)]
        public async Task ProcessBatchAsync_ContinueOnError(bool enableContinueOnError, string preferenceHeader, bool hasThirdResponse)
        {
            // Arrange
            RequestDelegate handler = async context =>
            {
                HttpRequest request = context.Request;
                string responseContent = request.GetDisplayUrl();
                string content = request.ReadBody();
                if (!string.IsNullOrEmpty(content))
                {
                    responseContent += "," + content;
                }

                HttpResponse response = context.Response;
                if (content.Equals("foo"))
                {
                    response.StatusCode = StatusCodes.Status400BadRequest;
                }

                await response.WriteAsync(responseContent);
            };

            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            HttpContext httpContext = HttpContextHelper.Create("Post", "http://example.com/$batch");

            string batchRequest = @"--d3df74a8-8212-4c2a-b4fb-d713a4ba383e
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1948857409

GET / HTTP/1.1
Host: example.com


--d3df74a8-8212-4c2a-b4fb-d713a4ba383e
Content-Type: multipart/mixed; boundary=""3c4d5753-d325-4806-8c80-38f4a5fbe523""

--3c4d5753-d325-4806-8c80-38f4a5fbe523
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1856004745

POST /values HTTP/1.1
Host: example.com
Content-Type: text/plain; charset=utf-8

foo
--3c4d5753-d325-4806-8c80-38f4a5fbe523--

--d3df74a8-8212-4c2a-b4fb-d713a4ba383e
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: -2010824035

POST /values HTTP/1.1
Host: example.com
Content-Type: text/plain; charset=utf-8

bar
--d3df74a8-8212-4c2a-b4fb-d713a4ba383e--
";

            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            httpContext.Request.Body = new MemoryStream(requestBytes);
            httpContext.Request.ContentType = "multipart/mixed;boundary=\"d3df74a8-8212-4c2a-b4fb-d713a4ba383e\"";
            httpContext.Request.ContentLength = 827;
            httpContext.Request.Method = "Post";

            IEdmModel model = new EdmModel();
            httpContext.ODataFeature().RoutePrefix = "odata";
            httpContext.RequestServices = BuildServiceProvider(opt => opt.AddRouteComponents("odata", model).EnableContinueOnErrorHeader = enableContinueOnError);

            if (preferenceHeader != null)
            {
                httpContext.Request.Headers.Append("prefer", preferenceHeader);
            }

            httpContext.Response.Body = new MemoryStream();
            batchHandler.PrefixName = "odata";

            // Act
            await batchHandler.ProcessBatchAsync(httpContext, handler);
            string responseBody = httpContext.Response.ReadBody();

            // Assert
            Assert.NotNull(responseBody);

            // #1 response
            Assert.Contains("http://example.com/", responseBody);

            // #2 bad response
            Assert.Contains("Bad Request", responseBody);
            Assert.Contains("http://example.com/values,foo", responseBody);

            // #3 response
            if (hasThirdResponse)
            {
                Assert.Contains("http://example.com/values,bar", responseBody);
            }
            else
            {
                Assert.DoesNotContain("http://example.com/values,bar", responseBody);
            }
        }

        [Fact]
        public async Task ProcessBatchAsync_DoesNotCopyContentHeadersToGetAndDelete()
        {
            // Arrange
            string batchRequest = @"
--40e2c6b6-e6ce-43aa-9985-ddc12dc4bb9b
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 483818399

GET / HTTP/1.1
Host: example.com


--40e2c6b6-e6ce-43aa-9985-ddc12dc4bb9b
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1035669256

DELETE / HTTP/1.1
Host: example.com


--40e2c6b6-e6ce-43aa-9985-ddc12dc4bb9b
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 632310651

POST /values HTTP/1.1
Host: example.com
Content-Type: text/plain; charset=utf-8

bar
--40e2c6b6-e6ce-43aa-9985-ddc12dc4bb9b--
";

            RequestDelegate handler = async context =>
            {
                HttpRequest request = context.Request;
                string responseContent = $"{request.Method},{request.ContentLength},{request.ContentType}";
                await context.Response.WriteAsync(responseContent);
            };

            HttpContext httpContext = HttpContextHelper.Create("Post", "http://example.com/$batch");
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            httpContext.Request.Body = new MemoryStream(requestBytes);
            httpContext.Request.ContentType = "multipart/mixed;boundary=40e2c6b6-e6ce-43aa-9985-ddc12dc4bb9b";
            httpContext.Request.ContentLength = requestBytes.Length;

            IEdmModel model = new EdmModel();
            httpContext.ODataFeature().RoutePrefix = "odata";
            httpContext.RequestServices = BuildServiceProvider(opt => opt.AddRouteComponents("odata", model));

            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            httpContext.Response.Body = new MemoryStream();
            batchHandler.PrefixName = "odata";

            // Act
            await batchHandler.ProcessBatchAsync(httpContext, handler);
            string responseBody = httpContext.Response.ReadBody();

            // Assert
            Assert.NotNull(responseBody);
            Assert.Contains("GET,,", responseBody);
            Assert.Contains("DELETE,,", responseBody);
            Assert.Contains("POST,3,text/plain; charset=utf-8", responseBody);
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_CallsInvokerForEachRequest()
        {
            // Arrange
            RequestDelegate handler = async context =>
            {
                HttpRequest request = context.Request;
                string responseContent = request.GetDisplayUrl();
                string content = request.ReadBody();
                if (!string.IsNullOrEmpty(content))
                {
                    responseContent += "," + content;
                }

                context.Response.Body = new MemoryStream();
                await context.Response.WriteAsync(responseContent);
            };

            ODataBatchRequestItem[] requests = new ODataBatchRequestItem[]
            {
                new OperationRequestItem(HttpContextHelper.Create("Get", "http://example.com/")),
                new ChangeSetRequestItem(new HttpContext[]
                {
                    HttpContextHelper.Create("Post", "http://example.com/values", "foo", "text/plan")
                })
            };
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act
            IList<ODataBatchResponseItem> responses = await batchHandler.ExecuteRequestMessagesAsync(requests, handler);

            // Assert
            Assert.Equal(2, responses.Count);

            // #1
            OperationResponseItem response0 = Assert.IsType<OperationResponseItem>(responses[0]);
            Assert.Equal("http://example.com/", response0.Context.Response.ReadBody());

            // #2
            ChangeSetResponseItem response1 = Assert.IsType<ChangeSetResponseItem>(responses[1]);
            HttpContext subContext = Assert.Single(response1.Contexts);
            Assert.Equal("http://example.com/values,foo", response1.Contexts.First().Response.ReadBody());
        }

        private static IServiceProvider BuildServiceProvider(Action<ODataOptions> setupAction)
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task ExecuteRequestMessagesAsync_Throws_IfRequestsIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.ExecuteRequestMessagesAsync(null, null), "requests");
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_Throws_IfRequestIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.ParseBatchRequestsAsync(null), "context");
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_Returns_RequestsFromMultipartContent()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            string batchRequest = @"
--7289e6c2-adbd-4dd8-bfb1-f36099442947
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1430195875

GET / HTTP/1.1
Host: example.com


--7289e6c2-adbd-4dd8-bfb1-f36099442947
Content-Type: multipart/mixed;boundary=e58fa556-67f1-4180-b04e-e28df22ac4d9

--e58fa556-67f1-4180-b04e-e28df22ac4d9
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 675766617

POST /values HTTP/1.1
Host: example.com


--e58fa556-67f1-4180-b04e-e28df22ac4d9--

--7289e6c2-adbd-4dd8-bfb1-f36099442947--
";

            HttpContext httpContext = HttpContextHelper.Create("Post", "http://example.com/$batch");
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            httpContext.Request.Body = new MemoryStream(requestBytes);
            httpContext.Request.ContentType = "multipart/mixed;boundary=7289e6c2-adbd-4dd8-bfb1-f36099442947";
            httpContext.Request.ContentLength = requestBytes.Length;

            IEdmModel model = new EdmModel();
            httpContext.ODataFeature().RoutePrefix = "odata";
            httpContext.RequestServices = BuildServiceProvider(opt => opt.AddRouteComponents("odata", model));
            httpContext.Response.Body = new MemoryStream();
            batchHandler.PrefixName = "odata";

            // Act
            IList<ODataBatchRequestItem> requests = await batchHandler.ParseBatchRequestsAsync(httpContext);

            // Assert
            Assert.Equal(2, requests.Count);

            var operationContext = Assert.IsType<OperationRequestItem>(requests[0]).Context;
            Assert.Equal("GET", operationContext.Request.Method);
            Assert.Equal("http://example.com/", operationContext.Request.GetDisplayUrl());

            var changeSetContexts = Assert.IsType<ChangeSetRequestItem>(requests[1]).Contexts;
            var changeSetContext = Assert.Single(changeSetContexts);
            Assert.Equal("POST", changeSetContext.Request.Method);
            Assert.Equal("http://example.com/values", changeSetContext.Request.GetDisplayUrl());
        }

        [Fact]
        public async Task ParseBatchRequestsAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            string batchRequest = @"
--7289e6c2-adbd-4dd8-bfb1-f36099442947
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1430195875

GET / HTTP/1.1
Host: example.com


--7289e6c2-adbd-4dd8-bfb1-f36099442947
Content-Type: multipart/mixed;boundary=e58fa556-67f1-4180-b04e-e28df22ac4d9

--e58fa556-67f1-4180-b04e-e28df22ac4d9
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 675766617

POST /values HTTP/1.1
Host: example.com


--e58fa556-67f1-4180-b04e-e28df22ac4d9--

--7289e6c2-adbd-4dd8-bfb1-f36099442947--
";
            HttpContext httpContext = HttpContextHelper.Create("Post", "http://example.com/$batch");
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            httpContext.Request.Body = new MemoryStream(requestBytes);
            httpContext.Request.ContentType = "multipart/mixed;boundary=7289e6c2-adbd-4dd8-bfb1-f36099442947";
            httpContext.Request.ContentLength = requestBytes.Length;

            IEdmModel model = new EdmModel();
            httpContext.ODataFeature().RoutePrefix = "odata";
            httpContext.RequestServices = BuildServiceProvider(opt => opt.AddRouteComponents("odata", model));
            httpContext.Response.Body = new MemoryStream();
            batchHandler.PrefixName = "odata";

            httpContext.Features[typeof(DefaultODataBatchHandlerTest)] = "bar";

            // Act
            IList<ODataBatchRequestItem> requests = await batchHandler.ParseBatchRequestsAsync(httpContext);

            // Assert
            Assert.Equal(2, requests.Count);

            var operationContext = ((OperationRequestItem)requests[0]).Context;
            Assert.Equal("GET", operationContext.Request.Method);
            Assert.Equal("http://example.com/", operationContext.Request.GetDisplayUrl());
            Assert.Equal("bar", operationContext.Features[typeof(DefaultODataBatchHandlerTest)]);

            var changeSetContext = ((ChangeSetRequestItem)requests[1]).Contexts.First();
            Assert.Equal("POST", changeSetContext.Request.Method);
            Assert.Equal("http://example.com/values", changeSetContext.Request.GetDisplayUrl());
            Assert.Equal("bar", operationContext.Features[typeof(DefaultODataBatchHandlerTest)]);
        }

        [Fact]
        public void ValidateRequest_Throws_IfRequestIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => batchHandler.ValidateRequest(null), "request");
        }

        [Fact]
        public async Task ValidateRequest_GetBadResponse_IfRequestContentIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            request.Body = null;
            context.Response.Body = new MemoryStream();

            // Act
            await batchHandler.ValidateRequest(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("The 'Body' property on the batch request cannot be null.", context.Response.ReadBody());
        }

        [Fact]
        public async Task ValidateRequest_GetBadResponse_IfRequestContentTypeIsNull()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            request.Body = new MemoryStream();
            request.ContentType = null;
            context.Response.Body = new MemoryStream();

            // Act
            await batchHandler.ValidateRequest(request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("The batch request must have a \"Content-Type\" header.", context.Response.ReadBody());
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestMediaTypeIsWrong()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            HttpContext context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();
            context.Request.ContentType = "text/json";
            context.Response.Body = new MemoryStream();

            // Act
            await batchHandler.ValidateRequest(context.Request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("The batch request must have 'multipart/mixed' or 'application/json' as the media type.", context.Response.ReadBody());
        }

        [Fact]
        public async Task ValidateRequest_Throws_IfRequestContentTypeDoesNotHaveBoundary()
        {
            // Arrange
            DefaultODataBatchHandler batchHandler = new DefaultODataBatchHandler();
            HttpContext context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream();
            context.Request.ContentType = "multipart/mixed";
            context.Response.Body = new MemoryStream();

            // Act
            await batchHandler.ValidateRequest(context.Request);

            // Assert
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("The batch request must have a boundary specification in the \"Content-Type\" header.", context.Response.ReadBody());
        }

        [Fact]
        public async Task SendAsync_Works_ForBatchRequestWithInsertedEntityReferencedInAnotherRequest()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.ConfigureControllers(typeof(BatchTestCustomersController), typeof(BatchTestOrdersController));
                    var builder = new ODataConventionModelBuilder();
                    builder.EntitySet<BatchTestCustomer>("BatchTestCustomers");
                    builder.EntitySet<BatchTestOrder>("BatchTestOrders");
                    IEdmModel model = builder.GetEdmModel();
                    services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", model, new DefaultODataBatchHandler()).Expand());
                })
                .Configure(app =>
                {
                    app.UseODataBatching();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            var server = new TestServer(builder);

            const string acceptJsonFullMetadata = "application/json;odata.metadata=minimal";
            const string acceptJson = "application/json";

            var client = server.CreateClient();

            var endpoint = "http://localhost/odata";

            var batchRef = $"batch_{Guid.NewGuid()}";
            var changesetRef = $"changeset_{Guid.NewGuid()}";

            var orderId = 2;
            var createOrderPayload = $@"{{""@odata.type"":""Microsoft.AspNetCore.OData.Test.Batch.BatchTestOrder"",""Id"":{orderId},""Amount"":50}}";
            var createRefPayload = @"{""@odata.id"":""$3""}";

            var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/$batch");
            batchRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            StringContent httpContent = new StringContent($@"
--{batchRef}
Content-Type: multipart/mixed; boundary={changesetRef}

--{changesetRef}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 3

POST {endpoint}/BatchTestOrders HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: {acceptJsonFullMetadata}
Accept: {acceptJsonFullMetadata}
Accept-Charset: UTF-8

{createOrderPayload}
--{changesetRef}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 4

POST {endpoint}/BatchTestCustomers(2)/Orders/$ref HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: {acceptJsonFullMetadata}
Accept: {acceptJsonFullMetadata}
Accept-Charset: UTF-8

{createRefPayload}
--{changesetRef}--
--{batchRef}--
");

            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchRef}");
            batchRequest.Content = httpContent;

            // Act
            var response = await client.SendAsync(batchRequest);

            // Assert
            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            HttpRequestMessage customerRequest = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/BatchTestCustomers(2)?$expand=Orders");
            customerRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptJson));

            var customerResponse = client.SendAsync(customerRequest).Result;
            var objAsJsonString = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<BatchTestCustomer>(objAsJsonString);

            Assert.NotNull(customer.Orders?.SingleOrDefault(d => d.Id.Equals(orderId)));
        }

        public static readonly TheoryDataSet<IEnumerable<string>, string, string, string> _batchHeadersTestData = new TheoryDataSet<IEnumerable<string>, string, string, string>()
        {
            {
                // should not copy over content type and content length headers to individual request
                Enumerable.Empty<string>(),
                "GET,ContentType=,ContentLength=,Prefer=",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer="
            },
            {
                // should not copy over preferences that should not be inherited
                new []
                {
                    "respond-async, odata.continue-on-error"
                },
                "GET,ContentType=,ContentLength=,Prefer=",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer="
            },
            {
                // should not concatenate preferences that should not be inherited
                new []
                {
                    "wait=100,handling=lenient"
                },
                "GET,ContentType=,ContentLength=,Prefer=",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer="
            },
            {
                // inheritable preferences should be copied over
                // and combined with the individual request's own preferences if any
                new []
                {
                    "allow-entityreferences, include-annotations=\"display.*\""
                },
                "GET,ContentType=,ContentLength=,Prefer=allow-entityreferences,include-annotations=\\\"display.*\\\"",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient,allow-entityreferences,include-annotations=\\\"display.*\\\"",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer=allow-entityreferences,include-annotations=\\\"display.*\\\""
            },
            {
                // if batch Prefer header contains both inheritable and non-inheritable preferences,
                // the non-inheritable ones should be removed before merging with individual request's own preferences
                new []
                {
                    "allow-entityreferences, respond-async, include-annotations=\"display.*\", continue-on-error"
                },
                "GET,ContentType=,ContentLength=,Prefer=allow-entityreferences,include-annotations=\\\"display.*\\\"",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient,allow-entityreferences,include-annotations=\\\"display.*\\\"",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer=allow-entityreferences,include-annotations=\\\"display.*\\\""
            },
            {
                // if batch and individual request define the same preference, then the one from the individual request should be retained
                new []
                {
                   "allow-entityreferences, respond-async, include-annotations=\"display.*\", continue-on-error, wait=200"
                },
                "GET,ContentType=,ContentLength=,Prefer=allow-entityreferences,include-annotations=\\\"display.*\\\",wait=200",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient,allow-entityreferences,include-annotations=\\\"display.*\\\"",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer=allow-entityreferences,include-annotations=\\\"display.*\\\",wait=200"
            },
            {
                // should correctly handle preferences that contain parameters
                new []
                {
                    "allow-entityreferences, respond-async, foo; param=paramValue,include-annotations=\"display.*\", continue-on-error, wait=200"
                },
                "GET,ContentType=,ContentLength=,Prefer=allow-entityreferences,foo; param=paramValue,include-annotations=\\\"display.*\\\",wait=200",
                "DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient,allow-entityreferences,foo; param=paramValue,include-annotations=\\\"display.*\\\"",
                "POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer=allow-entityreferences,foo; param=paramValue,include-annotations=\\\"display.*\\\",wait=200"
            },
            {
                // should correctly parse preferences with commas in their quoted values
                new []
                {
                    @"allow-entityreferences, respond-async, include-annotations=""display.*,foo"", continue-on-error, wait=""200,\""300"""
                },
                @"GET,ContentType=,ContentLength=,Prefer=allow-entityreferences,include-annotations=\""display.*,foo\"",wait=\""200,\\\""300\""",
                @"DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient,allow-entityreferences,include-annotations=\""display.*,foo\""",
                @"POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer=allow-entityreferences,include-annotations=\""display.*,foo\"",wait=\""200,\\\""300\"""
            },
            {
                // should correctly handle batch request with multiple Prefer headers and should not copy duplicate references
                new []
                {
                    @"allow-entityreferences, respond-async, wait=300",
                    @"continue-on-error, wait=250, include-annotations=display"
                },
                @"GET,ContentType=,ContentLength=,Prefer=allow-entityreferences,wait=300,include-annotations=display",
                @"DELETE,ContentType=,ContentLength=,Prefer=wait=100,handling=lenient,allow-entityreferences,include-annotations=display",
                @"POST,ContentType=text/plain; charset=utf-8,ContentLength=3,Prefer=allow-entityreferences,wait=300,include-annotations=display"
            }
        };

        [Theory]
        [MemberData(nameof(_batchHeadersTestData))]
        public async Task SendAsync_CorrectlyCopiesHeadersToIndividualRequests(
            IEnumerable<string> batchPreferHeaderValues,
            string getRequest,
            string deleteRequest,
            string postRequest)
        {
            var batchRef = $"batch_{Guid.NewGuid()}";
            var changesetRef = $"changeset_{Guid.NewGuid()}";
            var endpoint = "http://localhost/odata";
            var acceptJsonFullMetadata = "application/json;odata.metadata=minimal";
            var postPayload = "Bar";

            Type[] controllers = new[] { typeof(BatchTestHeadersCustomersController) };
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var builder = new ODataConventionModelBuilder();
                    builder.EntitySet<BatchTestHeadersCustomer>("BatchTestHeadersCustomers");
                    IEdmModel model = builder.GetEdmModel();
                    services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", model, new DefaultODataBatchHandler()).Expand());
                })
                .Configure(app =>
                {
                    ApplicationPartManager applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    applicationPartManager.ApplicationParts.Clear();

                    if (controllers != null)
                    {
                        AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
                        applicationPartManager.ApplicationParts.Add(part);
                    }

                    app.UseODataBatching();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/$batch");
            batchRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));
            batchRequest.Headers.Add("Prefer", batchPreferHeaderValues);

            var batchContent = $@"
--{batchRef}
Content-Type: application/http
Content-Transfer-Encoding: binary

GET {endpoint}/BatchTestHeadersCustomers HTTP/1.1
OData-Version: 4.0
OData-MaxVersion: 4.0
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8


--{batchRef}
Content-Type: application/http
Content-Transfer-Encoding: binary

DELETE {endpoint}/BatchTestHeadersCustomers(1) HTTP/1.1
OData-Version: 4.0
OData-MaxVersion: 4.0
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
Prefer: wait=100,handling=lenient


--{batchRef}
Content-Type: application/http
Content-Transfer-Encoding: binary

POST {endpoint}/BatchTestHeadersCustomers HTTP/1.1
OData-Version: 4.0;NetFx
OData-MaxVersion: 4.0;NetFx
Content-Type: text/plain; charset=utf-8
Content-Length: {postPayload.Length}
Accept: {acceptJsonFullMetadata}
Accept-Charset: UTF-8

{postPayload}
--{batchRef}--
";

            var httpContent = new StringContent(batchContent);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchRef}");
            httpContent.Headers.ContentLength = batchContent.Length;
            batchRequest.Content = httpContent;
            var response = await client.SendAsync(batchRequest);

            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains(getRequest, responseContent);
            Assert.Contains(deleteRequest, responseContent);
            Assert.Contains(postRequest, responseContent);
        }

        [Fact]
        public async Task SendAsync_CorrectlyHandlesCookieHeader()
        {
            var batchRef = $"batch_{Guid.NewGuid()}";
            var changesetRef = $"changeset_{Guid.NewGuid()}";
            var endpoint = "http://localhost";

            Type[] controllers = new[] { typeof(BatchTestCustomersController), typeof(BatchTestOrdersController) };

            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var builder = new ODataConventionModelBuilder();
                    builder.EntitySet<BatchTestOrder>("BatchTestOrders");
                    IEdmModel model = builder.GetEdmModel();
                    services.AddControllers().AddOData(opt => opt.AddRouteComponents(model, new DefaultODataBatchHandler()).Expand());
                })
                .Configure(app =>
                {
                    ApplicationPartManager applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    applicationPartManager.ApplicationParts.Clear();

                    if (controllers != null)
                    {
                        AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
                        applicationPartManager.ApplicationParts.Add(part);
                    }

                    app.UseODataBatching();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            var server = new TestServer(builder);
            var client = server.CreateClient();

            var orderId = 2;
            var createOrderPayload = $@"{{""@odata.type"":""Microsoft.AspNetCore.OData.Test.Batch.BatchTestOrder"",""Id"":{orderId},""Amount"":50}}";

            var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/$batch");
            batchRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/plain"));

            // Add cookie (for example IdentityServer adds antiforgery after login)
            batchRequest.Headers.TryAddWithoutValidation("Cookie", ".AspNetCore.Antiforgery.9TtSrW0hzOs=" + Guid.NewGuid());

            var batchContent = $@"
--{batchRef}
Content-Type: multipart/mixed;boundary={changesetRef}

--{changesetRef}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST {endpoint}/BatchTestOrders HTTP/1.1
Content-Type: application/json;type=entry
Prefer: return=representation

{createOrderPayload}
--{changesetRef}--
--{batchRef}
Content-Type: application/http
Content-Transfer-Encoding: binary

GET {endpoint}/BatchTestOrders({orderId}) HTTP/1.1
Content-Type: application/json;type=entry
Prefer: return=representation

--{batchRef}--
";

            var httpContent = new StringContent(batchContent);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed;boundary={batchRef}");
            httpContent.Headers.ContentLength = batchContent.Length;
            batchRequest.Content = httpContent;
            var response = await client.SendAsync(batchRequest);

            ExceptionAssert.DoesNotThrow(() => response.EnsureSuccessStatusCode());

            // TODO: assert somehow?
        }

        private static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            services.AddSingleton<ODataMediaTypeResolver>();
            services.AddSingleton<ODataMessageInfo>();

            return services.BuildServiceProvider();
        }
    }

    public class BatchTestCustomer
    {
        private static Lazy<IList<BatchTestCustomer>> _customers =
            new Lazy<IList<BatchTestCustomer>>(() =>
            {
                BatchTestCustomer customer01 = new BatchTestCustomer { Id = 1, Name = "Customer 01" };
                customer01.Orders = new List<BatchTestOrder> { BatchTestOrder.Orders.SingleOrDefault(d => d.Id.Equals(1)) };

                BatchTestCustomer customer02 = new BatchTestCustomer { Id = 2, Name = "Customer 02" };

                return new List<BatchTestCustomer> { customer01, customer02 };
            });

        public static IList<BatchTestCustomer> Customers
        {
            get
            {
                return _customers.Value;
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<BatchTestOrder> Orders { get; set; }
    }

    public class BatchTestOrder
    {
        private static Lazy<IList<BatchTestOrder>> _orders =
            new Lazy<IList<BatchTestOrder>>(() =>
            {
                BatchTestOrder order01 = new BatchTestOrder { Id = 1, Amount = 100 };

                return new List<BatchTestOrder> { order01 };
            });

        public static IList<BatchTestOrder> Orders
        {
            get
            {
                return _orders.Value;
            }
        }

        public int Id { get; set; }
        public decimal Amount { get; set; }
    }

    public class BatchTestCustomersController : ODataController
    {
        [EnableQuery]
        public IEnumerable<BatchTestCustomer> Get()
        {
            return BatchTestCustomer.Customers;
        }

        [EnableQuery]
        public SingleResult<BatchTestCustomer> Get([FromODataUri] int key)
        {
            return SingleResult.Create(BatchTestCustomer.Customers.Where(d => d.Id.Equals(key)).AsQueryable());
        }

        public IActionResult CreateRef([FromODataUri] int key, [FromODataUri] string navigationProperty, [FromBody] Uri link)
        {
            var customer = BatchTestCustomer.Customers.FirstOrDefault(d => d.Id.Equals(key));
            if (customer == null)
                return NotFound();

            switch (navigationProperty)
            {
                case "Orders":
                    var orderId = Request.GetKeyFromLinkUri<int>(link);
                    var order = BatchTestOrder.Orders.FirstOrDefault(d => d.Id.Equals(orderId));

                    if (order == null)
                        return NotFound();

                    if (customer.Orders == null)
                        customer.Orders = new List<BatchTestOrder>();
                    if (customer.Orders.FirstOrDefault(d => d.Id.Equals(orderId)) == null)
                        customer.Orders.Add(order);
                    break;
                default:
                    return BadRequest();
            }

            return NoContent();
        }
    }

    public class BatchTestOrdersController : ODataController
    {
        [EnableQuery]
        public IEnumerable<BatchTestOrder> Get()
        {
            return BatchTestOrder.Orders;
        }

        [EnableQuery]
        public SingleResult<BatchTestOrder> Get([FromODataUri] int key)
        {
            return SingleResult.Create(BatchTestOrder.Orders.Where(d => d.Id.Equals(key)).AsQueryable());
        }

        public IActionResult Post([FromBody] BatchTestOrder order)
        {
            BatchTestOrder.Orders.Add(order);

            return Created(order);
        }
    }

    public class BatchTestHeadersCustomersController : ODataController
    {
        private string GetResponseString(string method)
        {
            return $"{method},ContentType={HttpContext.Request.ContentType},ContentLength={HttpContext.Request.ContentLength},"
                + $"Prefer={HttpContext.Request.Headers["Prefer"]}";
        }
        public string Get()
        {
            return GetResponseString("GET");
        }

        public string Delete(int key)
        {
            return GetResponseString("DELETE");
        }

        public string Post()
        {
            return GetResponseString("POST");
        }
    }

    public class BatchTestHeadersCustomer
    {
        public int Id { get; set; }
    }
}
