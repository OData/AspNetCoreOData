// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Test.Batch
{
    public class ODataBatchMiddlewareTest
    {
        [Fact]
        public void Ctor_NullBatchMapping_WithoutServiceProvider()
        {
            // Arrange
            ODataBatchMiddleware middleware = new ODataBatchMiddleware(null, null);

            // Act & Assert
            Assert.Null(middleware.BatchMapping);
        }

        [Fact]
        public void Ctor_NullBatchMapping_WithServiceProvider_WithoutBatchHandler()
        {
            // Arrange
            IServiceProvider sp = BuildServiceProvider(opt => opt.AddModel("odata", EdmCoreModel.Instance));
            ODataBatchMiddleware middleware = new ODataBatchMiddleware(sp, null);

            // Act & Assert
            Assert.Null(middleware.BatchMapping);
        }

        [Fact]
        public void Ctor_NullBatchMapping_WithServiceProvider_WithBatchHandler()
        {
            // Arrange
            IServiceProvider sp = BuildServiceProvider(opt => opt.AddModel("odata", EdmCoreModel.Instance, new DefaultODataBatchHandler()));
            ODataBatchMiddleware middleware = new ODataBatchMiddleware(sp, null);

            // Act & Assert
            Assert.NotNull(middleware.BatchMapping);
        }

        [Fact]
        public async Task Invoke_CallNextDelegate_WithoutBatchHandler()
        {
            // Arrange
            bool called = false;
            RequestDelegate next = context =>
            {
                called = true;
                return Task.CompletedTask;
            };

            ODataBatchMiddleware middleware = new ODataBatchMiddleware(null, next.Invoke);
            HttpContext context = new DefaultHttpContext();

            // Act
            Assert.False(called);
            await middleware.Invoke(context);

            // Assert
            Assert.True(called);
        }

        [Fact]
        public async Task Invoke_CallProcessBatchAsync_WithBatchHandler()
        {
            // Arrange
            bool called = false;
            RequestDelegate next = context =>
            {
                called = true;
                return Task.CompletedTask;
            };
            Mock<ODataBatchHandler> batchHandlerMock = new Mock<ODataBatchHandler>();

            bool processed = false;
            batchHandlerMock.Setup(b => b.ProcessBatchAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
                .Returns(() =>
                {
                    processed = true;
                    return Task.CompletedTask;
                });

            IServiceProvider sp = BuildServiceProvider(opt => opt.AddModel("odata", EdmCoreModel.Instance, batchHandlerMock.Object));
            ODataBatchMiddleware middleware = new ODataBatchMiddleware(sp, next.Invoke);
            HttpContext context = new DefaultHttpContext();
            context.Request.Path = new PathString("/odata/$batch");

            // Act
            Assert.False(called);
            Assert.False(processed);
            await middleware.Invoke(context);

            // Assert
            Assert.False(called);
            Assert.True(processed);
        }

        [Fact]
        public async Task Invoke_CorsCallNextDelegateWithBatchHandler()
        {
            // Arrange
            bool called = false;
            RequestDelegate next = context =>
            {
                called = true;
                return Task.CompletedTask;
            };
            Mock<ODataBatchHandler> batchHandlerMock = new Mock<ODataBatchHandler>();

            bool processed = false;
            batchHandlerMock.Setup(b => b.ProcessBatchAsync(It.IsAny<HttpContext>(), It.IsAny<RequestDelegate>()))
                .Returns(() =>
                {
                    processed = true;
                    return Task.CompletedTask;
                });

            IServiceProvider sp = BuildServiceProvider(opt => opt.AddModel("odata", EdmCoreModel.Instance, batchHandlerMock.Object));
            ODataBatchMiddleware middleware = new ODataBatchMiddleware(sp, next.Invoke);
            HttpContext context = new DefaultHttpContext();
            context.Request.Path = new PathString("/odata/$batch");
            context.Request.Method = "options";

            // Act
            Assert.False(called);
            Assert.False(processed);
            await middleware.Invoke(context);

            // Assert
            Assert.True(called);
            Assert.False(processed);
        }

        private static IServiceProvider BuildServiceProvider(Action<ODataOptions> setupAction)
        {
            IServiceCollection services = new ServiceCollection();
            services.Configure(setupAction);
            return services.BuildServiceProvider();
        }
    }
}
