// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Attributes
{
    public class ODataModelAttributeTests
    {
        [Fact]
        public void CtorODataModelAttribute_SetsModel()
        {
            // Assert & Act & Assert
            ODataRouteComponentAttribute odataModel = new ODataRouteComponentAttribute();
            Assert.Equal(string.Empty, odataModel.RoutePrefix);

            // Assert & Act & Assert
            odataModel = new ODataRouteComponentAttribute("odata");
            Assert.Equal("odata", odataModel.RoutePrefix);
        }

        [Fact]
        public void CtorODataModelAttribute_ThrowsArgumentNull_Model()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataRouteComponentAttribute(routePrefix: null), "routePrefix");
        }
    }
}
