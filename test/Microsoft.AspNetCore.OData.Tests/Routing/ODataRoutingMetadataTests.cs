// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing
{
    public class ODataRoutingMetadataTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Prefix()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataRoutingMetadata(prefix: null, null, null), "prefix");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Model()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataRoutingMetadata("prefix", null, null), "model");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_template()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataRoutingMetadata("prefix", EdmCoreModel.Instance, null), "template");
        }

        [Fact]
        public void Ctor_SetPropertiesCorrectly()
        {
            // Arrange
            IEdmModel model = EdmCoreModel.Instance;
            ODataPathTemplate path = new ODataPathTemplate();

            // Act
            ODataRoutingMetadata metadata = new ODataRoutingMetadata("prefix", model, path);

            // Assert
            Assert.Equal("prefix", metadata.Prefix);
            Assert.Same(model, metadata.Model);
            Assert.Same(path, metadata.Template);
        }
    }
}
