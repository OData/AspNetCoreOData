// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Attributes
{
    public class ODataRoutePrefixAttributeTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Template()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new ODataRoutePrefixAttribute(template: null), "template");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("v1{data}")]
        public void Ctor_SetRoutePrefixCorrectly(string prefix)
        {
            // Assert
            ODataRoutePrefixAttribute routePrefixTemplate = new ODataRoutePrefixAttribute("Customers", prefix);

            // Act & Assert
            Assert.Equal("Customers", routePrefixTemplate.PathPrefixTemplate);
            Assert.Equal(prefix, routePrefixTemplate.RoutePrefix);
        }
    }
}
