// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
using System.Net.Http;
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
            Assert.Null(batchHandler.RouteName);
        }

        [Fact]
        public async Task CreateResponseMessageAsync_Throws_IfResponsesAreNull()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            HttpContext context = new DefaultHttpContext();

            context.ODataFeature().RouteName = "odata";
            context.RequestServices = BuildServiceProvider(opt => opt.AddModel("odata", model));

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => batchHandler.CreateResponseMessageAsync(null, context.Request),
                "responses");
        }

        private static IServiceProvider BuildServiceProvider(Action<ODataOptions> setupAction)
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            services.AddSingleton<IPerRouteContainer, PerRouteContainer>();
            return services.BuildServiceProvider();
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

#if false
        [Fact]
        public async Task ProcessBatchAsync_CallsRegisterForDispose()
        {
            // Arrange
            List<IDisposable> expectedResourcesForDisposal = new List<IDisposable>();
            MockHttpServer server = new MockHttpServer(request =>
            {
                var tmpContent = new StringContent(String.Empty);
                request.RegisterForDispose(tmpContent);
                expectedResourcesForDisposal.Add(tmpContent);
                return new HttpResponseMessage { Content = new StringContent(request.RequestUri.AbsoluteUri) };
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            HttpContext context = HttpContextHelper.Create("Post", "http://example.com/$batch");

            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/"))
                    }
                }
            };
            batchRequest.EnableHttpDependencyInjectionSupport();

            RequestDelegate next = context => Task.FromResult(context.Response);

            // Act
            await batchHandler.ProcessBatchAsync(context, next);
            var resourcesForDisposal = batchRequest.GetResourcesForDisposal();

            // Assert
            foreach (var expectedResource in expectedResourcesForDisposal)
            {
                Assert.Contains(expectedResource, resourcesForDisposal);
            }
        }



        [Fact]
        public async Task ProcessBatchAsync_CallsInvokerForEachRequest()
        {
            // Arrange
            RequestDelegate next = context =>
            {
                HttpRequest request = context.Request;
                string responseContent = request.GetDisplayUrl();
                if (request.Body != null)
                {
                    string content = ReadStream(request.Body);
                    if (!string.IsNullOrEmpty(content))
                    {
                        responseContent += "," + content;
                    }
                }

                byte[] contentBytes = Encoding.UTF8.GetBytes(responseContent);
                context.Response.Body = new MemoryStream(contentBytes);
                return Task.FromResult(context.Response);
            };

            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler();
            HttpContext context = HttpContextHelper.Create("Post", "http://example.com/$batch");

            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                        {
                            Content = new StringContent("foo")
                        })
                    }
                }
            };

            MultipartContent content = new MultipartContent("mixed");
            await content.ReadAsStreamAsync();

            batchRequest.EnableHttpDependencyInjectionSupport();

            // Act
            await batchHandler.ProcessBatchAsync(context, next);

            // Assert
            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            var batchResponses = batchContent.Responses.ToArray();
            Assert.Equal(2, batchResponses.Length);
            var response0 = (OperationResponseItem)batchResponses[0];
            var response1 = (ChangeSetResponseItem)batchResponses[1];
            Assert.Equal("http://example.com/", await response0.Response.Content.ReadAsStringAsync());
            Assert.Equal("http://example.com/values,foo", await response1.Responses.First().Content.ReadAsStringAsync());
        }

        public static HttpMessageContent CreateODataRequestContent(HttpRequest request)
        {
            var changeSetMessageContent = new HttpMessageContent(request);
            changeSetMessageContent.Headers.ContentType.Parameters.Clear();
            changeSetMessageContent.Headers.TryAddWithoutValidation("Content-Transfer-Encoding", "binary");
            changeSetMessageContent.Headers.TryAddWithoutValidation(
                "Content-ID",
                Guid.NewGuid().GetHashCode().ToString(CultureInfo.InvariantCulture));
            return changeSetMessageContent;
        }

        private static string ReadStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public async Task ProcessBatchAsync_DisposesResponseInCaseOfException()
        {
            // Arrange
            List<MockHttpResponseMessage> responses = new List<MockHttpResponseMessage>();
            MockHttpServer server = new MockHttpServer(request =>
            {
                if (request.Method == HttpMethod.Put)
                {
                    throw new InvalidOperationException();
                }
                var response = new MockHttpResponseMessage();
                responses.Add(response);
                return response;
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")),
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Put, "http://example.com/values"))
                    }
                }
            };
            batchRequest.EnableHttpDependencyInjectionSupport();

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None));

            Assert.Equal(2, responses.Count);
            foreach (var response in responses)
            {
                Assert.True(response.IsDisposed);
            }
        }

        [Theory]
        [InlineData(true, "", 2)]
        [InlineData(true, "odata.continue-on-error", 3)]
        [InlineData(true, "odata.continue-on-error=true", 3)]
        [InlineData(true, "odata.continue-on-error=false", 2)]
        [InlineData(true, "continue-on-error", 3)]
        [InlineData(true, "continue-on-error=true", 3)]
        [InlineData(true, "continue-on-error=false", 2)]
        [InlineData(false, "", 3)]
        [InlineData(false, "odata.continue-on-error", 3)]
        [InlineData(false, "odata.continue-on-error=true", 3)]
        [InlineData(false, "odata.continue-on-error=false", 3)]
        [InlineData(false, "continue-on-error", 3)]
        [InlineData(false, "continue-on-error=true", 3)]
        [InlineData(false, "continue-on-error=false", 3)]
        public async Task ProcessBatchAsync_ContinueOnError(bool enableContinueOnError, string preferenceHeader, int expectedResponses)
        {
            // Arrange
            MockHttpServer server = new MockHttpServer(async request =>
            {
                string responseContent = request.RequestUri.AbsoluteUri;
                string content = "";
                if (request.Content != null)
                {
                    content = await request.Content.ReadAsStringAsync();
                    if (!String.IsNullOrEmpty(content))
                    {
                        responseContent += "," + content;
                    }
                }
                HttpResponseMessage responseMessage = new HttpResponseMessage { Content = new StringContent(responseContent) };
                if (content.Equals("foo"))
                {
                    responseMessage.StatusCode = HttpStatusCode.BadRequest;
                }
                return responseMessage;
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                        {
                            Content = new StringContent("foo")
                        })
                    },
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                    {
                        Content = new StringContent("bar")
                    }),
                }
            };
            var enableContinueOnErrorconfig = new HttpConfiguration();
            enableContinueOnErrorconfig.EnableODataDependencyInjectionSupport();
            enableContinueOnErrorconfig.EnableContinueOnErrorHeader();
            if (enableContinueOnError)
                batchRequest.SetConfiguration(enableContinueOnErrorconfig);
            if (!string.IsNullOrEmpty(preferenceHeader))
                batchRequest.Headers.Add("prefer", preferenceHeader);
            batchRequest.EnableHttpDependencyInjectionSupport();

            // Act
            var response = await batchHandler.ProcessBatchAsync(batchRequest, CancellationToken.None);
            var batchContent = Assert.IsType<ODataBatchContent>(response.Content);
            var batchResponses = batchContent.Responses.ToArray();

            // Assert
            Assert.Equal(expectedResponses, batchResponses.Length);
        }

        [Fact]
        public void ValidateRequest_Throws_IfResponsesIsNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
            ExceptionAssert.ThrowsArgumentNull(
                () => batchHandler.ValidateRequest(null),
                "request");
        }

        [Fact]
        public async Task ExecuteChangeSet_Throws_IfReaderIsNull()
        {
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteChangeSetAsync(null, Guid.NewGuid(), new HttpRequestMessage(), CancellationToken.None),
                "batchReader");
        }

        [Fact]
        public async Task ExecuteChangeSet_Throws_IfRequestIsNull()
        {
            var httpContent = new StringContent(String.Empty, Encoding.UTF8, "multipart/mixed");
            httpContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", Guid.NewGuid().ToString()));
            var reader = await httpContent.GetODataMessageReaderAsync(new ODataMessageReaderSettings(), CancellationToken.None);
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(new HttpServer());

            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => batchHandler.ExecuteChangeSetAsync(reader.CreateODataBatchReader(), Guid.NewGuid(), null, CancellationToken.None),
                "originalRequest");
        }

        [Fact]
        public async Task ExecuteChangeSetAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                return new HttpResponseMessage
                {
                    RequestMessage = request
                };
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")
                        {
                            Content = new StringContent("foo")
                        })
                    }
                }
            };
            batchRequest.Properties.Add("foo", "bar");
            batchRequest.SetRouteData(new HttpRouteData(new HttpRoute()));
            batchRequest.RegisterForDispose(new StringContent(String.Empty));
            ODataMessageReader reader = await batchRequest.Content
                .GetODataMessageReaderAsync(new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") }, CancellationToken.None);
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            Guid batchId = Guid.NewGuid();
            await batchReader.ReadAsync();

            var response = await batchHandler.ExecuteChangeSetAsync(batchReader, Guid.NewGuid(), batchRequest, CancellationToken.None);

            var changeSetResponses = ((ChangeSetResponseItem)response).Responses;
            foreach (var changeSetResponse in changeSetResponses)
            {
                var changeSetRequest = changeSetResponse.RequestMessage;
                Assert.Equal("bar", changeSetRequest.Properties["foo"]);
                Assert.Null(changeSetRequest.GetRouteData());
                Assert.Same(changeSetRequest, changeSetRequest.GetUrlHelper().Request);
                Assert.Empty(changeSetRequest.GetResourcesForDisposal());
            }
        }

        [Fact]
        public async Task ExecuteChangeSetAsync_ReturnsSingleErrorResponse()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                }
                return Task.FromResult(new HttpResponseMessage());
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    new MultipartContent("mixed") // ChangeSet
                    {
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Put, "http://example.com/values")),
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Post, "http://example.com/values")),
                        ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Delete, "http://example.com/values")),
                    }
                }
            };
            ODataMessageReader reader = await batchRequest.Content
                .GetODataMessageReaderAsync(new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") }, CancellationToken.None);
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync();

            var response = await batchHandler.ExecuteChangeSetAsync(batchReader, Guid.NewGuid(), batchRequest, CancellationToken.None);

            var changesetResponse = Assert.IsType<ChangeSetResponseItem>(response);
            Assert.Single(changesetResponse.Responses);
            Assert.Equal(HttpStatusCode.BadRequest, changesetResponse.Responses.First().StatusCode);
        }

        [Fact]
        public async Task ExecuteOperationAsync_CopiesPropertiesFromRequest_WithoutExcludedProperties()
        {
            MockHttpServer server = new MockHttpServer(request =>
            {
                return new HttpResponseMessage
                {
                    RequestMessage = request
                };
            });
            UnbufferedODataBatchHandler batchHandler = new UnbufferedODataBatchHandler(server);
            HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com/$batch")
            {
                Content = new MultipartContent("mixed")
                {
                    ODataBatchRequestHelper.CreateODataRequestContent(new HttpRequestMessage(HttpMethod.Get, "http://example.com/")),
                }
            };
            batchRequest.Properties.Add("foo", "bar");
            batchRequest.SetRouteData(new HttpRouteData(new HttpRoute()));
            batchRequest.RegisterForDispose(new StringContent(String.Empty));
            ODataMessageReader reader = await batchRequest.Content
                .GetODataMessageReaderAsync(new ODataMessageReaderSettings { BaseUri = new Uri("http://example.com") }, CancellationToken.None);
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            Guid batchId = Guid.NewGuid();
            await batchReader.ReadAsync();

            var response = await batchHandler.ExecuteOperationAsync(batchReader, Guid.NewGuid(), batchRequest, CancellationToken.None);

            var operationResponse = ((OperationResponseItem)response).Response;
            var operationRequest = operationResponse.RequestMessage;
            Assert.Equal("bar", operationRequest.Properties["foo"]);
            Assert.Null(operationRequest.GetRouteData());
            Assert.Same(operationRequest, operationRequest.GetUrlHelper().Request);
            Assert.Empty(operationRequest.GetResourcesForDisposal());
        }
#endif
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
    }
}
