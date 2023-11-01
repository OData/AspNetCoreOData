//-----------------------------------------------------------------------------
// <copyright file="QueryFilterProviderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Globalization;
using System;
using System.Reflection;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class QueryFilterProviderTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_QueryFilter()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new QueryFilterProvider(null), "queryFilter");
        }

        [Fact]
        public void Ctor_SetsPropertiesCorrectly()
        {
            // Arrange & Act & Assert
            Mock<IActionFilter> filter = new Mock<IActionFilter>();
            QueryFilterProvider provider = new QueryFilterProvider(filter.Object);
            Assert.Same(filter.Object, provider.QueryFilter);
            Assert.Equal(0, provider.Order);
        }

        [Fact]
        public void OnProvidersExecuting_ThrowsArgumentNull_Context()
        {
            // Arrange & Act & Assert
            Mock<IActionFilter> filter = new Mock<IActionFilter>();
            QueryFilterProvider provider = new QueryFilterProvider(filter.Object);
            ExceptionAssert.ThrowsArgumentNull(() => provider.OnProvidersExecuting(null), "context");
        }

        [Fact]
        public void OnProvidersExecuting_AddQueryFilter()
        {
            // Arrange
            Mock<IActionFilter> filter = new Mock<IActionFilter>();
            QueryFilterProvider provider = new QueryFilterProvider(filter.Object);

            List<FilterItem> items = new List<FilterItem>();
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor()
            {
                ControllerTypeInfo = typeof(UserController).GetTypeInfo(),
                MethodInfo = typeof(UserController).GetMethod("GetUser"),
                Parameters = new List<ParameterDescriptor>()
            };
            FilterProviderContext context = CreateFilterContext(items, descriptor);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            FilterItem item = Assert.Single(items);
            Assert.Same(filter.Object, item.Filter);
        }

        [Fact]
        public void IsIQueryable_Works_ForInputTypes()
        {
            // Arrange & Act & Assert
            Assert.True(QueryFilterProvider.IsIQueryable(typeof(IQueryable)));
            Assert.True(QueryFilterProvider.IsIQueryable(typeof(IQueryable<>)));
            Assert.False(QueryFilterProvider.IsIQueryable(typeof(IEnumerable<>)));
            Assert.False(QueryFilterProvider.IsIQueryable(typeof(int)));
        }

        private FilterProviderContext CreateFilterContext(List<FilterItem> items, ControllerActionDescriptor descriptor)
        {
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor);
            actionContext.ActionDescriptor.FilterDescriptors = new List<FilterDescriptor>(
                items.Select(item => item.Descriptor));

            return new FilterProviderContext(actionContext, items);
        }

        private class UserController : ControllerBase
        {
            public IQueryable GetUser()
            {
                throw new NotImplementedException();
            }
        }
    }
}
