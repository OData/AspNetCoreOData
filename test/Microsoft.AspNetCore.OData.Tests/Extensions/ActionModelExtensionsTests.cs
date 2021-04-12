// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class ActionModelExtensionsTests
    {
        private static MethodInfo _methodInfo = typeof(TestController).GetMethod("Index");

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsNonODataAction_ReturnsAsExpected(bool expected)
        {
            // Arrange
            List<object> attributes = new List<object>();
            if (expected)
            {
                attributes.Add(new NonODataActionAttribute());
            }
            ActionModel action = new ActionModel(_methodInfo, attributes);

            // Act
            bool actual = action.IsNonODataAction();

            // Assert
            Assert.Equal(expected, actual);
        }
    }

    internal class TestController
    {
        public void Index(int id)
        {
        }
    }
}