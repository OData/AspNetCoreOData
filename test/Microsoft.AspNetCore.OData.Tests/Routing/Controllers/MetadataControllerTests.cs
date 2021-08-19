//-----------------------------------------------------------------------------
// <copyright file="MetadataControllerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Controllers
{
    public class MetadataControllerTests
    {
        [Fact]
        public void GetMetadataOnMetadataController_ReturnsODataModel()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            HttpRequest reqest = RequestFactory.Create(model);

            MetadataController metadataController = new MetadataController();
            metadataController.ControllerContext.HttpContext = reqest.HttpContext;

            // Act
            IEdmModel actual = metadataController.GetMetadata();

            // Assert
            Assert.Same(model, actual);
        }

        [Fact]
        public void GetMetadataOnMetadataController_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpRequest reqest = RequestFactory.Create(model: null);

            MetadataController metadataController = new MetadataController();
            metadataController.ControllerContext.HttpContext = reqest.HttpContext;

            // Act
            Action test = () => metadataController.GetMetadata();

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test,
                "The request must have an associated EDM model. Consider registering Edm model calling AddOData().");
        }

        [Fact]
        public void GetODataServiceDocumentOnMetadataController_ReturnsODataODataServiceDocument()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType entity = new EdmEntityType("NS", "Entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            container.AddSingleton("me", entity);
            model.AddElement(entity);
            model.AddElement(container);

            HttpRequest reqest = RequestFactory.Create(model);

            MetadataController metadataController = new MetadataController();
            metadataController.ControllerContext.HttpContext = reqest.HttpContext;

            // Act
            ODataServiceDocument actual = metadataController.GetServiceDocument();

            // Assert
            Assert.NotNull(actual);
            Assert.Empty(actual.EntitySets);
            Assert.Empty(actual.FunctionImports);
            ODataSingletonInfo singletonInfo = Assert.Single(actual.Singletons);
            Assert.Equal("me", singletonInfo.Name);
        }

        [Fact]
        public void GetODataServiceDocumentOnMetadataController_ThrowsInvalidOperationException_()
        {
            // Arrange
            HttpRequest reqest = RequestFactory.Create(model: null);

            MetadataController metadataController = new MetadataController();
            metadataController.ControllerContext.HttpContext = reqest.HttpContext;

            // Act
            Action test = () => metadataController.GetServiceDocument();

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test,
                "The request must have an associated EDM model. Consider registering Edm model calling AddOData().");
        }
    }
}
