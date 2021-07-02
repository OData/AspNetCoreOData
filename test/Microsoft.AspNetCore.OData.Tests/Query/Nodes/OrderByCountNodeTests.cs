// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class OrderByCountNodeTests
    {
        [Fact]
        public void CtorOrderByCountNode_ThrowsArgumentNull_OrderByClause()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new OrderByCountNode(null), "orderByClause");
        }
    }
}