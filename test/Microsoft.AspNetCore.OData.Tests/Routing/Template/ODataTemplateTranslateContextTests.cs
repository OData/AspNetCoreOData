//-----------------------------------------------------------------------------
// <copyright file="ODataTemplateTranslateContextTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template;

public  class ODataTemplateTranslateContextTests
{
    [Fact]
    public void CtorODataTemplateTranslateContext_ThrowsArgumentNull_Context()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => new ODataTemplateTranslateContext(null, null, null, null), "context");
    }

    [Fact]
    public void CtorODataTemplateTranslateContext_ThrowsArgumentNull_Endpoint()
    {
        // Arrange
        HttpContext context = new Mock<HttpContext>().Object;

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => new ODataTemplateTranslateContext(context, null, null, null), "endpoint");
    }

    [Fact]
    public void CtorODataTemplateTranslateContext_ThrowsArgumentNull_RouteValues()
    {
        // Arrange
        HttpContext context = new Mock<HttpContext>().Object;
        Endpoint endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test");

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => new ODataTemplateTranslateContext(context, endpoint, null, null), "routeValues");
    }

    [Fact]
    public void CtorODataTemplateTranslateContext_ThrowsArgumentNull_Model()
    {
        // Arrange
        HttpContext context = new Mock<HttpContext>().Object;
        Endpoint endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test");
        RouteValueDictionary routeValues = new RouteValueDictionary();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => new ODataTemplateTranslateContext(context, endpoint, routeValues, null), "model");
    }

    [Fact]
    public void CtorODataTemplateTranslateContext_SetsProperties()
    {
        // Arrange
        HttpContext context = new Mock<HttpContext>().Object;
        Endpoint endpoint = new Endpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "Test");
        RouteValueDictionary routeValues = new RouteValueDictionary();
        IEdmModel model = EdmCoreModel.Instance;

        // Act
        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext(context, endpoint, routeValues, model);

        // Assert
        Assert.Same(context, translateContext.HttpContext);
        Assert.Same(endpoint, translateContext.Endpoint);
        Assert.Same(routeValues, translateContext.RouteValues);
        Assert.Same(model, translateContext.Model);
        Assert.Empty(translateContext.UpdatedValues);
        Assert.Empty(translateContext.Segments);
    }

    [Fact]
    public void GetParameterAliasOrSelf_ReturnsNull_IfAliasNull()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
        requestFeature.QueryString = "?@p=[1, 2, null, 7, 8]";

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            HttpContext = context
        };

        // Act
        string alias = translateContext.GetParameterAliasOrSelf(null);

        // Assert
        Assert.Null(alias);
    }

    [Fact]
    public void GetParameterAliasOrSelf_ReturnsExpectedAliasValue()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
        requestFeature.QueryString = "?@p=[1, 2, null, 7, 8]";

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            HttpContext = context
        };

        // Act
        string alias = translateContext.GetParameterAliasOrSelf("@p");

        // Assert
        Assert.Equal("[1, 2, null, 7, 8]", alias);
    }

    [Fact]
    public void GetParameterAliasOrSelf_ReturnsExpectedAliasValue_ForMultiple()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
        requestFeature.QueryString = "?@p=@age&@age=@para1&@para1='ab''c'";

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            HttpContext = context
        };

        // Act
        string alias = translateContext.GetParameterAliasOrSelf("@p");

        // Assert
        Assert.Equal("'ab''c'", alias);
    }

    [Fact]
    public void GetParameterAliasOrSelf_Throws_ForInfiniteLoopParameterAlias()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
        requestFeature.QueryString = "?@p=@age&@age=@p1&@p1=@p";

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            HttpContext = context
        };

        // Act
        Action test = () => translateContext.GetParameterAliasOrSelf("@p");

        // Assert
        ExceptionAssert.Throws<ODataException>(test, "The parameter alias '@p' is in an infinite loop.");
    }

    [Fact]
    public void GetParameterAliasOrSelf_Throws_ForMissingParameterAlias()
    {
        // Arrange
        HttpContext context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
        requestFeature.QueryString = "?@p=@age&@para1='abc'";

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            HttpContext = context
        };

        // Act
        Action test = () => translateContext.GetParameterAliasOrSelf("@p");

        // Assert
        ExceptionAssert.Throws<ODataException>(test, "Missing the parameter alias '@age' in the request query string.");
    }

    [Theory]
    [InlineData("/{key}", true)]
    [InlineData("/{KEY}", true)]
    [InlineData("({key})", false)]
    [InlineData("({KEY})", false)]
    [InlineData("a/customer", true)]
    public void IsPartOfRouteTemplate_ReturnsCorrect_ForGivenPart(string part, bool expect)
    {
        // Arrange
        RouteEndpoint endpoint = new RouteEndpoint(
            c => Task.CompletedTask,
            RoutePatternFactory.Parse("odata/customers/{key}/Name"),
            0,
            EndpointMetadataCollection.Empty,
            "test");

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            Endpoint = endpoint
        };

        // Act
        bool actual = translateContext.IsPartOfRouteTemplate(part);

        // Assert
        Assert.Equal(expect, actual);
    }

    [Fact]
    public void IsPartOfRouteTemplate_ReturnsFalse_ForNonRouteEndpoint()
    {
        // Arrange
        MyEndpoint endpoint = new MyEndpoint(c => Task.CompletedTask, EndpointMetadataCollection.Empty, "test");

        ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
        {
            Endpoint = endpoint
        };

        // Act
        bool actual = translateContext.IsPartOfRouteTemplate("/{key}");

        // Assert
        Assert.False(actual);
    }

    internal class MyEndpoint : Endpoint
    {
        public MyEndpoint(RequestDelegate requestDelegate, EndpointMetadataCollection metadata, string displayName)
            : base(requestDelegate, metadata, displayName)
        { }
    }
}
