//-----------------------------------------------------------------------------
// <copyright file="ODataModelAttributeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Attributes;

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
