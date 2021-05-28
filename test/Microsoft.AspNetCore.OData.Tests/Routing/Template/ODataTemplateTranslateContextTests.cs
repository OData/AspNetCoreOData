// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public  class ODataTemplateTranslateContextTests
    {
        [Fact]
        public void GetParameterAliasOrSelf_ReturnsExpectedAliasValue()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            requestFeature.QueryString = "?@p=[1, 2, null, 7, 8]";

            ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
            {
                HttpContext = context
            };

            // Act
            string alias = translateContext.GetParameterAliasOrSelf("@p");

            // Assert
            Assert.Equal("[1, 2, null, 7, 8]", alias);
        }

        [Fact]
        public void GetParameterAliasOrSelf_ReturnsExpectedAliasValue_ForMultiple()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            requestFeature.QueryString = "?@p=@age&@age=@para1&@para1='ab''c'";

            ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
            {
                HttpContext = context
            };

            // Act
            string alias = translateContext.GetParameterAliasOrSelf("@p");

            // Assert
            Assert.Equal("'ab''c'", alias);
        }

        [Fact]
        public void GetParameterAliasOrSelf_Throws_ForInfiniteLoopParameterAlias()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            requestFeature.QueryString = "?@p=@age&@age=@p1&@p1=@p";

            ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
            {
                HttpContext = context
            };

            // Act
            Action test = () => translateContext.GetParameterAliasOrSelf("@p");

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "The parameter alias '@p' is in an infinite loop.");
        }

        [Fact]
        public void GetParameterAliasOrSelf_Throws_ForMissingParameterAlias()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            IHttpRequestFeature requestFeature = request.HttpContext.Features.Get<IHttpRequestFeature>();
            requestFeature.QueryString = "?@p=@age&@para1='abc'";

            ODataTemplateTranslateContext translateContext = new ODataTemplateTranslateContext
            {
                HttpContext = context
            };

            // Act
            Action test = () => translateContext.GetParameterAliasOrSelf("@p");

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "Missing the parameter alias '@age' in the request query string.");
        }
    }
}
