//-----------------------------------------------------------------------------
// <copyright file="OrderByClauseHelpersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class OrderByClauseHelpersTests
    {
        [Fact]
        public void OrderByClause_ToList_WorksForNull()
        {
            // Arrange
            OrderByClause clause = null;

            // Act
            IList<OrderByClause> list = clause.ToList();

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public void OrderByClause_ToList_WorksForMultiple()
        {
            // Arrange
            SingleValueNode valueNode = new Mock<SingleValueNode>().Object;
            RangeVariable rangeVariable = new Mock<RangeVariable>().Object;

            OrderByClause clause1 = new OrderByClause(null, valueNode, OrderByDirection.Ascending, rangeVariable);
            OrderByClause clause = new OrderByClause(clause1, valueNode, OrderByDirection.Ascending, rangeVariable);

            // Act
            IList<OrderByClause> list = clause.ToList();

            // Assert
            Assert.NotEmpty(list);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void OrderByClause_IsTopLevelSingleProperty_ReturnsFalseForNull()
        {
            // Arrange
            OrderByClause clause = null;

            // Act
            bool isTop = clause.IsTopLevelSingleProperty(out _, out _);

            // Assert
            Assert.False(isTop);
        }

        [Fact]
        public void OrderByClause_IsTopLevelSingleProperty_ReturnsFalseForNonTopLevelPropertyNode()
        {
            // Arrange
            SingleValueNode valueNode = new Mock<SingleValueNode>().Object;
            RangeVariable rangeVariable = new Mock<RangeVariable>().Object;
            OrderByClause clause = new OrderByClause(null, valueNode, OrderByDirection.Ascending, rangeVariable);

            // Act
            bool isTop = clause.IsTopLevelSingleProperty(out _, out _);

            // Assert
            Assert.False(isTop);
        }

        [Fact]
        public void OrderByClause_IsTopLevelSingleProperty_ReturnsFalseForTopLevelPropertyNode()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> type = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmType> definition = new Mock<IEdmType>();
            definition.Setup(t => t.TypeKind).Returns(EdmTypeKind.Entity);
            type.Setup(t => t.Definition).Returns(definition.Object);

            ResourceRangeVariable variable = new ResourceRangeVariable("$it", type.Object, navigationSource: null);
            SingleValueNode source = new ResourceRangeVariableReferenceNode("$it", variable);
            Mock<IEdmProperty> property = new Mock<IEdmProperty>();
            property.Setup(p => p.Name).Returns("Top");
            property.Setup(p => p.Type).Returns(type.Object);
            property.Setup(p => p.PropertyKind).Returns(EdmPropertyKind.Structural);
            SingleValueNode valueNode = new SingleValuePropertyAccessNode(source, property.Object);
            RangeVariable rangeVariable = new Mock<RangeVariable>().Object;
            OrderByClause clause = new OrderByClause(null, valueNode, OrderByDirection.Ascending, rangeVariable);

            // Act
            bool isTop = clause.IsTopLevelSingleProperty(out IEdmProperty actualProperty, out string name);

            // Assert
            Assert.True(isTop);
            Assert.Same(property.Object, actualProperty);
            Assert.Equal("Top", name);
        }
    }
}
