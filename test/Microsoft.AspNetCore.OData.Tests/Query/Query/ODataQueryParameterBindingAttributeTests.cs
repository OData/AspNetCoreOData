//-----------------------------------------------------------------------------
// <copyright file="ODataQueryParameterBindingAttributeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query;

public class ODataQueryParameterBindingAttributeTests
{
    [Fact]
    public void BindModelAsync_ThrowsArgumentNull_BindingContext()
    {
        // Arrange & Act & Assert
        ODataQueryParameterBindingAttribute.ODataQueryParameterBinding binding = new ODataQueryParameterBindingAttribute.ODataQueryParameterBinding();
        ExceptionAssert.ThrowsArgumentNull(() => binding.BindModelAsync(null), "bindingContext");
    }

    [Fact]
    public void BindModelAsync_ThrowsArgument_ModelBindingContextMustHaveRequest()
    {
        // Arrange
        ODataQueryParameterBindingAttribute.ODataQueryParameterBinding binding = new ODataQueryParameterBindingAttribute.ODataQueryParameterBinding();
        Mock<HttpContext> httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.Request).Returns((HttpRequest)null);

        Mock<ModelBindingContext> context = new Mock<ModelBindingContext>();
        context.Setup(c => c.HttpContext).Returns(httpContext.Object);

        // Act & Assert
        ExceptionAssert.ThrowsArgument(
            () => binding.BindModelAsync(context.Object),
            "bindingContext",
            "The model binding context requires an attached request in order to model binding.");
    }

    [Fact]
    public void BindModelAsync_ThrowsArgument_ActionContextMustHaveDescriptor()
    {
        // Arrange
        ODataQueryParameterBindingAttribute.ODataQueryParameterBinding binding = new ODataQueryParameterBindingAttribute.ODataQueryParameterBinding();

        Mock<ModelBindingContext> context = new Mock<ModelBindingContext>();
        context.Setup(c => c.HttpContext).Returns(new DefaultHttpContext());

        ActionContext actionContext = new ActionContext
        {
            ActionDescriptor = null
        };

        context.Setup(c => c.ActionContext).Returns(actionContext);

        // Act & Assert
        ExceptionAssert.ThrowsArgument(
            () => binding.BindModelAsync(context.Object),
            "actionContext",
            "The HttpActionContext.ActionDescriptor is null.");
    }

    [Theory]
    [InlineData(typeof(int[]), typeof(int))]
    [InlineData(typeof(IEnumerable<int>), typeof(int))]
    [InlineData(typeof(List<int>), typeof(int))]
    [InlineData(typeof(IQueryable<int>), typeof(int))]
    [InlineData(typeof(Task<IQueryable<int>>), typeof(int))]
    public void GetEntityClrTypeFromActionReturnType_Returns_CorrectEntityType(Type returnType, Type elementType)
    {
        // Arrange
        Mock<MethodInfo> mock = new Mock<MethodInfo>();
        mock.Setup(s => s.ReturnType).Returns(returnType);
        ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
        descriptor.MethodInfo = mock.Object;

        // Act & Assert
        Assert.Equal(
            elementType,
            ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.GetEntityClrTypeFromActionReturnType(descriptor));
    }

    [Theory]
    [InlineData(typeof(ODataQueryOptions<int>), typeof(int))]
    [InlineData(typeof(ODataQueryOptions<string>), typeof(string))]
    [InlineData(typeof(ODataQueryOptions), null)]
    [InlineData(typeof(int), null)]
    public void GetEntityClrTypeFromParameterType_Returns_CorrectEntityType(Type parameterType, Type elementType)
    {
        // Arrange & Act & Assert
        Assert.Equal(elementType,
            ODataQueryParameterBindingAttribute.GetEntityClrTypeFromParameterType(parameterType));
    }

    [Fact]
    public void GetEntityClrTypeFromActionReturnType_ThrowsInvalidOperation_ActionDescriptorNotControllerActionDescriptor()
    {
        // Arrange & Act & Assert
        ActionDescriptor descriptor = new Mock<ActionDescriptor>().Object;

        ExceptionAssert.Throws<InvalidOperationException>(
            () => ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.GetEntityClrTypeFromActionReturnType(descriptor),
            "ActionDescriptor is not ControllerActionDescriptor.");
    }

    [Fact]
    public void GetEntityClrTypeFromActionReturnType_ThrowsInvalidOperation_FailedToBuildEdmModelBecauseReturnTypeIsNull()
    {
        // Arrange & Act & Assert
        ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
        Mock<MethodInfo> mock = new Mock<MethodInfo>();
        mock.Setup(s => s.ReturnType).Returns((Type)null);
        descriptor.MethodInfo = mock.Object;

        ExceptionAssert.Throws<InvalidOperationException>(
            () => ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.GetEntityClrTypeFromActionReturnType(descriptor),
            "Cannot create an EDM model as the action '' on controller '' has a void return type.");
    }

    [Fact]
    public void GetEntityClrTypeFromActionReturnType_ThrowsInvalidOperation_FailedToRetrieveTypeToBuildEdmModel()
    {
        // Arrange & Act & Assert
        ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
        Mock<MethodInfo> mock = new Mock<MethodInfo>();
        mock.Setup(s => s.ReturnType).Returns(typeof(int));
        descriptor.MethodInfo = mock.Object;

        ExceptionAssert.Throws<InvalidOperationException>(
            () => ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.GetEntityClrTypeFromActionReturnType(descriptor),
            "Cannot create an EDM model as the action '' on controller '' has a return type 'System.Int32' that does not implement IEnumerable<T>.");
    }

    [Fact]
    public void IsODataQueryOptions_Returns_BooleanAsExpected()
    {
        // Arrange & Act & Assert
        Assert.False(ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.IsODataQueryOptions(null));
        Assert.False(ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.IsODataQueryOptions(typeof(int)));

        Assert.True(ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.IsODataQueryOptions(typeof(ODataQueryOptions)));
        Assert.True(ODataQueryParameterBindingAttribute.ODataQueryParameterBinding.IsODataQueryOptions(typeof(ODataQueryOptions<>)));
    }
}
