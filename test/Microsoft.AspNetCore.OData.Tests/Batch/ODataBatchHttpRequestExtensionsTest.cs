// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchHttpRequestExtensionsTest
    {
        [Fact]
        public void GetODataBatchId_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.GetODataBatchId(null), "request");
        }

        [Fact]
        public void SetODataBatchId_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.SetODataBatchId(null, Guid.NewGuid()), "request");
        }

        [Fact]
        public void SetODataBatchId_SetsTheBatchIdOnTheRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;
            Guid id = Guid.NewGuid();

            // Act
            request.SetODataBatchId(id);

            // Assert
            Assert.Equal(id, request.GetODataBatchId());
        }

        [Fact]
        public void GetODataChangeSetId_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.GetODataChangeSetId(null), "request");
        }

        [Fact]
        public void SetODataChangeSetId_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.SetODataChangeSetId(null, Guid.NewGuid()), "request");
        }

        [Fact]
        public void SetODataChangeSetId_SetsTheChangeSetIdOnTheRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;
            Guid id = Guid.NewGuid();

            // Act
            request.SetODataChangeSetId(id);

            // Assert
            Assert.Equal(id, request.GetODataChangeSetId());
        }

        [Fact]
        public void GetODataContentIdMapping_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.GetODataContentIdMapping(null), "request");
        }

        [Fact]
        public void SetODataContentIdMapping_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => ODataBatchHttpRequestExtensions.SetODataContentIdMapping(null, new Dictionary<string, string>()),
                "request");
        }

        [Fact]
        public void SetODataContentIdMapping_SetsTheContentIdMappingOnTheRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;
            IDictionary<string, string> mapping = new Dictionary<string, string>();

            // Act
            request.SetODataContentIdMapping(mapping);

            // Assert
            Assert.Equal(mapping, ODataBatchHttpRequestExtensions.GetODataContentIdMapping(request));
        }

        [Fact]
        public async Task CreateODataBatchResponseAsync_ReturnsHttpStatusCodeOK()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            context.ODataFeature().RoutePrefix = "odata";
            context.RequestServices = BuildServiceProvider(opt => opt.AddRouteComponents("odata", model));

            ODataBatchResponseItem[] responses = new ODataBatchResponseItem[] { };
            ODataMessageQuotas quotas = new ODataMessageQuotas();

            // Act
            await request.CreateODataBatchResponseAsync(responses, quotas);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        }

        [Fact]
        public void GetODataContentId_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.GetODataContentId(null), "request");
        }

        [Fact]
        public void SetODataContentId_NullRequest_Throws()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataBatchHttpRequestExtensions.SetODataContentId(null, Guid.NewGuid().ToString()), "request");
        }

        [Fact]
        public void SetODataContentId_SetsTheContentIdOnTheRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;
            string id = Guid.NewGuid().ToString();

            // Act
            request.SetODataContentId(id);

            // Assert
            Assert.Equal(id, request.GetODataContentId());
        }

        private static ODataMessageQuotas _odataMessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue };

        [Theory]
        // if no accept header, return multipart/mixed
        [InlineData(null, "multipart/mixed")]

        // if accept is multipart/mixed, return multipart/mixed
        [InlineData(new[] { "multipart/mixed" }, "multipart/mixed")]

        // if accept is application/json, return application/json
        [InlineData(new[] { "application/json" }, "application/json")]

        // if accept is application/json with charset, return application/json
        [InlineData(new[] { "application/json; charset=utf-8" }, "application/json")]

        // if multipart/mixed is high proprity, return multipart/mixed
        [InlineData(new[] { "multipart/mixed;q=0.9", "application/json;q=0.5" }, "multipart/mixed")]
        [InlineData(new[] { "application/json;q=0.5", "multipart/mixed;q=0.9" }, "multipart/mixed")]

        // if application/json is high proprity, return application/json
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed;q=0.5" }, "application/json")]
        [InlineData(new[] { "multipart/mixed;q=0.5", "application/json;q=0.9" }, "application/json")]

        // if priorities are same, return first
        [InlineData(new[] { "multipart/mixed;q=0.9", "application/json;q=0.9" }, "multipart/mixed")]
        [InlineData(new[] { "multipart/mixed", "application/json" }, "multipart/mixed")]

        // if priorities are same, return first
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed;q=0.9" }, "application/json")]
        [InlineData(new[] { "application/json", "multipart/mixed" }, "application/json")]

        // no priority has q=1.0
        [InlineData(new[] { "application/json", "multipart/mixed;q=0.9" }, "application/json")]
        [InlineData(new[] { "application/json;q=0.9", "multipart/mixed" }, "multipart/mixed")]
        public async Task CreateODataBatchResponseAsync(string[] accept, string expected)
        {
            var request = RequestFactory.Create("Get", "http://localhost/$batch", opt => opt.AddRouteComponents(EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = string.Empty;
            var responses = new[] { new ChangeSetResponseItem(Enumerable.Empty<HttpContext>()) };

            if (accept != null)
            {
                request.Headers.Add("Accept", accept);
            }

            await request.CreateODataBatchResponseAsync(responses, _odataMessageQuotas);

            Assert.StartsWith(expected, request.HttpContext.Response.ContentType);
        }

        [Theory]
        // if no contentType, return multipart/mixed
        [InlineData(null, "multipart/mixed")]
        // if contentType is application/json, return application/json
        [InlineData("application/json", "application/json")]
        [InlineData("application/json; charset=utf-8", "application/json")]
        // if contentType is multipart/mixed, return multipart/mixed
        [InlineData("multipart/mixed", "multipart/mixed")]
        public async Task CreateODataBatchResponseAsyncWhenNoAcceptHeader(string contentType, string expected)
        {
            var request = RequestFactory.Create("Get", "http://localhost/$batch", opt => opt.AddRouteComponents(EdmCoreModel.Instance));
            request.ODataFeature().RoutePrefix = string.Empty;
            var responses = new[] { new ChangeSetResponseItem(Enumerable.Empty<HttpContext>()) };

            if (contentType != null)
            {
                request.ContentType = contentType;
            }

            await request.CreateODataBatchResponseAsync(responses, _odataMessageQuotas);

            Assert.False(request.Headers.ContainsKey("Accept")); // check no accept header
            Assert.StartsWith(expected, request.HttpContext.Response.ContentType);
        }

        private static IServiceProvider BuildServiceProvider(Action<ODataOptions> setupAction)
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            return services.BuildServiceProvider();
        }
    }
}