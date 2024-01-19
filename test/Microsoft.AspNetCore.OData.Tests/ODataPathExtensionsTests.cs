//-----------------------------------------------------------------------------
// <copyright file="ODataPathExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataPathExtensionsTests
    {
        [Fact]
        public void IsStreamPropertyPath_Returns_CollectBooleanValue()
        {
            // Arrange
            ODataPath path = null;

            // Act & Assert
            Assert.False(path.IsStreamPropertyPath());

            // Act & Assert
            path = new ODataPath(MetadataSegment.Instance);
            Assert.False(path.IsStreamPropertyPath());

            // Act & Assert
            IEdmTypeReference typeRef = EdmCoreModel.Instance.GetStream(false);
            Mock<IEdmStructuralProperty> mock = new Mock<IEdmStructuralProperty>();
            mock.Setup(s => s.Name).Returns("any");
            mock.Setup(s => s.Type).Returns(typeRef);
            PropertySegment segment = new PropertySegment(mock.Object);

            path = new ODataPath(segment);
            Assert.True(path.IsStreamPropertyPath());
        }

        [Fact]
        public void GetEdmType_ThrowsArgumentNull_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = null;
            ExceptionAssert.ThrowsArgumentNull(() => path.GetEdmType(), "path");
        }

        [Fact]
        public void GetNavigationSource_ThrowsArgumentNull_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = null;
            ExceptionAssert.ThrowsArgumentNull(() => path.GetNavigationSource(), "path");
        }

        [Theory]
        // entity set segment should return the entity set
        [InlineData("Customers", "Customers", EdmNavigationSourceKind.EntitySet)]
        // key segment should return the corresponding entity set
        [InlineData("Customers(1)", "Customers", EdmNavigationSourceKind.EntitySet)]
        // property segment should return the previous segment's navigation source
        [InlineData("Customers(1)/Name", "Customers", EdmNavigationSourceKind.EntitySet)]
        // navigation property segment
        [InlineData("Customers(1)/Orders", "Orders", EdmNavigationSourceKind.EntitySet)]
        // key segment of a navigation property
        [InlineData("Customers(1)/Orders(1)", "Orders", EdmNavigationSourceKind.EntitySet)]
        // navigation property link segment
        [InlineData("Customers(1)/Orders(1)/$ref", "Orders", EdmNavigationSourceKind.EntitySet)]
        // property segment of a navigation property
        [InlineData("Customers(1)/Orders(1)/Amount", "Orders", EdmNavigationSourceKind.EntitySet)]
        // non-contained navigation property without an entity set should return UnknownEntitySet
        [InlineData("MyOrders(1)/NonContainedOrderLines", "NonContainedOrderLines", EdmNavigationSourceKind.UnknownEntitySet)]
        [InlineData("MyOrders(1)/NonContainedOrderLines(1)", "NonContainedOrderLines", EdmNavigationSourceKind.UnknownEntitySet)]
        [InlineData("MyOrders(1)/NonContainedOrderLines(1)/$ref", "NonContainedOrderLines", EdmNavigationSourceKind.UnknownEntitySet)]
        // contained navigation property should return ContainedEntitySet
        [InlineData("MyOrders(1)/OrderLines", "OrderLines", EdmNavigationSourceKind.ContainedEntitySet)]
        [InlineData("MyOrders(1)/OrderLines(1)", "OrderLines", EdmNavigationSourceKind.ContainedEntitySet)]
        [InlineData("MyOrders(1)/OrderLines(1)/$ref", "OrderLines", EdmNavigationSourceKind.ContainedEntitySet)]
        // property segment with a complex type, returns navigation source of previous segment
        [InlineData("Customers(1)/Account", "Customers", EdmNavigationSourceKind.EntitySet)]
        // property segment of a complex type, returns navigation source of previous segment
        [InlineData("Customers(1)/Account/Bank", "Customers", EdmNavigationSourceKind.EntitySet)]
        // dynamic path segment should return null
        [InlineData("Customers(1)/Account/DynamicProperty", null, EdmNavigationSourceKind.None)]
        // unbound operation import with an entity set path should return the entity set
        [InlineData("GetTopCustomers()", "Customers", EdmNavigationSourceKind.EntitySet)]
        // unbound operation import without an entity set path should return null
        [InlineData("GetTotalSalesAmount()", null, EdmNavigationSourceKind.None)]
        // bound operation without an entity set path should return null
        [InlineData("Customers(1)/NS.IsUpgraded()", null, EdmNavigationSourceKind.None)]
        [InlineData("Customers/NS.UpgradeAll()", null, EdmNavigationSourceKind.None)]
        // bound operaiton with an entity set path should return the entity set
        [InlineData("Customers/NS.GetTop", "Customers", EdmNavigationSourceKind.EntitySet)]
        [InlineData("Customers/NS.GetBestOrders()", "Orders", EdmNavigationSourceKind.EntitySet)]
        // singleton segment should return the singleton
        [InlineData("VipCustomer", "VipCustomer", EdmNavigationSourceKind.Singleton)]
        [InlineData("VipCustomer/Name", "VipCustomer", EdmNavigationSourceKind.Singleton)]
        // navigation property segment after a singleton should return the navigation property's entity set
        [InlineData("VipCustomer/Orders", "Orders", EdmNavigationSourceKind.EntitySet)]
        [InlineData("VipCustomer/NS.IsUpgraded", null, EdmNavigationSourceKind.None)]
        // type segment should return the corresponding navigation source
        [InlineData("Customers(1)/NS.SpecialCustomer", "Customers", EdmNavigationSourceKind.EntitySet)]
        [InlineData("VipCustomer/NS.SpecialCustomer", "VipCustomer", EdmNavigationSourceKind.Singleton)]
        // coung segment should return null
        [InlineData("Customers/$count", null, EdmNavigationSourceKind.None)]
        // value segment should return null
        [InlineData("Customers(1)/Account/Amount/$value", null, EdmNavigationSourceKind.None)]
        // batch segment should return null
        [InlineData("$batch", null, EdmNavigationSourceKind.None)]
        // metadata segment should return null
        [InlineData("$metadata", null, EdmNavigationSourceKind.None)]
        public void GetNavigationSource_ReturnsCorrectNavigationSource(string path, string expectedNavigationSource, EdmNavigationSourceKind navigationSourceKind)
        {
            var model = new Models.CustomersModelWithInheritance();
            var parser = new ODataUriParser(model.Model, new Uri(path, UriKind.Relative));
            parser.EnableUriTemplateParsing = true;
            ODataPath odataPath = parser.ParsePath();

            var navigationSource = odataPath.GetNavigationSource();

            if (expectedNavigationSource == null)
            {
                Assert.Null(navigationSource);
            }
            else
            {
                Assert.NotNull(navigationSource);
                Assert.Equal(expectedNavigationSource, navigationSource.Name);
                Assert.Equal(navigationSourceKind, navigationSource.NavigationSourceKind());
            }
        }

        [Fact]
        public void GetNavigationSource_WhenPathTemplateSegment_ReturnsNull()
        {
            var model = new Models.CustomersModelWithInheritance();
            ODataPath path = new ODataPath(
                new EntitySetSegment(model.Customers),
                new PathTemplateSegment("template")
            );

            var navigationSource = path.GetNavigationSource();

            Assert.Null(navigationSource);
        }

        [Fact]
        public void GetNavigationSource_WhenUnknownPathSegment_ReturnsNull()
        {
            var model = new Models.CustomersModelWithInheritance();
            ODataPath path = new ODataPath(
                new EntitySetSegment(model.Customers),
                new UnknownTestODataPathSegment()
            );

            var navigationSource = path.GetNavigationSource();

            Assert.Null(navigationSource);
        }

        [Fact]
        public void GetPathString_ThrowsArgumentNull_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = null;
            ExceptionAssert.ThrowsArgumentNull(() => path.GetPathString(), "path");

            // Arrange & Act & Assert
            IList<ODataPathSegment> segments = null;
            ExceptionAssert.ThrowsArgumentNull(() => segments.GetPathString(), "segments");
        }

        [Fact]
        public void GetPathString_Returns_Path()
        {
            // Arrange & Act & Assert
            ODataPath path = new ODataPath(MetadataSegment.Instance);
            Assert.Equal("$metadata", path.GetPathString());

            // Arrange & Act & Assert
            IList<ODataPathSegment> segments = new List<ODataPathSegment>
            {
                MetadataSegment.Instance
            };
            Assert.Equal("$metadata", segments.GetPathString());
        }

        /// <summary>
        /// Test path segment used to test handling of unknown path segments.
        /// </summary>
        class UnknownTestODataPathSegment : ODataPathSegment
        {
            public override IEdmType EdmType => throw new NotImplementedException();

            public override void HandleWith(PathSegmentHandler handler)
            {
                handler.Handle(this);
            }

            public override T TranslateWith<T>(PathSegmentTranslator<T> translator)
            {
                throw new NotImplementedException();
            }
        }
    }
}
