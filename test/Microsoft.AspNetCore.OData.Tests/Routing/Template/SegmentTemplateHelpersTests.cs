// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class SegmentTemplateHelpersTests
    {
        #region BuildRouteKey
        [Fact]
        public void BuildRouteKey_ThrowsArgumentNull()
        {
            // Arrange
            IDictionary<string, string> parameterMappings = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => parameterMappings.BuildRouteKey(), "parameterMappings");
        }

        [Fact]
        public void BuildRouteKey_ReturnsCorrectlyRouteKeyString_ForEmptyParameterMapping()
        {
            // Arrange
            IDictionary<string, string> parameterMappings = new Dictionary<string, string>();

            // Act
            string routeKey = parameterMappings.BuildRouteKey();

            // Assert
            Assert.Equal("", routeKey);
        }

        [Fact]
        public void BuildRouteKey_ReturnsCorrectlyRouteKeyString()
        {
            // Arrange
            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "organizationId", "orgId" },
                { "partmentId", "parId" }
            };

            // Act
            string routeKey = parameterMappings.BuildRouteKey();

            // Assert
            Assert.Equal("orgId;parId", routeKey);
        }
        #endregion

        #region TryParseRouteKey
        [Fact]
        public void TryParseRouteKey_WorksForSingleKey()
        {
            // Assert
            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "key", " '10001' " }
            };
            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "Id", "key" },
            };
            RouteValueDictionary updateValues = new RouteValueDictionary();

            // Act
            bool actual = SegmentTemplateHelpers.TryParseRouteKey(routeValues, updateValues, parameterMappings);

            // Assert
            Assert.True(actual);
            KeyValuePair<string, object> updateValue = Assert.Single(updateValues);
            Assert.Equal("key", updateValue.Key);
            Assert.Equal("'10001'", updateValue.Value);
        }

        [Fact]
        public void TryParseRouteKey_MultipleKeyValues()
        {
            // Assert
            RouteValueDictionary routeValues = new RouteValueDictionary()
            {
                { "orgId;partId", "organizationId=  '10001' ,  partmentId  = 1234 " }
            };
            IDictionary<string, string> parameterMappings = new Dictionary<string, string>
            {
                { "organizationId", "orgId" },
                { "partmentId", "partId" },
            };
            RouteValueDictionary updateValues = new RouteValueDictionary();

            // Act
            bool actual = SegmentTemplateHelpers.TryParseRouteKey(routeValues, updateValues, parameterMappings);

            // Assert
            Assert.True(actual);
            Assert.Equal(2, updateValues.Count);
            Assert.Equal("'10001'", updateValues["orgId"]);
            Assert.Equal("1234", updateValues["partId"]);
        }
        #endregion
    }
}
