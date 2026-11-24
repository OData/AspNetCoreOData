//-----------------------------------------------------------------------------
// <copyright file="ODataBatchRequestItemTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchRequestItemTest
    {
        [Fact]
        public async Task SendRequestAsync_Throws_WhenInvokerIsNull()
        {
            // Arrange
            Mock<HttpContext> httpContext = new Mock<HttpContext>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendRequestAsync(null, httpContext.Object, null),
                "handler");
        }

        [Fact]
        public async Task SendRequestAsync_Throws_WhenRequestIsNull()
        {
            // Arrange
            Mock<RequestDelegate> handler = new Mock<RequestDelegate>();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(
                () => ODataBatchRequestItem.SendRequestAsync(handler.Object, null, null),
                "context");
        }

        [Fact]
        public async Task SendRequestAsync_CallsHandler()
        {
            // Arrange
            HttpContext context = HttpContextHelper.Create("Get", "http://example.com");

            RequestDelegate handler = context =>
            {
                context.Response.StatusCode = StatusCodes.Status201Created;
                return Task.CompletedTask;
            };

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, new Dictionary<string, string>());

            // Assert
            Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendMessageAsync_Resolves_Uri_From_ContentId()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            HttpResponseMessage response = new HttpResponseMessage();
            RequestDelegate handler = (c) => Task.CompletedTask;
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>();
            contentIdLocationMappings.Add("1", "http://localhost:12345/odata/Customers(42)");
            Uri unresolvedUri = new Uri("http://localhost:12345/odata/$1/Orders");
            context.Request.CopyAbsoluteUrl(unresolvedUri);

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.Equal("/odata/Customers(42)/Orders", context.Request.Path.ToString());
        }

        [Fact]
        public async Task SendRequestAsync_SkipsContentIdResolution_WhenResolvedUriHasDifferentAuthority()
        {
            // Arrange — Content-ID mapping resolves to a different host.
            // The resolved URI authority must be validated before updating the request URL;
            // a mismatch means the mapping is not applied and the original URL is preserved.
            DefaultHttpContext context = new DefaultHttpContext();
            RequestDelegate handler = (c) => Task.CompletedTask;
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            // Location resolves to a completely different host.
            { "1", "http://other.example.com/odata/Customers(42)" }
        };
            Uri unresolvedUri = new Uri("http://localhost:12345/odata/$1/Orders");
            context.Request.CopyAbsoluteUrl(unresolvedUri);

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert — URL must remain unchanged; authority mismatch causes the mapping to be skipped.
            Assert.Equal("/odata/$1/Orders", context.Request.Path.ToString());
        }

        [Fact]
        public async Task SendRequestAsync_AppliesContentIdResolution_WhenResolvedUriHasSameAuthority()
        {
            // Arrange — normal Content-ID resolution where authority is unchanged.
            DefaultHttpContext context = new DefaultHttpContext();
            RequestDelegate handler = (c) => Task.CompletedTask;
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/odata/Customers(42)" }
        };
            Uri unresolvedUri = new Uri("http://localhost:12345/odata/$1/Orders");
            context.Request.CopyAbsoluteUrl(unresolvedUri);

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert — URL should be updated to the resolved location.
            Assert.Equal("/odata/Customers(42)/Orders", context.Request.Path.ToString());
        }

        [Theory]
        [InlineData("http://localhost:12345/odata", "/odata")]
        [InlineData("http://localhost:12345/odata/Customers(42)", "/odata/Customers(42)")]
        public async Task SendRequestAsync_AppliesContentIdResolution_WhenResolvedPathIsWithinServiceRoot(string location, string expectedPath)
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool handlerCalled = false;
            RequestDelegate handler = (c) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string> { { "1", location } };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/odata/$1"));
            context.ODataFeature().RoutePrefix = "odata";

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.True(handlerCalled);
            Assert.Equal(expectedPath, context.Request.Path.ToString());
        }

        [Theory]
        [InlineData("http://localhost:12345/odata/Customers(42)", "http://localhost:12345/odata/?$1/../../../control/resource")]
        [InlineData("http://localhost:12345/odata-other/Customers(42)", "http://localhost:12345/odata/$1")]
        public async Task SendRequestAsync_ReturnsBadRequestWithoutCallingHandler_WhenResolvedPathIsOutsideServiceRoot(string location, string unresolvedUrl)
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool handlerCalled = false;
            RequestDelegate handler = (c) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string> { { "1", location } };
            context.Request.CopyAbsoluteUrl(new Uri(unresolvedUrl));
            context.ODataFeature().RoutePrefix = "odata";

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.False(handlerCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_UsesPathBaseWhenValidatingResolvedPath()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool handlerCalled = false;
            RequestDelegate handler = (c) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/base/odata/Customers(42)" }
        };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/base/odata/?$1/../../../../control/resource"));
            context.Request.PathBase = new PathString("/base");
            context.ODataFeature().RoutePrefix = "odata";

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.False(handlerCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_UsesPathBaseWhenRoutePrefixIsEmpty()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool handlerCalled = false;
            RequestDelegate handler = (c) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/base/Customers(42)" }
        };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/base/?$1/../../../control/resource"));
            context.Request.PathBase = new PathString("/base");

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.False(handlerCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_RejectsResolvedNonODataEndpointWhenServiceIsRegisteredAtHostRoot()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool endpointCalled = false;
            RequestDelegate endpointHandler = (c) =>
            {
                endpointCalled = true;
                return Task.CompletedTask;
            };
            RequestDelegate handler = async (c) =>
            {
                c.SetEndpoint(new Endpoint(endpointHandler, EndpointMetadataCollection.Empty, "Non-OData endpoint"));
                await c.GetEndpoint().RequestDelegate(c);
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/Customers(42)" }
        };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/?$1/../../control/resource"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.False(endpointCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("/control/resource", context.Request.Path.ToString());
        }

        [Fact]
        public async Task SendRequestAsync_AllowsResolvedODataEndpointWhenServiceIsRegisteredAtHostRoot()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool endpointCalled = false;
            RequestDelegate endpointHandler = (c) =>
            {
                endpointCalled = true;
                return Task.CompletedTask;
            };
            var metadata = new ODataRoutingMetadata(string.Empty, EdmCoreModel.Instance, new ODataPathTemplate());
            RequestDelegate handler = async (c) =>
            {
                c.SetEndpoint(new Endpoint(endpointHandler, new EndpointMetadataCollection(metadata), "OData endpoint"));
                await c.GetEndpoint().RequestDelegate(c);
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/Customers(42)" }
        };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/?$1"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.True(endpointCalled);
            Assert.Equal("/Customers(42)", context.Request.Path.ToString());
        }

        [Fact]
        public async Task SendRequestAsync_RejectsNonODataEndpointWithoutContentIdResolution()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool endpointCalled = false;
            RequestDelegate endpointHandler = (c) =>
            {
                endpointCalled = true;
                return Task.CompletedTask;
            };
            RequestDelegate handler = async (c) =>
            {
                c.SetEndpoint(new Endpoint(endpointHandler, EndpointMetadataCollection.Empty, "Non-OData endpoint"));
                await c.GetEndpoint().RequestDelegate(c);
            };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/control/resource"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, null);

            // Assert
            Assert.False(endpointCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_RejectsResolvedNonODataEndpointWithinConfiguredRoutePrefix()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool endpointCalled = false;
            RequestDelegate endpointHandler = (c) =>
            {
                endpointCalled = true;
                return Task.CompletedTask;
            };
            RequestDelegate handler = async (c) =>
            {
                c.SetEndpoint(new Endpoint(endpointHandler, EndpointMetadataCollection.Empty, "Non-OData endpoint"));
                await c.GetEndpoint().RequestDelegate(c);
            };
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/odata/Customers(42)" }
        };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/odata/?$1/../control/resource"));
            context.ODataFeature().RoutePrefix = "odata";

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.False(endpointCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("/odata/control/resource", context.Request.Path.ToString());
        }

        [Fact]
        public async Task SendRequestAsync_AllowsRoutingToCompleteWithoutSelectingEndpoint()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool handlerCalled = false;
            RequestDelegate handler = (c) =>
            {
                c.SetEndpoint(null);
                handlerCalled = true;
                return Task.CompletedTask;
            };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/unknown"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, null);

            // Assert
            Assert.True(handlerCalled);
            Assert.Null(context.GetEndpoint());
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_CallsHandlerWithOriginalPath_WhenContentIdMappingIsNull()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            string observedPath = null;
            RequestDelegate handler = (c) =>
            {
                observedPath = c.Request.Path;
                return Task.CompletedTask;
            };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/odata/$1"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, null);

            // Assert
            Assert.Equal("/odata/$1", observedPath);
        }

        [Theory]
        [InlineData("http://localhost:12345/odata/$1")]
        [InlineData("http://localhost:12345/odata/%241")]
        public async Task SendRequestAsync_CallsHandlerWithOriginalPath_WhenContentIdCannotBeResolved(string unresolvedUrl)
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            bool handlerCalled = false;
            RequestDelegate handler = (c) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            };
            context.Request.CopyAbsoluteUrl(new Uri(unresolvedUrl));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, new Dictionary<string, string>());

            // Assert
            Assert.True(handlerCalled);
            Assert.NotEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_ResolvesOnlyReferencedContentId_WhenMappingContainsMultipleItems()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            RequestDelegate handler = (c) => Task.CompletedTask;
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/odata/Customers(1)" },
            { "2", "http://localhost:12345/odata/Customers(2)" }
        };
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/odata/$2/Orders"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.Equal("/odata/Customers(2)/Orders", context.Request.Path.ToString());
        }

        [Fact]
        public async Task SendRequestAsync_UsesReAddedContentIdMappingValue()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            RequestDelegate handler = (c) => Task.CompletedTask;
            Dictionary<string, string> contentIdLocationMappings = new Dictionary<string, string>
        {
            { "1", "http://localhost:12345/odata/Customers(1)" }
        };
            contentIdLocationMappings.Remove("1");
            contentIdLocationMappings.Add("1", "http://localhost:12345/odata/Customers(2)");
            context.Request.CopyAbsoluteUrl(new Uri("http://localhost:12345/odata/$1/Orders"));

            // Act
            await ODataBatchRequestItem.SendRequestAsync(handler, context, contentIdLocationMappings);

            // Assert
            Assert.Equal("/odata/Customers(2)/Orders", context.Request.Path.ToString());
        }
    }
}
