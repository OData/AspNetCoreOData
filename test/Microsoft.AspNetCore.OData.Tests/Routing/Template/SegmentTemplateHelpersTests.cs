// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class SegmentTemplateHelpersTests
    {
        #region TryParseRouteKey
        //[Fact]
        //public void TryParseRouteKey_WorksForSingleKey()
        //{
        //    // Assert
        //    ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

        //    RouteValueDictionary routeValues = new RouteValueDictionary()
        //    {
        //        { "key", "'10001'" }
        //    };
        //    IDictionary<string, string> parameterMappings = new Dictionary<string, string>
        //    {
        //        { "Id", "key" },
        //    };
        //    RouteValueDictionary updateValues = new RouteValueDictionary();


        //    // Act
        //    bool actual = SegmentTemplateHelpers.Match(routeValues, updateValues, parameterMappings);

        //    // Assert
        //    Assert.True(actual);
        //    KeyValuePair<string, object> updateValue = Assert.Single(updateValues);
        //    Assert.Equal("key", updateValue.Key);
        //    Assert.Equal("'10001'", updateValue.Value);
        //}

        //[Fact]
        //public void TryParseRouteKey_MultipleKeyValues()
        //{
        //    // Assert
        //    RouteValueDictionary routeValues = new RouteValueDictionary()
        //    {
        //        { "orgId;partId", "organizationId=  '10001' ,  partmentId  = 1234 " }
        //    };
        //    IDictionary<string, string> parameterMappings = new Dictionary<string, string>
        //    {
        //        { "organizationId", "orgId" },
        //        { "partmentId", "partId" },
        //    };
        //    RouteValueDictionary updateValues = new RouteValueDictionary();

        //    // Act
        //    bool actual = SegmentTemplateHelpers.TryParseRouteKey(routeValues, updateValues, parameterMappings);

        //    // Assert
        //    Assert.True(actual);
        //    Assert.Equal(2, updateValues.Count);
        //    Assert.Equal("'10001'", updateValues["orgId"]);
        //    Assert.Equal("1234", updateValues["partId"]);
        //}
        #endregion
    }
}
