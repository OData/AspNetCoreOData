// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataOutputFormatterTests
    {
        [Fact]
        public void CtorODataOutputFormatter_ThrowsArgumentNull_IfPayloadsIsNull()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataOutputFormatter(payloadKinds: null), "payloadKinds");
        }

        [Fact]
        public void CanWriteResultODataOutputFormatter_ThrowsArgumentNull_IfContextIsNull()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => formatter.CanWriteResult(context: null), "context");
        }

        [Fact]
        public void CanWriteResultODataOutputFormatter_Throws_IfRequestIsNull()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns((HttpRequest)null);
            OutputFormatterWriteContext context = new OutputFormatterWriteContext(httpContext.Object,
                (s, e) => null,
                typeof(int),
                6);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => formatter.CanWriteResult(context),
                "The OData formatter requires an attached request in order to deserialize.");
        }

        [Fact]
        public void CanWriteResultODataOutputFormatter_ReturnsFalseIfNoODataPathSet()
        {
            // Arrange & Act
            OutputFormatterWriteContext context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => null,
                typeof(int),
                6);

            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });

            // Assert
            Assert.False(formatter.CanWriteResult(context));
        }

        [Theory]
        [InlineData(typeof(Customer), ODataPayloadKind.Resource, true)]
        [InlineData(typeof(Customer), ODataPayloadKind.Property, false)]
        [InlineData(typeof(Customer[]), ODataPayloadKind.ResourceSet, true)]
        [InlineData(typeof(int), ODataPayloadKind.Property, true)]
        [InlineData(typeof(IList<double>), ODataPayloadKind.Collection, true)]
        [InlineData(typeof(IEdmEntityObject), ODataPayloadKind.Resource, true)]
        public void CanWriteResultODataOutputFormatter_ReturnsBooleanValueAsExpected(Type type, ODataPayloadKind payloadKind, bool expected)
        {
            // Arrange & Act
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("Customers");
            EntitySetSegment entitySetSeg = new EntitySetSegment(entitySet);
            HttpRequest request = RequestFactory.Create(opt => opt.AddModel("odata", model));
            request.ODataFeature().PrefixName = "odata";
            request.ODataFeature().Model = model;
            request.ODataFeature().Path = new ODataPath(entitySetSeg);

            OutputFormatterWriteContext context = new OutputFormatterWriteContext(
                request.HttpContext,
                (s, e) => null,
                objectType: type,
                @object: null);

            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { payloadKind });
            formatter.SupportedMediaTypes.Add("application/json");

            // Assert
            Assert.Equal(expected, formatter.CanWriteResult(context));
        }

        #region WriteResponseHeaders
        [Fact]
        public void WriteResponseHeadersODataOutputFormatter_ThrowsArgumentNull_Context()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => formatter.WriteResponseHeaders(context: null), "context");
        }

        [Fact]
        public void WriteResponseHeadersODataOutputFormatter_ThrowsArgumentNull_ObjectType()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            OutputFormatterWriteContext context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => null,
                null,
                null);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => formatter.WriteResponseHeaders(context), "type");
        }

        [Fact]
        public void WriteResponseHeadersODataOutputFormatter_Throws_IfRequestIsNull()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns((HttpRequest)null);
            OutputFormatterWriteContext context = new OutputFormatterWriteContext(httpContext.Object,
                (s, e) => null,
                typeof(int),
                6);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => formatter.WriteResponseHeaders(context),
                "The OData formatter requires an attached request in order to serialize.");
        }
        #endregion

        #region WriteResponseBodyAsync
        [Fact]
        public void WriteResponseBodyAsyncODataOutputFormatter_ThrowsArgumentNull_Context()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            Encoding encoding = Encoding.UTF8;

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => formatter.WriteResponseBodyAsync(context: null, encoding), "context");
        }

        [Fact]
        public void WriteResponseBodyAsyncODataOutputFormatter_ThrowsArgumentNull_ObjectType()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            OutputFormatterWriteContext context = new OutputFormatterWriteContext(
                new DefaultHttpContext(),
                (s, e) => null,
                null,
                null);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => formatter.WriteResponseBodyAsync(context, Encoding.UTF8), "type");
        }

        [Fact]
        public void WriteResponseBodyAsyncODataOutputFormatter_Throws_IfRequestIsNull()
        {
            // Arrange & Act
            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.Request).Returns((HttpRequest)null);
            OutputFormatterWriteContext context = new OutputFormatterWriteContext(httpContext.Object,
                (s, e) => null,
                typeof(int),
                6);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => formatter.WriteResponseBodyAsync(context, Encoding.UTF8),
                "The OData formatter requires an attached request in order to serialize.");
        }
        #endregion

        [Fact]
        public void GetDefaultBaseAddressODataOutputFormatter_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataOutputFormatter.GetDefaultBaseAddress(null), "request");
        }

        [Fact]
        public void TryGetContentHeaderODataOutputFormatter_ThrowsArgumentNull_Type()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => ODataOutputFormatter.TryGetContentHeader(null, null, out _), "type");
        }

        private class Customer
        {
            public int Id { get; set; }
        }
    }
}