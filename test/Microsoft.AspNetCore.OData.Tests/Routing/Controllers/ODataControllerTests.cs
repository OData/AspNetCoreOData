//-----------------------------------------------------------------------------
// <copyright file="ODataControllerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Controllers;

public class ODataControllerTests
{
    [Fact]
    public void CreatedOnODataController_ThrowsArgumentNull_Entity()
    {
        // Arrange
        MyController myController = new MyController();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => myController.MyCreated<Customer>(null), "entity");
    }

    [Fact]
    public void UpdatedOnODataController_ThrowsArgumentNull_Entity()
    {
        // Arrange
        MyController myController = new MyController();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => myController.MyUpdated<Customer>(null), "entity");
    }

    private class Customer
    {
    }
}

public class MyController : ODataController
{
    public CreatedODataResult<TEntity> MyCreated<TEntity>(TEntity entity)
    {
        return base.Created(entity);
    }

    public UpdatedODataResult<TEntity> MyUpdated<TEntity>(TEntity entity)
    {
        return base.Updated(entity);
    }
}
