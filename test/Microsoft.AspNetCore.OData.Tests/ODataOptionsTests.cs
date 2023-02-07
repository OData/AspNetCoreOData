//-----------------------------------------------------------------------------
// <copyright file="ODataOptionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataOptionsTests
    {
        [Fact]
        public void PropertySetting_SetsCorrectValue()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // verify default
            Assert.Equal(ODataUrlKeyDelimiter.Slash, options.UrlKeyDelimiter); // Guard
            Assert.False(options.EnableContinueOnErrorHeader);
            Assert.True(options.EnableAttributeRouting);
            Assert.Equal(TimeZoneInfo.Local, options.TimeZone);
            Assert.True(options.RouteOptions.EnableKeyAsSegment);
            Assert.True(options.EnableNoDollarQueryOptions);

            // Act
            options.UrlKeyDelimiter = ODataUrlKeyDelimiter.Parentheses;
            options.EnableContinueOnErrorHeader = true;
            options.EnableAttributeRouting = false;
            options.TimeZone = TimeZoneInfo.Utc;
            options.RouteOptions.EnableKeyAsSegment = false;
            options.EnableNoDollarQueryOptions = false;

            // Act & Assert
            Assert.Equal(ODataUrlKeyDelimiter.Parentheses, options.UrlKeyDelimiter);
            Assert.True(options.EnableContinueOnErrorHeader);
            Assert.False(options.EnableAttributeRouting);
            Assert.Equal(TimeZoneInfo.Utc, options.TimeZone);
            Assert.False(options.RouteOptions.EnableKeyAsSegment);
            Assert.False(options.EnableNoDollarQueryOptions);

            Assert.Empty(options.RouteComponents);
        }

        [Fact]
        public void Conventions_InsertsConvention()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.Empty(options.Conventions); // Guard
            Mock<IODataControllerActionConvention> mock = new Mock<IODataControllerActionConvention>();

            // Act
            options.Conventions.Add(mock.Object);

            // & Assert
            IODataControllerActionConvention convention = Assert.Single(options.Conventions);
            Assert.Same(mock.Object, convention);
        }

        #region AddModel
        [Theory]
        [InlineData(null)]
        [InlineData("odata")]
        public void AddRouteComponents_WithoutOrWithPrefix_SetModel(string prefix)
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            if (prefix == null)
            {
                options.AddRouteComponents(edmModel);
            }
            else
            {
                options.AddRouteComponents(prefix, edmModel);
            }

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);

            Assert.Equal(prefix ?? String.Empty, model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
        }

        [Fact]
        public void AddRouteComponents_WithBatchHandler_SetModel()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;
            ODataBatchHandler handler = new Mock<ODataBatchHandler>().Object;

            // Act
            options.AddRouteComponents(edmModel, handler);

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);
            Assert.Equal(String.Empty, model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
            ODataBatchHandler actual = model.Value.Item2.GetService<ODataBatchHandler>();
            Assert.Same(handler, actual);
        }

        [Fact]
        public void AddRouteComponents_WithDependencyInjection_SetModelAndServices()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddRouteComponents("odata", edmModel, services => services.AddSingleton<IODataFeature, ODataFeature>());

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);
            Assert.Equal("odata", model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
            IODataFeature actual = model.Value.Item2.GetService<IODataFeature>();
            Assert.IsType<ODataFeature>(actual);
        }

        [Theory]
        [InlineData(ODataVersion.V4, true)]
        [InlineData(ODataVersion.V401, true)]
        public void AddRouteComponents_WithVersionAndDependencyInjection_SetModelAndServices(ODataVersion version, bool readingODataPrefixSetting)
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddRouteComponents("odata", edmModel, version, null);

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);
            Assert.Equal("odata", model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
            ODataSimplifiedOptions actual = model.Value.Item2.GetService<ODataSimplifiedOptions>();
            Assert.Equal(readingODataPrefixSetting, actual.EnableReadingODataAnnotationWithoutPrefix);
        }

        [Theory]
        [InlineData("/odata", "odata")]
        [InlineData("/odata/", "odata")]
        [InlineData("odata/", "odata")]
        [InlineData("/", "")]
        public void AddRouteComponents_Strips_RoutePrefix_Leading_And_Trailing_Slashes(string routePrefix, string expectedRoutePrefix)
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddRouteComponents(routePrefix, edmModel, services => services.AddSingleton<IODataFeature, ODataFeature>());

            // Assert
            Assert.False(options.RouteComponents.ContainsKey(routePrefix));
            Assert.True(options.RouteComponents.ContainsKey(expectedRoutePrefix));
        }

        [Fact]
        public void AddRouteComponents_Throws_IfModelNull()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => options.AddRouteComponents("odata", null, builder => { }), "model");
        }

        [Fact]
        public void AddRouteComponents_Throws_IfRoutePrefixNull()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => options.AddRouteComponents(null, EdmCoreModel.Instance, builder => { }), "routePrefix");
        }

        [Fact]
        public void AddRouteComponents_Throws_IfPrefixExisted()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;
            options.AddRouteComponents("odata", edmModel);

            // Act
            Action test = () => options.AddRouteComponents("odata", edmModel);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test, "The prefix 'odata' was already used for other Edm model.");

        }
        #endregion

        [Fact]
        public void GetRouteServices_ReturnsNull()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            Assert.Null(options.GetRouteServices(null));
        }

        [Fact]
        public void GetRouteServices_ReturnsCorrectServiceProvider()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddRouteComponents("odata", edmModel);

            // & Assert
            IServiceProvider sp = options.GetRouteServices("odata");
            Assert.NotNull(sp);
        }

        [Theory]
        [InlineData("/odata")]
        [InlineData("/odata/")]
        [InlineData("odata/")]
        public void GetRouteServices_ReturnsCorrectServiceProvider_When_Leading_Or_Trailing_Slashes(string routePrefix)
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddRouteComponents(routePrefix, edmModel);

            // & Assert
            // can retrieve service provider using original routePrefix
            IServiceProvider sp = options.GetRouteServices(routePrefix);
            Assert.NotNull(sp);

            // can retrieve service provider using sanitized routePrefix
            string sanitizedRoutePrefix = "odata";
            IServiceProvider sp2 = options.GetRouteServices(sanitizedRoutePrefix);
            Assert.NotNull(sp2);
        }

        #region QuerySetting
        [Fact]
        public void SetMaxTop_Throws_ForWrongValue()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => options.SetMaxTop(-2));
            Assert.Contains("Value must be greater than or equal to 0", exception.Message);
        }

        [Fact]
        public void SetMaxTop_SetMaxTopValue()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.Equal(0, options.QueryConfigurations.MaxTop); // Guard

            // Act
            options.SetMaxTop(2);

            // Assert
            Assert.Equal(2, options.QueryConfigurations.MaxTop.Value);
        }

        [Fact]
        public void Expand_SetExpand()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QueryConfigurations.EnableExpand); // Guard

            // Act
            options.Expand();

            // Assert
            Assert.True(options.QueryConfigurations.EnableExpand);
        }

        [Fact]
        public void Select_SetSelect()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QueryConfigurations.EnableSelect); // Guard

            // Act
            options.Select();

            // Assert
            Assert.True(options.QueryConfigurations.EnableSelect);
        }

        [Fact]
        public void Filter_SetFilter()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QueryConfigurations.EnableFilter); // Guard

            // Act
            options.Filter();

            // Assert
            Assert.True(options.QueryConfigurations.EnableFilter);
        }

        [Fact]
        public void OrderBy_SetOrderBy()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QueryConfigurations.EnableOrderBy); // Guard

            // Act
            options.OrderBy();

            // Assert
            Assert.True(options.QueryConfigurations.EnableOrderBy);
        }

        [Fact]
        public void Count_SetCount()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QueryConfigurations.EnableCount); // Guard

            // Act
            options.Count();

            // Assert
            Assert.True(options.QueryConfigurations.EnableCount);
        }

        [Fact]
        public void SkipToken_SetSkipToken()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QueryConfigurations.EnableSkipToken); // Guard

            // Act
            options.SkipToken();

            // Assert
            Assert.True(options.QueryConfigurations.EnableSkipToken);
        }
        #endregion

    }

}
