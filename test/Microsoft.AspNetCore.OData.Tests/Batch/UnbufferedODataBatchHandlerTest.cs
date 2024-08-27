//-----------------------------------------------------------------------------
// <copyright file="UnbufferedODataBatchHandlerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class UnbufferedODataBatchHandlerTest
    {
        [Fact]
        public void Parameter_Constructor()
        {
            // Arrange
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();

            // Act & Assert
            Assert.False(batchHandler.ContinueOnError);
            Assert.NotNull(batchHandler.MessageQuotas);
            Assert.Null(batchHandler.PrefixName);
        }

        [Fact]
        public async Task CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            // Arrange
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            HttpContext context = new DefaultHttpContext();

            context.ODataFeature().RoutePrefix = "odata";
            context.RequestServices = BuildServiceProvider(opt => opt.AddRouteComponents("odata", EdmCoreModel.Instance));

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.CreateResponseMessageAsync(null, context.Request),
                "responses");
        }

        [Fact]
        public async Task ProcessBatchAsync_Throws_IfContextIsNull()
        {
            // Arrange
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            RequestDelegate next = null;

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.ProcessBatchAsync(null, next), "context");
        }

        [Fact]
        public async Task ProcessBatchAsync_CallsInvokerForEachRequest()
        {
            // Arrange
            RequestDelegate next = async context =>
            {
                HttpRequest request = context.Request;
                string responseContent = request.GetDisplayUrl();
                string content = request.ReadBody();
                if (!string.IsNullOrEmpty(content))
                {
                    responseContent += "," + content;
                }

                await context.Response.WriteAsync(responseContent);
            };

            string batchRequest = @"--2d958200-beb1-4437-97c5-9d19f7a1d538
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1301336816

GET / HTTP/1.1
Host: example.com


--2d958200-beb1-4437-97c5-9d19f7a1d538
Content-Type: multipart/mixed; boundary=""6aef4f89-41ba-4da5-b733-9100d2318dfa""

--6aef4f89-41ba-4da5-b733-9100d2318dfa
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1557260198

POST /values HTTP/1.1
Host: example.com
Content-Type: text/plain; charset=utf-8

foo
--6aef4f89-41ba-4da5-b733-9100d2318dfa--

--2d958200-beb1-4437-97c5-9d19f7a1d538--
";


            HttpRequest request = RequestFactory.Create("Post", "http://example.com/$batch", opt => opt.AddRouteComponents("odata", EdmCoreModel.Instance));
            HttpContext httpContext = request.HttpContext;
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            request.Body = new MemoryStream(requestBytes);
            request.ContentType = "multipart/mixed; boundary=\"2d958200-beb1-4437-97c5-9d19f7a1d538\"";
            request.ContentLength = 603;
            httpContext.ODataFeature().RoutePrefix = "odata";
            httpContext.Response.Body = new MemoryStream();

            // Act
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            batchHandler.PrefixName = "odata";
            await batchHandler.ProcessBatchAsync(httpContext, next);

            // Assert
            string responseBody = httpContext.Response.ReadBody();

            Assert.Contains("http://example.com/", responseBody);
            Assert.Contains("http://example.com/values,foo", responseBody);
        }

        [Fact]
        public async Task ProcessBatchAsync_DisposesResponseInCaseOfException()
        {
            // Arrange
            RequestDelegate handler = context =>
            {
                HttpRequest request = context.Request;
                if (request.Method.ToLowerInvariant() == "put")
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
                else if (request.Method.ToLowerInvariant() == "post")
                {
                    context.Response.WriteAsync("POSTED VALUE");
                }
                else
                {
                    context.Response.WriteAsync("OTHER VALUE");
                }

                return Task.CompletedTask;
            };

            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            string batchRequest = @"
--97a4482b-26c6-4551-aed3-85f8b500bbbf
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 794393547

GET / HTTP/1.1
Host: example.com


--97a4482b-26c6-4551-aed3-85f8b500bbbf
Content-Type: multipart/mixed;boundary=""be13321d-3c7b-4126-aa20-958b2c7a87e0""

--be13321d-3c7b-4126-aa20-958b2c7a87e0
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: -583046506

POST /values HTTP/1.1
Host: example.com


--be13321d-3c7b-4126-aa20-958b2c7a87e0
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: -811158921

PUT /values HTTP/1.1
Host: example.com


--be13321d-3c7b-4126-aa20-958b2c7a87e0--

--97a4482b-26c6-4551-aed3-85f8b500bbbf--
";
            HttpRequest request = RequestFactory.Create("Post", "http://example.com/$batch", opt => opt.AddRouteComponents(EdmCoreModel.Instance));
            HttpContext httpContext = request.HttpContext;

            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            request.Body = new MemoryStream(requestBytes);
            request.ContentType = "multipart/mixed;boundary=\"be13321d-3c7b-4126-aa20-958b2c7a87e0\"";
            request.ContentLength = 736;
            httpContext.ODataFeature().RoutePrefix = "";
            httpContext.Response.Body = new MemoryStream();
            batchHandler.PrefixName = "";

            // Act
            await batchHandler.ProcessBatchAsync(httpContext, handler);

            // Assert
            string responseBody = httpContext.Response.ReadBody();
            Assert.Contains("POSTED VALUE", responseBody);
            Assert.Contains("400 Bad Request", responseBody);
            Assert.DoesNotContain("OTHER VALUE", responseBody);
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

            HttpRequest request = RequestFactory.Create("Post", "http://example.com/$batch",
                opt => opt.AddRouteComponents(EdmCoreModel.Instance).EnableContinueOnErrorHeader = enableContinueOnError);
            HttpContext httpContext = request.HttpContext;
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            request.Body = new MemoryStream(requestBytes);
            request.ContentType = "multipart/mixed;boundary=\"d3df74a8-8212-4c2a-b4fb-d713a4ba383e\"";
            request.ContentLength = 827;
            httpContext.ODataFeature().RoutePrefix = "";

            if (preferenceHeader != null)
            {
                httpContext.Request.Headers.Append("prefer", preferenceHeader);
            }

            httpContext.Response.Body = new MemoryStream();
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            batchHandler.PrefixName = "";

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
        public void ValidateRequest_Throws_IfResponsesIsNull()
        {
            // Arrange
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => batchHandler.ValidateRequest(null), "request");
        }

        [Fact]
        public async Task ExecuteChangeSet_Throws_IfReaderIsNull()
        {
            // Arrange
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            HttpContext httpContext = new DefaultHttpContext();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.ExecuteChangeSetAsync(null, Guid.NewGuid(), httpContext.Request, null),
                "batchReader");
        }

        [Fact]
        public async Task ExecuteChangeSet_Throws_IfRequestIsNull()
        {
            // Arrange
            ODataMessageInfo messageInfo = new ODataMessageInfo();
            ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
            Mock<ODataInputContext> inputContext = new Mock<ODataInputContext>(ODataFormat.Batch, messageInfo, settings);
            Mock<ODataBatchReader> batchReader = new Mock<ODataBatchReader>(inputContext.Object, false);
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteChangeSetAsync(batchReader.Object, Guid.NewGuid(), null, null),
                "originalRequest");
        }

        [Fact]
        public async Task ExecuteChangeSetAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            // Arrange
            RequestDelegate handler = context =>
            {
                context.Features[typeof(UnbufferedODataBatchHandlerTest)] = "bar";
                return Task.CompletedTask;
            };

            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();

            string batchRequest = @"--cb7bb9ee-dce2-4c94-bf11-b742b2bc6072
