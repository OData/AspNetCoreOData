// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataPathExtensionsTests
    {
        [Fact]
        public void IsStreamPropertyPath_Returns_CollectBooleanValue()
        {
            // Arrange
            ODataPath path = null;

            // Act & Assert
            Assert.False(path.IsStreamPropertyPath());

            // Act & Assert
            path = new ODataPath(MetadataSegment.Instance);
            Assert.False(path.IsStreamPropertyPath());

            // Act & Assert
            IEdmTypeReference typeRef = EdmCoreModel.Instance.GetStream(false);
            Mock<IEdmStructuralProperty> mock = new Mock<IEdmStructuralProperty>();
            mock.Setup(s => s.Name).Returns("any");
            mock.Setup(s => s.Type).Returns(typeRef);
            PropertySegment segment = new PropertySegment(mock.Object);

            path = new ODataPath(segment);
            Assert.True(path.IsStreamPropertyPath());
        }

        [Fact]
        public void GetEdmType_ThrowsArgumentNull_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = null;
            ExceptionAssert.ThrowsArgumentNull(() => path.GetEdmType(), "path");
        }

        [Fact]
        public void GetNavigationSource_ThrowsArgumentNull_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = null;
            ExceptionAssert.ThrowsArgumentNull(() => path.GetNavigationSource(), "path");
        }

        [Fact]
        public void GetPathString_ThrowsArgumentNull_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = null;
            ExceptionAssert.ThrowsArgumentNull(() => path.GetPathString(), "path");

            // Arrange & Act & Assert
            IList<ODataPathSegment> segments = null;
            ExceptionAssert.ThrowsArgumentNull(() => segments.GetPathString(), "segments");
        }

        [Fact]
        public void GetPathString_Returns_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = new ODataPath(MetadataSegment.Instance);
            Assert.Equal("$metadata", path.GetPathString());

            // Arrange & Act & Assert
            IList<ODataPathSegment> segments = new List<ODataPathSegment>
            {
                MetadataSegment.Instance
            };
            Assert.Equal("$metadata", segments.GetPathString());
        }
    }
}
