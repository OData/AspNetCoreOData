//-----------------------------------------------------------------------------
// <copyright file="CreatedODataResultTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using NavigationSourceLinkBuilderAnnotation = Microsoft.AspNetCore.OData.Edm.NavigationSourceLinkBuilderAnnotation;

namespace Microsoft.AspNetCore.OData.Tests.Results;

public class CreatedODataResultTest
{
    private readonly TestEntity _entity = new TestEntity();

    [Fact]
    public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => new CreatedODataResult<TestEntity>(entity: null), "entity");
    }

    [Fact]
    public void GetEntity_ReturnsCorrect()
    {
        // Arrange
        Mock<CreatedODataResultTest> mock = new Mock<CreatedODataResultTest>();
        CreatedODataResult<CreatedODataResultTest> result =
            new CreatedODataResult<CreatedODataResultTest>(mock.Object);

        // Act & Assert
        Assert.Same(mock.Object, result.Entity);
    }

    [Fact]
    public void GetInnerActionResult_ReturnsNegotiatedContentResult_IfRequestHasNoPreferenceHeader()
    {
        // Arrange
        var request = CreateRequest();
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act
        var result = createdODataResult.GetInnerActionResult(request);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Same(typeof(TestEntity), objectResult.Value.GetType());
        Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)objectResult.StatusCode);
    }

    [Fact]
    public void GetInnerActionResult_ReturnsNoContentStatusCodeResult_IfRequestAsksForNoContent()
    {
        // Arrange
        var request = CreateRequest("return=minimal");
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act
        var result = createdODataResult.GetInnerActionResult(request);

        // Assert
        StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(HttpStatusCode.NoContent, (HttpStatusCode)statusCodeResult.StatusCode);
    }

    [Fact]
    public void GetInnerActionResult_ReturnsNegotiatedContentResult_IfRequestAsksForContent()
    {
        // Arrange
        var request = CreateRequest("return=representation");
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act
        var result = createdODataResult.GetInnerActionResult(request);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Same(typeof(TestEntity), objectResult.Value.GetType());
        Assert.Equal(HttpStatusCode.Created, (HttpStatusCode)objectResult.StatusCode);
    }

    [Fact]
    public void GenerateLocationHeader_ThrowsODataPathMissing_IfRequestDoesNotHaveODataPath()
    {
        var request = RequestFactory.Create(EdmCoreModel.Instance);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
            "The operation cannot be completed because no ODataPath is available for the request.");
    }

    [Fact]
    public void GenerateLocationHeader_ThrowsEntitySetMissingDuringSerialization_IfODataPathEntitySetIsNull()
    {
        // Arrange
        ODataPath path = new ODataPath();
        var request = RequestFactory.Create(EdmCoreModel.Instance, path: path);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
            "The related entity set or singleton cannot be found from the OData path. The related entity set or singleton is required to serialize the payload.");
    }

    [Fact]
    public void GenerateLocationHeader_ThrowsEntityTypeNotInModel_IfContentTypeIsNotThereInModel()
    {
        // Arrange
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(opt => opt.AddRouteComponents(model.Model));
        request.Configure("", model.Model, path);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
            "Cannot find the resource type 'Microsoft.AspNetCore.OData.Tests.Results.CreatedODataResultTest+TestEntity' in the model.");
    }

    [Fact]
    public void GenerateLocationHeader_ThrowsTypeMustBeEntity_IfMappingTypeIsNotEntity()
    {
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        model.Model.SetAnnotationValue(model.Address, new ClrTypeAnnotation(typeof(TestEntity)));
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
            "NS.Address is not an entity type. Only entity types are supported.");
    }

    [Fact]
    public void GenerateLocationHeader_UsesEntitySetLinkBuilder_ToGenerateLocationHeader()
    {
        // Arrange
        Uri editLink = new Uri("http://id-link");
        Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
        linkBuilder.CallBase = true;
        linkBuilder.Setup(
            b => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.Full, null))
            .Returns(editLink);

        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
        model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act
        var locationHeader = createdODataResult.GenerateLocationHeader(request);

        // Assert
        Assert.Same(editLink, locationHeader);
    }

    [Fact]
    public void GenerateLocationHeader_ForContainment()
    {
        // Arrange
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.OrderLine, new ClrTypeAnnotation(typeof(OrderLine)));
        ODataPath path = new ODataUriParser(model.Model,
            new Uri("http://localhost/"),
            new Uri("MyOrders(1)/OrderLines", UriKind.Relative)).ParsePath();

        var request = RequestFactory.Create(opt => opt.AddRouteComponents(model.Model));
        request.Configure("", model.Model, path);
        var orderLine = new OrderLine { ID = 2 };
        var createdODataResult = GetCreatedODataResult<OrderLine>(orderLine, request);

        // Act
        var locationHeader = createdODataResult.GenerateLocationHeader(request);

        // Assert
        Assert.Equal("http://localhost/MyOrders(1)/OrderLines(2)", locationHeader.ToString());
    }

    [Fact]
    public void GenerateLocationHeader_ThrowsEditLinkNullForLocationHeader_IfEntitySetLinkBuilderReturnsNull()
    {
        // Arrange
        Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
        model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request);

        // Act
        ExceptionAssert.Throws<InvalidOperationException>(() => createdODataResult.GenerateLocationHeader(request),
            "The edit link builder for the entity set 'Customers' returned null. An edit link is required for the location header.");
    }

    [Fact]
    public void Property_LocationHeader_IsEvaluatedLazily()
    {
        // Arrange
        Uri editLink = new Uri("http://edit-link");
        Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
        linkBuilder.Setup(b => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.Full, null))
            .Returns(editLink);

        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
        model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        TestController controller = CreateController(request);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request, controller);

        // Act
        Uri locationHeader = createdODataResult.GenerateLocationHeader(request);

        // Assert
        Assert.Same(editLink, locationHeader);
    }

    [Fact]
    public void Property_LocationHeader_IsEvaluatedOnlyOnce()
    {
        // Arrange
        Uri editLink = new Uri("http://edit-link");
        Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
        linkBuilder.Setup(b => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.Full, null))
            .Returns(editLink);

        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
        model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        TestController controller = CreateController(request);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request, controller);

        // Act
        Uri locationHeader = createdODataResult.GenerateLocationHeader(request);

        // Assert
        linkBuilder.Verify(
            (b) => b.BuildEditLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.Full, null),
            Times.Once());
    }

    [Fact]
    public void Property_EntityIdHeader_IsEvaluatedLazilyAndOnlyOnce()
    {
        // Arrange
        Uri idLink = new Uri("http://id-link");
        Mock<NavigationSourceLinkBuilderAnnotation> linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
        linkBuilder.CallBase = true;
        linkBuilder.Setup(b => b.BuildIdLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.Full))
            .Returns(idLink);

        CustomersModelWithInheritance model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new ClrTypeAnnotation(typeof(TestEntity)));
        model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
        ODataPath path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        TestController controller = CreateController(request);
        var createdODataResult = GetCreatedODataResult<TestEntity>(_entity, request, controller);

        // Act
        Uri entityIdHeader = createdODataResult.GenerateEntityId(request);

        // Assert
        Assert.Same(idLink, entityIdHeader);
        linkBuilder.Verify(
            b => b.BuildIdLink(It.IsAny<ResourceContext>(), ODataMetadataLevel.Full),
            Times.Once());
    }

    private class TestEntity
    {
    }

    private class TestController : ODataController
    {
    }

    private CreatedODataResult<T> GetCreatedODataResult<T>(T entity, HttpRequest request, TestController controller = null)
    {
        return new CreatedODataResult<T>(entity);
    }

    private HttpRequest CreateRequest(string preferHeaderValue = null)
    {
        var request = RequestFactory.Create();
        if (!string.IsNullOrEmpty(preferHeaderValue))
        {
            request.Headers.Append("Prefer", new StringValues(preferHeaderValue));
        }

        return request;
    }

    private TestController CreateController(AspNetCore.Http.HttpRequest request)
    {
        TestController controller = new TestController();
        return controller;
    }
}
