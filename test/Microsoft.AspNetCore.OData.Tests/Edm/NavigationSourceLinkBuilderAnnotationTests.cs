//-----------------------------------------------------------------------------
// <copyright file="NavigationSourceLinkBuilderAnnotationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class NavigationSourceLinkBuilderAnnotationTests
    {
        private static IEdmModel _model;
        private static IEdmEntitySet _customers;

        static NavigationSourceLinkBuilderAnnotationTests()
        {
            _model = GetEdmMode();
            _customers = _model.EntityContainer.FindEntitySet("Customers");
        }

        [Fact]
        public void CtorNavigationSourceLinkBuilderAnnotation_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationSourceLinkBuilderAnnotation(null, null), "navigationSource");

            // Arrange & Act & Assert
            IEdmNavigationSource navigationSource = new Mock<IEdmNavigationSource>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationSourceLinkBuilderAnnotation(navigationSource, null), "model");
        }

        [Fact]
        public void BuildIdLink_ThrowsArgumentNull_InstanceContext()
        {
            // Arrange
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_customers, _model);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildIdLink(null, ODataMetadataLevel.Full), "instanceContext");
        }

        [Fact]
        public void BuildEditLink_ThrowsArgumentNull_InstanceContext()
        {
            // Arrange
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_customers, _model);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildEditLink(null, ODataMetadataLevel.Full, null), "instanceContext");
        }

        [Fact]
        public void BuildReadLink_ThrowsArgumentNull_InstanceContext()
        {
            // Arrange
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_customers, _model);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildReadLink(null, ODataMetadataLevel.Full, null), "instanceContext");
        }

        [Fact]
        public void BuildNavigationLink_ThrowsArgumentNull_InstanceContext()
        {
            // Arrange
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_customers, _model);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildNavigationLink(null, null, ODataMetadataLevel.Full), "instanceContext");
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildNavigationLink(null, null), "instanceContext");
        }

        [Fact]
        public void BuildNavigationLink_ThrowsArgumentNull_NavigationProperty()
        {
            // Arrange
            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation(_customers, _model);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildNavigationLink(new ResourceContext(), null, ODataMetadataLevel.Full), "navigationProperty");
            ExceptionAssert.ThrowsArgumentNull(() => linkBuilder.BuildNavigationLink(new ResourceContext(), null), "navigationProperty");
        }

        [Fact]
        public void CtorNavigationSourceLinkBuilderAnnotation_SetsProperties()
        {
            // Arrange & Act & Assert
            Func<ResourceContext, IEdmNavigationProperty, Uri> navigationLinkFactory = (r, p) => null;
            NavigationLinkBuilder builder = new NavigationLinkBuilder(navigationLinkFactory, false);

            // Assert
            Assert.Same(navigationLinkFactory, builder.Factory);
            Assert.False(builder.FollowsConventions);
        }

        private static IEdmModel GetEdmMode()
        {
            var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }

        private class Customer
        {
            public int Id { get; set; }
        }
    }
}
