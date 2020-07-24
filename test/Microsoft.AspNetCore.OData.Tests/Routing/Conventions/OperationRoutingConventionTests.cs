// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Conventions;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
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
}
