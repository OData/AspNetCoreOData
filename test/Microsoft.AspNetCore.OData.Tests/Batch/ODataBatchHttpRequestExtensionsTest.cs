// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
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
            context.ODataFeature().PrefixName = "odata";
            context.RequestServices = BuildServiceProvider(opt => opt.AddModel("odata", model));

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

        private static IServiceProvider BuildServiceProvider(Action<ODataOptions> setupAction)
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            services.AddSingleton<IPerRouteContainer, PerRouteContainer>();
            return services.BuildServiceProvider();
        }
    }
}