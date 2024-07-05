//-----------------------------------------------------------------------------
// <copyright file="ODataOutFormatterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Attributes;
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
        public void GetSupportedContentTypesODataOutputFormatter_WorksForContentType()
        {
            // Arrange
            ODataOutputFormatter formatter = ODataOutputFormatterFactory.Create().First();
            Assert.NotNull(formatter); // guard

            // Act & Assert
            IReadOnlyList<string> contentTypes = formatter.GetSupportedContentTypes("application/json", typeof(string));
            Assert.Equal(36, contentTypes.Count);

            // Act & Assert
            formatter.SupportedMediaTypes.Clear();
            ExceptionAssert.DoesNotThrow(() => formatter.GetSupportedContentTypes("application/json", typeof(string)));
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
                "The OData formatter requires an attached request in order to serialize.");
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
            HttpRequest request = RequestFactory.Create(opt => opt.AddRouteComponents("odata", model));
            request.ODataFeature().RoutePrefix = "odata";
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

        [Fact]
        public void SerializeIllegalUnannotatedObject_ThrowsInvalidOperationException()
        {
            // Arrange
            var illegalObject = new IllegalUnannotatedObject
            {
                DynamicProperties = new Dictionary<string, object>
                {
                    { "Inv@l:d.", 1 }
                }
            };

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<IllegalUnannotatedObject>("IllegalUnannotatedObjects");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("IllegalUnannotatedObjects");
            EntitySetSegment entitySetSeg = new EntitySetSegment(entitySet);
            HttpRequest request = RequestFactory.Create(opt => opt.AddRouteComponents("odata", model));
            request.ODataFeature().RoutePrefix = "odata";
            request.ODataFeature().Model = model;
            request.ODataFeature().Path = new ODataPath(entitySetSeg);

            OutputFormatterWriteContext context = new OutputFormatterWriteContext(
                request.HttpContext,
                (s, e) => null,
                objectType: typeof(IllegalUnannotatedObject),
                @object: illegalObject);

            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            formatter.SupportedMediaTypes.Add("application/json");

            // Act & Assert
            Assert.Throws<ODataException>(() => formatter.WriteResponseBodyAsync(context, Encoding.UTF8).GetAwaiter().GetResult());
        }

        // positive test as above
        [Fact]
        public void SerializeIllegalAnnotatedObject_ReturnsFixedValidObject()
        {
            // Arrange
            var illegalObject = new IllegalAnnotatedObject
            {
                DynamicProperties = new Dictionary<string, object>
                {
                    { "Inv@l:d.", 1 }
                }
            };

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<IllegalAnnotatedObject>("IllegalAnnotatedObject");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet("IllegalAnnotatedObject");
            EntitySetSegment entitySetSeg = new EntitySetSegment(entitySet);
            HttpRequest request = RequestFactory.Create(opt => opt.AddRouteComponents("odata", model));
            request.ODataFeature().RoutePrefix = "odata";
            request.ODataFeature().Model = model;
            request.ODataFeature().Path = new ODataPath(entitySetSeg);

            OutputFormatterWriteContext context = new OutputFormatterWriteContext(
                request.HttpContext,
                (s, e) => null,
                objectType: typeof(IllegalAnnotatedObject),
                @object: illegalObject);

            ODataOutputFormatter formatter = new ODataOutputFormatter(new[] { ODataPayloadKind.Resource });
            formatter.SupportedMediaTypes.Add("application/json");

            // Set the Response.Body to a new MemoryStream to capture the response
            var memoryStream = new MemoryStream();
            context.HttpContext.Response.Body = memoryStream;

            // Act
            formatter.WriteResponseBodyAsync(context, Encoding.UTF8).GetAwaiter().GetResult();

            memoryStream.Position = 0;
            var content = new StreamReader(memoryStream).ReadToEnd();
            var jd = System.Text.Json.JsonDocument.Parse(content);
            var root = jd.RootElement;

            // Assert
            // check that the JSON response contains the fixed property name and its value is 1
            Assert.Equal(1, root.GetProperty("Inv_l_d_").GetInt32());
        }

        private class Customer
        {
            public int Id { get; set; }
        }

        private class IllegalUnannotatedObject
        {
            [Key]
            public int Id { get; set; }
            public IDictionary<string, object> DynamicProperties { get; set; }
        }

        [ReplaceIllegalFieldNameCharacters]
        private class IllegalAnnotatedObject : IllegalUnannotatedObject
        {

        }

    }
}