Content-Type: multipart/mixed; boundary=""09f27d33-41ea-4334-8ace-1738bd2793a9""

--09f27d33-41ea-4334-8ace-1738bd2793a9
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1328966982

POST /values HTTP/1.1
Host: example.com
Content-Type: text/plain; charset=utf-8

foo
--09f27d33-41ea-4334-8ace-1738bd2793a9--

--cb7bb9ee-dce2-4c94-bf11-b742b2bc6072--
";
            HttpRequest request = RequestFactory.Create("Post", "http://example.com/$batch", opt => opt.AddRouteComponents(EdmCoreModel.Instance));
            HttpContext httpContext = request.HttpContext;
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            request.Body = new MemoryStream(requestBytes);
            request.ContentType = "multipart/mixed;boundary=\"09f27d33-41ea-4334-8ace-1738bd2793a9\"";
            request.ContentLength = 431;
            httpContext.ODataFeature().RoutePrefix = "";
            IServiceProvider sp = request.GetRouteServices();

            IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(request.Body, request.Headers, sp);
            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") });

            ODataBatchReader batchReader = await oDataMessageReader.CreateODataBatchReaderAsync();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();

            // Act
            var response = await batchHandler.ExecuteChangeSetAsync(batchReader, Guid.NewGuid(), request, handler);

            // Arrange
            ChangeSetResponseItem changeSetResponseItem = Assert.IsType<ChangeSetResponseItem>(response);
            HttpContext changeSetContext = Assert.Single(changeSetResponseItem.Contexts);
            Assert.Equal("bar", changeSetContext.Features[typeof(UnbufferedODataBatchHandlerTest)]);
        }

        [Fact]
        public async Task ExecuteChangeSetAsync_ReturnsSingleErrorResponse()
        {
            // Arrange
            RequestDelegate handler = context =>
            {
                if (context.Request.Method.ToUpperInvariant() == "POST")
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }

                return Task.CompletedTask;
            };

            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            string batchRequest = @"--86aef3eb-4af6-4750-a66d-df65e3f31ab0
