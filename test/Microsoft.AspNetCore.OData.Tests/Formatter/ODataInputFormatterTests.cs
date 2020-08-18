// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataInputFormatterTests
    {
        private static IServiceProvider _serviceProvider = BuildServiceProvider();
        private static IEdmModel _edmModel = GetEdmModel();

        [Fact]
        public void ConstructorThrowsIfPayloadsIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("payloadKinds", () => new ODataInputFormatter(payloadKinds: null));
        }

        [Fact]
        public void CanReadThrowsIfContextIsNull()
        {
            // Arrange & Act
            ODataInputFormatter formatter = new ODataInputFormatter(new[] { ODataPayloadKind.Resource });

            // Assert
            Assert.Throws<ArgumentNullException>("context", () => formatter.CanRead(context: null));
        }

        [Fact]
        public void CanReadReturnsFalseIfNoODataPathSet()
        {
            // Arrange & Act
            InputFormatterContext context = new InputFormatterContext(
                new DefaultHttpContext(),
                "modelName",
                new ModelStateDictionary(),
                new EmptyModelMetadataProvider().GetMetadataForType(typeof(int)),
                (stream, encoding) => new StreamReader(stream, encoding));

            ODataInputFormatter formatter = new ODataInputFormatter(new[] { ODataPayloadKind.Resource });

            // Assert
            Assert.False(formatter.CanRead(context));
        }

        [Theory]
        [InlineData(typeof(Customer), ODataPayloadKind.Resource, true)]
        [InlineData(typeof(Customer), ODataPayloadKind.Property, false)]
        [InlineData(typeof(Customer[]), ODataPayloadKind.ResourceSet, true)]
        [InlineData(typeof(int), ODataPayloadKind.Property, true)]
        [InlineData(typeof(IList<double>), ODataPayloadKind.Collection, true)]
        [InlineData(typeof(ODataActionParameters), ODataPayloadKind.Parameter, true)]
        [InlineData(typeof(Delta<Customer>), ODataPayloadKind.Resource, true)]
        [InlineData(typeof(IEdmEntityObject), ODataPayloadKind.Resource, true)]
        public void CanReadReturnsBooleanValueAsExpected(Type type, ODataPayloadKind payloadKind, bool expected)
        {
            // Arrange & Act
            IEdmEntitySet entitySet = _edmModel.EntityContainer.FindEntitySet("Customers");
            EntitySetSegment entitySetSeg = new EntitySetSegment(entitySet);
            HttpRequest request = RequestFactory.Create(f => { f.Model = _edmModel; f.Path = new ODataPath(entitySetSeg); });

            InputFormatterContext context = CreateInputContext(type, request);

            ODataInputFormatter formatter = new ODataInputFormatter(new[] { payloadKind });

            // Assert
            Assert.Equal(expected, formatter.CanRead(context));
        }

        private static InputFormatterContext CreateInputContext(Type type, HttpRequest request)
        {
            request.HttpContext.RequestServices = _serviceProvider;
            return new InputFormatterContext(
                request.HttpContext,
                "modelName",
                new ModelStateDictionary(),
                new EmptyModelMetadataProvider().GetMetadataForType(type),
                (stream, encoding) => new StreamReader(stream, encoding));
        }

        private static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ODataDeserializerProvider, DefaultODataDeserializerProvider>();

            // Deserializers.
            services.AddSingleton<ODataResourceDeserializer>();
            services.AddSingleton<ODataEnumDeserializer>();
            services.AddSingleton<ODataPrimitiveDeserializer>();
            services.AddSingleton<ODataResourceSetDeserializer>();
            services.AddSingleton<ODataCollectionDeserializer>();
            services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
            services.AddSingleton<ODataActionPayloadDeserializer>();
            return services.BuildServiceProvider();
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<Customer>("Customers");
            model.ComplexType<Address>();
            return model.GetEdmModel();
        }

        private class Customer
        {
            public int Id { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
        }
    }

}