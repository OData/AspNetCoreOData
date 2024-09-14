//-----------------------------------------------------------------------------
// <copyright file="ResultHelpersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Results;

public class ResultHelpersTest
{
    private readonly TestEntity _entity = new TestEntity();
    private readonly Uri _entityId = new Uri("http://entity_id");
    private readonly string _version = "4.0.1.101";

    [Fact]
    public void GenerateODataLink_ThrowsIdLinkNullForEntityIdHeader_IfEntitySetLinkBuilderReturnsNull()
    {
        // Arrange
        var linkBuilder = new Mock<NavigationSourceLinkBuilderAnnotation>();
        var model = new CustomersModelWithInheritance();
        model.Model.SetAnnotationValue(model.Customer, new Microsoft.OData.ModelBuilder.ClrTypeAnnotation(typeof(TestEntity)));
        model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder.Object);
        var path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);

        // Act & Assert
        ExceptionAssert.Throws<InvalidOperationException>(
            () => ResultHelpers.GenerateODataLink(request, _entity, isEntityId: true),
            "The Id link builder for the entity set 'Customers' returned null. An Id link is required for the OData-EntityId header.");
    }

    [Fact]
    public void GenerateODataLink_CanResolveIEdmObject()
    {
        // Arrange
        var model = new CustomersModelWithInheritance();
        var path = new ODataPath(new EntitySetSegment(model.Customers));
        var request = RequestFactory.Create(model.Model, path: path);
        request.ODataFeature().BaseAddress = "http://localhost";
        var edmEntity = new EdmEntityObject(model.Customer);
        edmEntity.TrySetPropertyValue("ID", 1);

        // Act & Assert
        Assert.Equal("http://localhost/Customers(1)",
            ResultHelpers.GenerateODataLink(request, edmEntity, isEntityId: true).AbsoluteUri);
    }

    [Fact]
    public void AddEntityId_AddsEntityId_IfResponseStatusCodeIsNoContent()
    {
        // Arrange
        var response = ResponseFactory.Create(StatusCodes.Status204NoContent);

        // Act
        ResultHelpers.AddEntityId(response, () => _entityId);

        // Assert
        var entityIdHeaderValues = response.Headers[ResultHelpers.EntityIdHeaderName].ToList();
        Assert.Single(entityIdHeaderValues);
        Assert.Equal(_entityId.ToString(), entityIdHeaderValues.Single());
    }

    [Fact]
    public void AddEntityId_DoesNotAddEntityId_IfResponseStatusCodeIsOtherThanNoContent()
    {
        // Arrange
        var response = ResponseFactory.Create(StatusCodes.Status200OK);

        // Act
        ResultHelpers.AddEntityId(response, () => _entityId);

        // Assert
        Assert.False(response.Headers.ContainsKey(ResultHelpers.EntityIdHeaderName));
    }

    [Fact]
    public void AddServiceVersion_AddsODataVersion_IfResponseStatusCodeIsNoContent()
    {
        // Arrange
        var response = ResponseFactory.Create(StatusCodes.Status204NoContent);

        // Act
        ResultHelpers.AddServiceVersion(response, () => _version);

        // Assert
        var versionHeaderValues = response.Headers[ODataVersionConstraint.ODataServiceVersionHeader].ToList();
        Assert.Single(versionHeaderValues);
        Assert.Equal(_version, versionHeaderValues.Single());
    }

    [Fact]
    public void AddServiceVersion_DoesNotAddServiceVersion_IfResponseStatusCodeIsOtherThanNoContent()
    {
        // Arrange
        var response = ResponseFactory.Create(StatusCodes.Status200OK);

        // Act
        ResultHelpers.AddServiceVersion(response, () => _version);

        // Assert
        Assert.False(response.Headers.ContainsKey(ODataVersionConstraint.ODataServiceVersionHeader));
    }

    private class TestEntity
    {
    }
}
