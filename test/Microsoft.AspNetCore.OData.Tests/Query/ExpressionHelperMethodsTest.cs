// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNetCore.OData.Query;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ExpressionHelperMethodsTests
    {
        [Fact]
        public void EntityAsQueryable_Returns_MethodInfo()
        {
            // Arrange & Act & Assert
            Assert.Equal("ToQueryable", ExpressionHelperMethods.EntityAsQueryable.Name);

            Assert.Equal("SelectMany", ExpressionHelperMethods.QueryableSelectManyGeneric.Name);

            Assert.Equal("Max", ExpressionHelperMethods.QueryableMax.Name);
            Assert.Equal("Min", ExpressionHelperMethods.QueryableMin.Name);

            Assert.Equal("GroupBy", ExpressionHelperMethods.EnumerableGroupByGeneric.Name);
            Assert.Equal("SelectMany", ExpressionHelperMethods.EnumerableSelectManyGeneric.Name);
        }
    }
}
