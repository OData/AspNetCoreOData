// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public  class ODataTemplateTranslateContextTests
    {
        [Theory]
        [InlineData("@p", "[1, 2, null, 7, 8]")]
        [InlineData("@p1", null)]
        public void GetQueryStringWorksAsExpected(string key, string expected)
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            var request = context.Request;
            var requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext(context);

            requestFeature.QueryString = "?@p=[1, 2, null, 7, 8]";

            // Act
            StringValues alias = translateContext.GetQueryString(key);

            // Assert
            if (expected == null)
            {
                Assert.Empty(alias);
            }
            else
            {
                string value = Assert.Single(alias);
                Assert.Equal(expected, value);
            }
        }

        [Fact]
        public void GetQueryStringWorksForMulitipleAliasAsExpected()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            var request = context.Request;
            var requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext(context);

            requestFeature.QueryString = "?@p=@p1&@p1='ab''c'";

            // #1: Act
            StringValues alias = translateContext.GetQueryString("@p");

            // #1: Assert
            string value = Assert.Single(alias);
            Assert.Equal("@p1", value);

            // #2: Act
            alias = translateContext.GetQueryString(value);

            // #2: Assert
            value = Assert.Single(alias);
            Assert.Equal("'ab''c'", value);
        }
    }
}
