//-----------------------------------------------------------------------------
// <copyright file="OperationRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Routing.Conventions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions;

public class OperationRoutingConventionTests
{
    [Theory]
    [InlineData("", "", null, false)]
    [InlineData("IsUpdate", "IsUpdate", null, false)]
    [InlineData("IsUpdateOnVipCustomer", "IsUpdate", "VipCustomer", false)]
    [InlineData("IsUpdateOnCollectionOfVipCustomer", "IsUpdate", "VipCustomer", true)]
    [InlineData("GetMostProfitableOnCollectionOfVIP", "GetMostProfitable", "VIP", true)]
    public void SplitActionNameWorksAsExpected(string action, string operation, string cast, bool isCollection)
    {
        // Arrange
        string actual = OperationRoutingConvention.SplitActionName(action, out string actualCast, out bool actualIsCollection);

        // Act & Assert
        Assert.Equal(actual, operation);
        Assert.Equal(actualCast, cast);
        Assert.Equal(actualIsCollection, isCollection);
    }
}
