//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerHelperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataSerializerHelperTests
    {
        [Fact]
        public void AppendInstanceAnnotations_AddNullAnnotationsIntoDestination()
        {
            // Arrange
            IDictionary<string, object> annotations = new Dictionary<string, object>
            {
                { "NS.Test1", null },
                { "NS.Test2", new ODataNullValue() },
            };

            ICollection<ODataInstanceAnnotation> destination = new Collection<ODataInstanceAnnotation>();
            ODataSerializerContext context = new ODataSerializerContext();
            IODataSerializerProvider provider = new Mock<IODataSerializerProvider>().Object;

            // Act
            ODataSerializerHelper.AppendInstanceAnnotations(annotations, destination, context, provider);

            // Assert
            Assert.Equal(2, destination.Count);
            Assert.Collection(destination,
                e =>
                {
                    Assert.Equal("NS.Test1", e.Name);
                    Assert.Same(ODataNullValueExtensions.NullValue, e.Value);
                },
                e =>
                {
                    Assert.Equal("NS.Test2", e.Name);
                    Assert.Same(ODataNullValueExtensions.NullValue, e.Value);
                });
        }

        [Fact]
        public void AppendInstanceAnnotations_AddInstanceAnnotationsIntoDestination()
        {
            // Arrange
            Mock<IODataEdmTypeSerializer> serializer = new Mock<IODataEdmTypeSerializer>();

            Mock<IODataSerializerProvider> serializerProvider = new Mock<IODataSerializerProvider>();
            serializerProvider.Setup(c => c.GetEdmTypeSerializer(It.IsAny<IEdmTypeReference>())).Returns(serializer.Object);

            Mock<IEdmTypeReference> type1 = new Mock<IEdmTypeReference>();
            Mock<IEdmObject> value1 = new Mock<IEdmObject>();
            value1.Setup(c => c.GetEdmType()).Returns(type1.Object);

            Mock<IEdmTypeReference> type2 = new Mock<IEdmTypeReference>();
            Mock<IEdmObject> value2 = new Mock<IEdmObject>();
            value2.Setup(c => c.GetEdmType()).Returns(type2.Object);

            IDictionary<string, object> annotations = new Dictionary<string, object>
            {
                { "NS.Test1", value1.Object },
                { "NS.Test2", value2.Object },
            };

            ODataSerializerContext context = new ODataSerializerContext();
            ODataPrimitiveValue oValue1 = new ODataPrimitiveValue(42);
            ODataPrimitiveValue oValue2 = new ODataPrimitiveValue(43);

            serializer.Setup(s => s.CreateODataValue(value1.Object, type1.Object, context)).Returns(oValue1);
            serializer.Setup(s => s.CreateODataValue(value2.Object, type2.Object, context)).Returns(oValue2);

            ICollection<ODataInstanceAnnotation> destination = new Collection<ODataInstanceAnnotation>();

            // Act
            ODataSerializerHelper.AppendInstanceAnnotations(annotations, destination, context, serializerProvider.Object);

            // Assert
            Assert.Equal(2, destination.Count);
            Assert.Collection(destination,
                e =>
                {
                    Assert.Equal("NS.Test1", e.Name);
                    Assert.Same(oValue1, e.Value);
                },
                e =>
                {
                    Assert.Equal("NS.Test2", e.Name);
                    Assert.Same(oValue2, e.Value);
                });
        }
    }
}
