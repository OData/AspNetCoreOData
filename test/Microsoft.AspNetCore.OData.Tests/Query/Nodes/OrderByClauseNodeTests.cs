//-----------------------------------------------------------------------------
// <copyright file="OrderByClauseNodeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class OrderByClauseNodeTests
{
    [Fact]
    public void CtorOrderByClauseNode_ThrowsArgumentNull_OrderByClause()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new OrderByClauseNode(null), "orderByClause");
    }
}