Content-Type: multipart/mixed; boundary=""7a61b8c1-a80e-4e6b-bac7-2f65564e3fd6""

--7a61b8c1-a80e-4e6b-bac7-2f65564e3fd6
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: -1233709575

PUT /values HTTP/1.1
Host: example.com


--7a61b8c1-a80e-4e6b-bac7-2f65564e3fd6
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: -1854117385

POST /values HTTP/1.1
Host: example.com


--7a61b8c1-a80e-4e6b-bac7-2f65564e3fd6
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: -1665098746

DELETE /values HTTP/1.1
Host: example.com


--7a61b8c1-a80e-4e6b-bac7-2f65564e3fd6--

--86aef3eb-4af6-4750-a66d-df65e3f31ab0--";

            IEdmModel model = new EdmModel();
            HttpRequest request = RequestFactory.Create("Post", "http://example.com/$batch", opt => opt.AddRouteComponents(model));
            HttpContext httpContext = request.HttpContext;
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            httpContext.Request.Body = new MemoryStream(requestBytes);
            httpContext.Request.ContentType = "multipart/mixed;boundary=\"86aef3eb-4af6-4750-a66d-df65e3f31ab0\"";
            httpContext.Request.ContentLength = 431;
            httpContext.ODataFeature().RoutePrefix = "";
            IServiceProvider sp = request.GetRouteServices();

            IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(httpContext.Request.Body, request.Headers, sp);
            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") });

            ODataBatchReader batchReader = await oDataMessageReader.CreateODataBatchReaderAsync();

            // Act
            var response = await batchHandler.ExecuteChangeSetAsync(batchReader, Guid.NewGuid(), httpContext.Request, handler);

            // Assert
            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            var returnContext = Assert.Single(changesetResponse.Contexts);
            Assert.Equal(StatusCodes.Status400BadRequest, returnContext.Response.StatusCode);
        }

        [Fact]
        public async Task ExecuteOperationAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            // Arrange
            RequestDelegate handler = context =>
            {
                context.Features[typeof(UnbufferedODataBatchHandlerTest)] = "foo";
                return Task.CompletedTask;
            };

            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            string batchRequest = @"--75148e61-e67a-4bb7-ac0f-78fa30d0da30
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 318941389

GET / HTTP/1.1
Host: example.com


--75148e61-e67a-4bb7-ac0f-78fa30d0da30--
";

            HttpRequest request = RequestFactory.Create("Post", "http://example.com/$batch", opt => opt.AddRouteComponents(EdmCoreModel.Instance));
            HttpContext httpContext = request.HttpContext;
            byte[] requestBytes = Encoding.UTF8.GetBytes(batchRequest);
            request.Body = new MemoryStream(requestBytes);
            request.ContentType = "multipart/mixed;boundary=\"75148e61-e67a-4bb7-ac0f-78fa30d0da30\"";
            request.ContentLength = 431;
            httpContext.ODataFeature().RoutePrefix = "";
            IServiceProvider sp = request.GetRouteServices();

            IODataRequestMessage oDataRequestMessage = ODataMessageWrapperHelper.Create(httpContext.Request.Body, request.Headers, sp);
            ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") });

            ODataBatchReader batchReader = await oDataMessageReader.CreateODataBatchReaderAsync();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            await batchReader.ReadAsync();

            // Act
            var response = await batchHandler.ExecuteOperationAsync(batchReader, Guid.NewGuid(), httpContext.Request, handler);

            // Assert
            OperationResponseItem operationResponseItem = Assert.IsType<OperationResponseItem>(response);
            Assert.Equal("foo", operationResponseItem.Context.Features[typeof(UnbufferedODataBatchHandlerTest)]);
        }

        [Fact]
        public async Task ExecuteOperation_Throws_IfReaderIsNull()
        {
            // Arrange
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            Mock<HttpRequest> request = new Mock<HttpRequest>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteOperationAsync(null, Guid.NewGuid(), request.Object, null),
                "batchReader");
        }

        [Fact]
        public async Task ExecuteOperation_Throws_IfRequestIsNull()
        {
            // Arrange
            StringContent httpContent = new StringContent(string.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));

            ODataMessageReader reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteOperationAsync(reader.CreateODataBatchReader(), Guid.NewGuid(), null, null),
                "originalRequest");
        }

        private static IServiceProvider BuildServiceProvider(Action<ODataOptions> setupAction)
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            return services.BuildServiceProvider();
        }
    }
}
