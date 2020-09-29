// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Attributes
{
    public class ODataRouteAttributeTests
    {
        [Fact]
        public void Ctor_DoesNotThrowsArgumentNull_Template()
        {
            // Assert & Act & Assert
            ExceptionAssert.DoesNotThrow(() => new ODataRouteAttribute(template: null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("v1{data}")]
        public void Ctor_SetRoutePrefixCorrectly(string prefix)
        {
            // Assert
            ODataRouteAttribute routeTemplate = new ODataRouteAttribute("({key})", prefix);

            // Act & Assert
            Assert.Equal("({key})", routeTemplate.PathTemplate);
            Assert.Equal(prefix, routeTemplate.RoutePrefix);
        }
    }
}
