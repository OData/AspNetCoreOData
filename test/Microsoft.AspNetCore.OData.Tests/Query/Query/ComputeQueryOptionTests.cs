//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryOptionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ComputeQueryOptionTests
    {
        private static IEdmModel _model = GetModel();

        [Fact]
        public void CtorComputeQueryOption_ThrowsArgumentNull_ForInputParameter()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new ComputeQueryOption(null, null, null), "rawValue");
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => new ComputeQueryOption(string.Empty, null, null), "rawValue");

            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ComputeQueryOption("groupby", null, null), "context");

            // Arrange & Act & Assert
            ODataQueryContext context = new ODataQueryContext(EdmCoreModel.Instance, typeof(int));
            ExceptionAssert.ThrowsArgumentNull(() => new ComputeQueryOption("groupby", context, null), "queryOptionParser");
        }

        [Fact]
        public void CtorComputeQueryOption_CanConstructValidComputeQuery()
        {
            // Arrange
            IEdmModel model = _model;
            ODataQueryContext context = new ODataQueryContext(model, typeof(ComputeCustomer));

            // Act
            ComputeQueryOption compute = new ComputeQueryOption("Price mul Qty as Total", context);

            // Assert
            Assert.Same(context, compute.Context);
            Assert.Equal("Price mul Qty as Total", compute.RawValue);
        }

        [Fact]
        public void CtorComputeQueryOption_GetQueryNodeParsesQuery()
        {
            // Arrange
            IEdmModel model = _model;
            ODataQueryContext context = new ODataQueryContext(model, typeof(ComputeCustomer)) { RequestContainer = new MockServiceProvider() };

            // Act
            ComputeQueryOption compute = new ComputeQueryOption("Price mul Qty as Total,Price mul 2.0 as Tax", context);
            ComputeClause computeClause = compute.ComputeClause;

            // Assert
            Assert.Equal(2, computeClause.ComputedItems.Count());

            Assert.Collection(computeClause.ComputedItems,
                e =>
                {
                    Assert.Equal("Total", e.Alias);

                    Assert.Equal(QueryNodeKind.BinaryOperator, e.Expression.Kind);
                    BinaryOperatorNode binaryNode = e.Expression as BinaryOperatorNode;
                    Assert.Equal(BinaryOperatorKind.Multiply, binaryNode.OperatorKind);
                    Assert.Equal(QueryNodeKind.Convert, binaryNode.Right.Kind);
                    ConvertNode convertNode = (ConvertNode)binaryNode.Right;
                    Assert.Equal("Qty", ((SingleValuePropertyAccessNode)convertNode.Source).Property.Name);

                    Assert.Equal(QueryNodeKind.SingleValuePropertyAccess, binaryNode.Left.Kind);
                    var propertyAccessNode = binaryNode.Left as SingleValuePropertyAccessNode;
                    Assert.Equal("Price", propertyAccessNode.Property.Name);
                },
                e =>
                {
                    Assert.Equal("Tax", e.Alias);

                    Assert.Equal(QueryNodeKind.BinaryOperator, e.Expression.Kind);
                    BinaryOperatorNode binaryNode = e.Expression as BinaryOperatorNode;
                    Assert.Equal(BinaryOperatorKind.Multiply, binaryNode.OperatorKind);
                    Assert.Equal(QueryNodeKind.Constant, binaryNode.Right.Kind);
                    Assert.Equal(2.0, ((ConstantNode)binaryNode.Right).Value);

                    Assert.Equal(QueryNodeKind.SingleValuePropertyAccess, binaryNode.Left.Kind);
                    var propertyAccessNode = binaryNode.Left as SingleValuePropertyAccessNode;
                    Assert.Equal("Price", propertyAccessNode.Property.Name);
                });
        }

        private static IEdmModel GetModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ComputeCustomer>("Customers");
            return builder.GetEdmModel();
        }
    }

    public class ComputeCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public double Price { get; set; }

        public int Qty { get; set; }

        public IDictionary<string, object> Dynamics { get; set; }
    }

    public class ComputeAddress
    {
        public string Street { get; set; }
    }
}
