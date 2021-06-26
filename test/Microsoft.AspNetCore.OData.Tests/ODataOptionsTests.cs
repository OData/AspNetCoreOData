// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
            Assert.Null(options.BuilderFactory);
            Assert.Equal(TimeZoneInfo.Local, options.TimeZone);
            Assert.True(options.RouteOptions.EnableKeyAsSegment);
            Assert.True(options.EnableNoDollarQueryOptions);

            // Act
            options.UrlKeyDelimiter = ODataUrlKeyDelimiter.Parentheses;
            options.EnableContinueOnErrorHeader = true;
            options.EnableAttributeRouting = false;
            options.BuilderFactory = () => null;
            options.TimeZone = TimeZoneInfo.Utc;
            options.RouteOptions.EnableKeyAsSegment = false;
            options.EnableNoDollarQueryOptions = false;

            // Act & Assert
            Assert.Equal(ODataUrlKeyDelimiter.Parentheses, options.UrlKeyDelimiter);
            Assert.True(options.EnableContinueOnErrorHeader);
            Assert.False(options.EnableAttributeRouting);
            Assert.NotNull(options.BuilderFactory);
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
        public void AddModel_WithoutOrWithPrefix_SetModel(string prefix)
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            if (prefix == null)
            {
                options.AddModel(edmModel);
            }
            else
            {
                options.AddModel(prefix, edmModel);
            }

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);

            Assert.Equal(prefix ?? String.Empty, model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
        }

        [Fact]
        public void AddModel_WithBatchHandler_SetModel()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;
            ODataBatchHandler handler = new Mock<ODataBatchHandler>().Object;

            // Act
            options.AddModel(edmModel, handler);

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);
            Assert.Equal(String.Empty, model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
            ODataBatchHandler actual = model.Value.Item2.GetService<ODataBatchHandler>();
            Assert.Same(handler, actual);
        }

        [Fact]
        public void AddModel_WithDependencyInjection_SetModelAndServices()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddModel("odata", edmModel, builder => builder.AddService<IODataFeature, ODataFeature>(Microsoft.OData.ServiceLifetime.Singleton));

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);
            Assert.Equal("odata", model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
            IODataFeature actual = model.Value.Item2.GetService<IODataFeature>();
            Assert.IsType<ODataFeature>(actual);
        }

        [Fact]
        public void AddModel_CanUse_CustomizedBuilderFactory()
        {
            // Arrange
            IServiceProvider sp = new ServiceCollection().BuildServiceProvider();
            IContainerBuilder builer = new MyInternalContainerBuilder(sp);
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;
            options.BuilderFactory = () => builer;

            // Act
            options.AddModel("odata", edmModel);

            // Assert
            KeyValuePair<string, (IEdmModel, IServiceProvider)> model = Assert.Single(options.RouteComponents);
            Assert.Equal("odata", model.Key);

            Assert.Same(edmModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
            Assert.Same(sp, model.Value.Item2);
        }

        [Fact]
        public void AddModel_Throws_IfBuilderFactoryReturnsNull()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;
            options.BuilderFactory = () => null;

            // Act
            Action test = () => options.AddModel("odata", edmModel);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test, "The container builder created by the container builder factory must not be null.");
        }

        [Fact]
        public void AddModel_Throws_IfModelNull()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => options.AddModel("odata", null, builder => { }), "model");
        }

        [Fact]
        public void AddModel_Throws_IfPrefixExisted()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;
            options.AddModel("odata", edmModel);

            // Act
            Action test = () => options.AddModel("odata", edmModel);

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(test, "The prefix 'odata' was already used for other Edm model.");

        }
        #endregion

        [Fact]
        public void GetODataServiceProvider_ReturnsNull()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            Assert.Null(options.GetODataServiceProvider(null));
        }

        [Fact]
        public void GetODataServiceProvider_ReturnsCorrectServiceProvider()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            IEdmModel edmModel = EdmCoreModel.Instance;

            // Act
            options.AddModel("odata", edmModel);

            // & Assert
            IServiceProvider sp = options.GetODataServiceProvider("odata");
            Assert.NotNull(sp);
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
            Assert.Equal(0, options.QuerySettings.MaxTop); // Guard

            // Act
            options.SetMaxTop(2);

            // Assert
            Assert.Equal(2, options.QuerySettings.MaxTop.Value);
        }

        [Fact]
        public void Expand_SetExpand()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QuerySettings.EnableExpand); // Guard

            // Act
            options.Expand();

            // Assert
            Assert.True(options.QuerySettings.EnableExpand);
        }

        [Fact]
        public void Select_SetSelect()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QuerySettings.EnableSelect); // Guard

            // Act
            options.Select();

            // Assert
            Assert.True(options.QuerySettings.EnableSelect);
        }

        [Fact]
        public void Filter_SetFilter()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QuerySettings.EnableFilter); // Guard

            // Act
            options.Filter();

            // Assert
            Assert.True(options.QuerySettings.EnableFilter);
        }

        [Fact]
        public void OrderBy_SetOrderBy()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QuerySettings.EnableOrderBy); // Guard

            // Act
            options.OrderBy();

            // Assert
            Assert.True(options.QuerySettings.EnableOrderBy);
        }

        [Fact]
        public void Count_SetCount()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QuerySettings.EnableCount); // Guard

            // Act
            options.Count();

            // Assert
            Assert.True(options.QuerySettings.EnableCount);
        }

        [Fact]
        public void SkipToken_SetSkipToken()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            Assert.False(options.QuerySettings.EnableSkipToken); // Guard

            // Act
            options.SkipToken();

            // Assert
            Assert.True(options.QuerySettings.EnableSkipToken);
        }
        #endregion

        private class MyInternalContainerBuilder : IContainerBuilder
        {
            private IServiceProvider _sp;

            public MyInternalContainerBuilder(IServiceProvider sp)
            {
                _sp = sp;
            }

            public IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime, Type serviceType, Type implementationType)
            {
                return null;
            }

            public IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime, Type serviceType, Func<IServiceProvider, object> implementationFactory)
            {
                return null;
            }

            public IServiceProvider BuildContainer()
            {
                return _sp;
            }
        }
    }
}
