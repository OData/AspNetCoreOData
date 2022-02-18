//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerContextTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Models;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataDeserializerContextTest
    {
        [Theory]
        [InlineData(typeof(Delta), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Delta<Customer>), true)]
        public void Property_IsDeltaOfT_HasRightValue(Type resourceType, bool expectedResult)
        {
            // Arrange & Act
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = resourceType };

            // Act
            Assert.Equal(expectedResult, context.IsDeltaOfT);
        }

        [Theory]
        [InlineData(typeof(Delta), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Delta<Customer>), false)]
        [InlineData(typeof(IEdmObject), true)]
        [InlineData(typeof(IEdmComplexObject), true)]
        [InlineData(typeof(IEdmEntityObject), true)]
        [InlineData(typeof(EdmComplexObject), true)]
        [InlineData(typeof(EdmEntityObject), true)]
        [InlineData(typeof(ODataUntypedActionParameters), true)]
        public void Property_IsNoClrType_HasRightValue(Type resourceType, bool expectedResult)
        {
            // Arrange & Act
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = resourceType };

            // Assert
            Assert.Equal(expectedResult, context.IsNoClrType);
        }
    }
}
