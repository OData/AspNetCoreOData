//-----------------------------------------------------------------------------
// <copyright file="ODataResourceWrapperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Wrapper
{
    public class ODataResourceWrapperTests
    {
        [Fact]
        public void ODataResourceWrapper_SetsUsingODataResource_CorrectProperties()
        {
            // Arrange & Act & Assert
            ODataResource resource = new ODataResource
            {
                TypeName = "NS.Namespace"
            };

            // Act
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(resource);

            // Assert
            Assert.Same(resource, resourceWrapper.Resource);
            Assert.False(resourceWrapper.IsDeletedResource);
            Assert.Empty(resourceWrapper.NestedPropertyInfos);
            Assert.Empty(resourceWrapper.NestedResourceInfos);
        }

        [Fact]
        public void ODataResourceWrapper_SetsUsingODataDeletedResource_CorrectProperties()
        {
            // Arrange & Act & Assert
            ODataDeletedResource deletedResource = new ODataDeletedResource
            {
                TypeName = "NS.Namespace"
            };

            // Act
            ODataResourceWrapper resourceWrapper = new ODataResourceWrapper(deletedResource);

            // Assert
            Assert.Same(deletedResource, resourceWrapper.Resource);
            Assert.True(resourceWrapper.IsDeletedResource);
            Assert.Empty(resourceWrapper.NestedPropertyInfos);
            Assert.Empty(resourceWrapper.NestedResourceInfos);
        }
    }
}
