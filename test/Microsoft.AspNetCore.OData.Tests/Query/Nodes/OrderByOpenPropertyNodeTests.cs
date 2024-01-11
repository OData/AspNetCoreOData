//-----------------------------------------------------------------------------
// <copyright file="OrderByOpenPropertyNodeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class OrderByOpenPropertyNodeTests
    {
        [Fact]
        public void CtorOrderByOpenPropertyNodeTests_ThrowsArgumentNull_OrderByClause()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new OrderByOpenPropertyNode(null), "orderByClause");
        }

        [Fact]
        public void CtorOrderByOpenPropertyNodeTests_ThrowsODataException_NotOpenPropertyAccess()
        {
            // Arrange
            SingleValueNode singleValue = new Mock<SingleValueNode>().Object;
            RangeVariable rangeVariable = new Mock<RangeVariable>().Object;
            OrderByClause orderBy = new OrderByClause(null, singleValue, OrderByDirection.Descending, rangeVariable);

            // Act & Assert
            ExceptionAssert.Throws<ODataException>(() => new OrderByOpenPropertyNode(orderBy),
                "OrderBy clause kind 'None' is not valid. Only kind 'SingleValueOpenPropertyAccess' is accepted.");
        }
    }
}
