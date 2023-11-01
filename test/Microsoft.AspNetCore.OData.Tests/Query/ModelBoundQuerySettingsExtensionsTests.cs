//-----------------------------------------------------------------------------
// <copyright file="ModelBoundQuerySettingsExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Config;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class ModelBoundQuerySettingsExtensionsTests
    {
        [Fact]
        public void CopyOrderByConfigurations_Copies_OrderBy()
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            Assert.Empty(settings.OrderByConfigurations);

            // Act
            settings.CopyOrderByConfigurations(new Dictionary<string, bool> { { "any", true } });

            // Assert
            Assert.NotEmpty(settings.OrderByConfigurations);
            KeyValuePair<string, bool> item = Assert.Single(settings.OrderByConfigurations);
            Assert.Equal("any", item.Key);
            Assert.True(item.Value);
        }

        [Fact]
        public void CopySelectConfigurations_Copies_SelectExpand()
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            Assert.Empty(settings.SelectConfigurations);

            // Act
            settings.CopySelectConfigurations(new Dictionary<string, SelectExpandType> { { "any", SelectExpandType.Disabled } });

            // Assert
            Assert.NotEmpty(settings.SelectConfigurations);
            KeyValuePair<string, SelectExpandType> item = Assert.Single(settings.SelectConfigurations);
            Assert.Equal("any", item.Key);
            Assert.Equal(SelectExpandType.Disabled, item.Value);
        }

        [Fact]
        public void CopyFilterConfigurations_Copies_Filter()
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            Assert.Empty(settings.FilterConfigurations);

            // Act
            settings.CopyFilterConfigurations(new Dictionary<string, bool> { { "any", false } });

            // Assert
            Assert.NotEmpty(settings.FilterConfigurations);
            KeyValuePair<string, bool> item = Assert.Single(settings.FilterConfigurations);
            Assert.Equal("any", item.Key);
            Assert.False(item.Value);
        }

        [Theory]
        [InlineData(SelectExpandType.Disabled, false)]
        [InlineData(SelectExpandType.Allowed, false)]
        [InlineData(SelectExpandType.Automatic, true)]
        public void IsAutomaticExpand_ReturnsCorrectly(SelectExpandType expandType, bool expected)
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            settings.ExpandConfigurations["navProperty"] = new ExpandConfiguration { ExpandType = expandType };

            // Act & Assert
            Assert.Equal(expected, settings.IsAutomaticExpand("navProperty"));
        }

        [Theory]
        [InlineData(SelectExpandType.Disabled, false)]
        [InlineData(SelectExpandType.Allowed, false)]
        [InlineData(SelectExpandType.Automatic, true)]
        public void IsAutomaticSelect_ReturnsCorrectly(SelectExpandType expandType, bool expected)
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            settings.SelectConfigurations["property"] = expandType;

            // Act & Assert
            Assert.Equal(expected, settings.IsAutomaticSelect("property"));
        }

        [Theory]
        [InlineData(SelectExpandType.Disabled, false)]
        [InlineData(SelectExpandType.Allowed, true)]
        [InlineData(SelectExpandType.Automatic, true)]
        public void Selectable_ReturnsCorrectly_UsingDefaultConfiguration(SelectExpandType selectType, bool expected)
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            Assert.Empty(settings.SelectConfigurations);
            settings.DefaultSelectType = selectType;

            // Act & Assert
            Assert.Equal(expected, settings.Selectable("property"));
        }

        [Theory]
        [InlineData(SelectExpandType.Disabled, false)]
        [InlineData(SelectExpandType.Allowed, true)]
        [InlineData(SelectExpandType.Automatic, true)]
        public void Selectable_ReturnsCorrectly(SelectExpandType selectType, bool expected)
        {
            // Arrange
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            settings.SelectConfigurations["property"] = selectType;

            // Act & Assert
            Assert.Equal(expected, settings.Selectable("property"));
        }

        [Fact]
        public void Sortable_ReturnsCorrectly()
        {
            // Arrange & Act & Assert
            ModelBoundQuerySettings settings = new ModelBoundQuerySettings();
            Assert.Empty(settings.OrderByConfigurations);
            Assert.False(settings.Sortable("property"));

            // Arrange & Act & Assert
            settings.DefaultEnableOrderBy = true;
            Assert.True(settings.Sortable("property"));

            // Arrange & Act & Assert
            settings.OrderByConfigurations["property"] = false;
            Assert.False(settings.Sortable("property"));

            // Arrange & Act & Assert
            settings.OrderByConfigurations["property"] = true;
            Assert.True(settings.Sortable("property"));
        }
    }
}
