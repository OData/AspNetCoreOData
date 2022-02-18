//-----------------------------------------------------------------------------
// <copyright file="ETagActionFilterAttributeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    public class ETagActionFilterAttributeTests
    {
        [Fact]
        public void OnActionExecuted_ThrowsArgumentNull_ActionExecutedContext()
        {
            // Arrange
            ETagActionFilterAttribute filter = new ETagActionFilterAttribute();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnActionExecuted(null), "actionExecutedContext");
        }

        [Fact]
        public void OnActionExecuted_ThrowsArgumentNull_HttpContextOnActionExecutedContext()
        {
            // Arrange
            ETagActionFilterAttribute filter = new ETagActionFilterAttribute();
            ActionContext actionContext = new ActionContext
            (
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor()
            );
            ActionExecutedContext context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null);
            context.HttpContext = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnActionExecuted(context), "httpContext");
        }

        [Fact]
        public void OnActionExecuted_ThrowsArgumentNull_PathOnActionExecutedContext()
        {
            // Arrange
            ETagActionFilterAttribute filter = new ETagActionFilterAttribute();
            HttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            ActionExecutedContext context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnActionExecuted(context), "path");
        }

        [Fact]
        public void OnActionExecuted_ThrowsArgumentNull_ModelOnActionExecutedContext()
        {
            // Arrange
            ETagActionFilterAttribute filter = new ETagActionFilterAttribute();
            HttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            ActionExecutedContext context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null);
            httpContext.ODataFeature().Path = new ODataPath();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnActionExecuted(context), "model");
        }

        [Fact]
        public void OnActionExecuted_ThrowsArgumentNull_ETagHandlerOnActionExecutedContext()
        {
            // Arrange
            ETagActionFilterAttribute filter = new ETagActionFilterAttribute();
            HttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            ActionExecutedContext context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null);
            httpContext.ODataFeature().Path = new ODataPath();
            httpContext.ODataFeature().Model = new EdmModel();
            httpContext.ODataFeature().Services = new ServiceCollection().BuildServiceProvider();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => filter.OnActionExecuted(context), "etagHandler");
        }

        [Fact]
        public void GetSingleEntityEntityType_Returns_EntityType()
        {
            // Arrange & Act & Assert
            Assert.Null(ETagActionFilterAttribute.GetSingleEntityEntityType(null));
            Assert.Null(ETagActionFilterAttribute.GetSingleEntityEntityType(new ODataPath()));
        }
    }
}
