//-----------------------------------------------------------------------------
// <copyright file="JsonPropertyNameMapperTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Container
{
    public class JsonPropertyNameMapperTests
    {
        [Fact]
        public void MapProperty_Maps_PropertyName()
        {
            // Arrange
            (IEdmModel model, IEdmStructuredType address) = GetOData();
            JsonPropertyNameMapper mapper = new JsonPropertyNameMapper(model, address);

            // Act & Assert
            Assert.Equal("Road", mapper.MapProperty("Street"));

            // Act & Assert
            Assert.Equal("City", mapper.MapProperty("City"));
        }

        private static (IEdmModel, IEdmStructuredType) GetOData()
        {
            EdmModel model = new EdmModel();
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("City", EdmPrimitiveTypeKind.String);
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            model.SetAnnotationValue(address, new ClrTypeAnnotation(typeof(JAddress)));

            model.SetAnnotationValue(address.FindProperty("Street"),
                new ClrPropertyInfoAnnotation(typeof(JAddress).GetProperty("Street")));

            return (model, address);
        }

        private class JAddress
        {
            public string City { get; set; }

            [JsonPropertyName("Road")]
            public string Street { get; set; }
        }
    }
}
