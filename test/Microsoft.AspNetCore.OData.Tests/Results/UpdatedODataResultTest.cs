//-----------------------------------------------------------------------------
// <copyright file="UpdatedODataResultTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Results
{
    public class UpdatedODataResultTest
    {
        private readonly TestEntity _entity = new TestEntity();

        [Fact]
        public void Ctor_ControllerDependency_ThrowsArgumentNull_Entity()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new UpdatedODataResult<TestEntity>(entity: null), "entity");
        }

        [Fact]
        public void GetEntity_ReturnsCorrect()
        {
            // Arrange
            Mock<UpdatedODataResultTest> mock = new Mock<UpdatedODataResultTest>();
            UpdatedODataResult<UpdatedODataResultTest> updatedODataResult =
                new UpdatedODataResult<UpdatedODataResultTest>(mock.Object);

            // Act & Assert
            Assert.Same(mock.Object, updatedODataResult.Entity);
        }

        [Fact]
        public void GetActionResult_ReturnsNoContentStatusCodeResult_IfRequestHasNoPreferenceHeader()
        {
            // Arrange
            var request = CreateRequest();

            // Act
            var result = CreateActionResult(request);

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, (HttpStatusCode)statusCodeResult.StatusCode);
        }

        [Fact]
        public void GetActionResult_ReturnsNoContentStatusCodeResult_IfRequestAsksForNoContent()
        {
            // Arrange
            var request = CreateRequest("return=minimal");

            // Act
            var result = CreateActionResult(request);

            // Assert
            StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(HttpStatusCode.NoContent, (HttpStatusCode)statusCodeResult.StatusCode);
        }

        [Fact]
        public void GetActionResult_ReturnsNegotiatedContentResult_IfRequestAsksForContent()
        {
            // Arrange
            var request = CreateRequest("return=representation");

            // Act
            var result = CreateActionResult(request);

            // Assert
            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Same(typeof(TestEntity), objectResult.Value.GetType());
            Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)objectResult.StatusCode);
        }

        private class TestEntity
        {
        }

        private class TestController : ODataController
        {
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

        private IActionResult CreateActionResult(HttpRequest request)
        {
            UpdatedODataResult<TestEntity> updatedODataResult = new UpdatedODataResult<TestEntity>(_entity);
            return updatedODataResult.GetInnerActionResult(request);
        }
    }
}
