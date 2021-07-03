// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class ActionModelExtensionsTests
    {
        private static MethodInfo _methodInfo = typeof(TestController).GetMethod("Index");

        [Fact]
        public void IsNonODataAction_ThrowsArgumentNull_Action()
        {
            // Arrange & Act & Assert
            ActionModel action = null;
            ExceptionAssert.ThrowsArgumentNull(() => action.IsODataIgnored(), "action");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsNonODataAction_ReturnsAsExpected(bool expected)
        {
            // Arrange
            List<object> attributes = new List<object>();
            if (expected)
            {
                attributes.Add(new ODataIgnoredAttribute());
            }
            ActionModel action = new ActionModel(_methodInfo, attributes);

            // Act
            bool actual = action.IsODataIgnored();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void HasParameter_ThrowsArgumentNull_Action()
        {
            // Arrange & Act & Assert
            ActionModel action = null;
            ExceptionAssert.ThrowsArgumentNull(() => action.HasParameter<int>("key"), "action");
        }

        [Fact]
        public void HasParameter_Returns_BooleanAsExpected()
        {
            // Arrange
            ActionModel action = _methodInfo.BuildActionModel();

            // Act & Assert
            Assert.False(action.HasParameter<int>("key"));
            Assert.True(action.HasParameter<int>("id"));
            Assert.False(action.HasParameter<string>("id"));
        }

        [Fact]
        public void GetAttribute_ThrowsArgumentNull_Action()
        {
            // Arrange & Act & Assert
            ActionModel action = null;
            ExceptionAssert.ThrowsArgumentNull(() => action.GetAttribute<int>(), "action");
        }

        [Fact]
        public void HasODataKeyParameter_ThrowsArgumentNull_Action()
        {
            // Arrange & Act & Assert
            ActionModel action = null;
            ExceptionAssert.ThrowsArgumentNull(() => action.HasODataKeyParameter(entityType: null), "action");
        }

        [Fact]
        public void HasODataKeyParameter_ThrowsArgumentNull_EntityType()
        {
            // Arrange & Act & Assert
            ActionModel action = new ActionModel(_methodInfo, new List<object>());
            ExceptionAssert.ThrowsArgumentNull(() => action.HasODataKeyParameter(entityType: null), "entityType");
        }

        [Fact]
        public void AddSelector_ThrowsArgumentNull_ForInputParameter()
        {
            // Arrange & Act & Assert
            ActionModel action = null;
            ExceptionAssert.ThrowsArgumentNull(() => action.AddSelector(null, null, null, null), "action");

            // Arrange & Act & Assert
            MethodInfo methodInfo = typeof(TestController).GetMethod("Get");
            action = methodInfo.BuildActionModel();
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => action.AddSelector(null, null, null, null), "httpMethods");

            // Arrange & Act & Assert
            string httpMethods = "get";
            ExceptionAssert.ThrowsArgumentNull(() => action.AddSelector(httpMethods, null, null, null), "model");

            // Arrange & Act & Assert
            IEdmModel model = new Mock<IEdmModel>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => action.AddSelector(httpMethods, null, model, null), "path");
        }
    }

    internal class TestController
    {
        public void Index(int id)
        {
        }

        public void Get(int key)
        { }
    }
}