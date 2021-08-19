//-----------------------------------------------------------------------------
// <copyright file="ODataRouteOptionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing
{
    public class ODataRouteOptionsTests
    {
        [Fact]
        public void DefaultODataRouteOptions_HasDefaultProperties()
        {
            // Arrange & Act
            ODataRouteOptions options = ODataRouteOptions.Default;

            // Assert
            Assert.True(options.EnableKeyInParenthesis);
            Assert.True(options.EnableKeyAsSegment);
            Assert.True(options.EnableQualifiedOperationCall);
            Assert.True(options.EnableUnqualifiedOperationCall);
            Assert.False(options.EnableNonParenthesisForEmptyParameterFunction);
            Assert.False(options.EnableControllerNameCaseInsensitive);
        }

        [Fact]
        public void CtorODataRouteOptions_HasDefaultProperties()
        {
            // Arrange & Act
            ODataRouteOptions options = new ODataRouteOptions();

            // Assert
            Assert.True(options.EnableKeyInParenthesis);
            Assert.True(options.EnableKeyAsSegment);
            Assert.True(options.EnableQualifiedOperationCall);
            Assert.True(options.EnableUnqualifiedOperationCall);
            Assert.False(options.EnableNonParenthesisForEmptyParameterFunction);
            Assert.False(options.EnableControllerNameCaseInsensitive);
        }

        [Fact]
        public void ConfigProperties_WorksForEachProperty()
        {
            Verify(opt => opt.EnableKeyInParenthesis, (opt, b) => opt.EnableKeyInParenthesis = b);
            Verify(opt => opt.EnableKeyAsSegment, (opt, b) => opt.EnableKeyAsSegment = b);
            Verify(opt => opt.EnableQualifiedOperationCall, (opt, b) => opt.EnableQualifiedOperationCall = b);
            Verify(opt => opt.EnableUnqualifiedOperationCall, (opt, b) => opt.EnableUnqualifiedOperationCall = b);
            Verify(opt => opt.EnableNonParenthesisForEmptyParameterFunction, (opt, b) => opt.EnableNonParenthesisForEmptyParameterFunction = b, false);
            Verify(opt => opt.EnableControllerNameCaseInsensitive, (opt, b) => opt.EnableControllerNameCaseInsensitive = b, false);
        }

        private static void Verify(Func<ODataRouteOptions, bool> func, Action<ODataRouteOptions, bool> config, bool defValue = true)
        {
            // Arrange
            ODataRouteOptions options = new ODataRouteOptions();
            Assert.Equal(defValue, func(options));

            // Act
            config(options, !defValue);

            // Assert
            Assert.Equal(!defValue, func(options));
        }

        [Fact]
        public void ConfigKeyOptions_DoesNotThrowsODataException()
        {
            // Arrange & Act & Assert
            ODataRouteOptions options = new ODataRouteOptions();
            options.EnableKeyAsSegment = false;
            options.EnableKeyInParenthesis = true;

            // Arrange & Act & Assert
            options = new ODataRouteOptions();
            options.EnableKeyInParenthesis = false;
            options.EnableKeyAsSegment = true;
        }

        [Fact]
        public void ConfigKeyOptions_ThrowsODataException()
        {
            // Arrange
            string expect = "The route option disables key in parenthesis and key as segment. At least one option should enable.";

            // Act & Assert
            Action test = () =>
            {
                ODataRouteOptions options = new ODataRouteOptions();
                options.EnableKeyAsSegment = false;
                options.EnableKeyInParenthesis = false;
            };

            ExceptionAssert.Throws<ODataException>(test, expect);

            // Act & Assert
            test = () =>
            {
                ODataRouteOptions options = new ODataRouteOptions();
                options.EnableKeyInParenthesis = false;
                options.EnableKeyAsSegment = false;
            };

            ExceptionAssert.Throws<ODataException>(test, expect);
        }

        [Fact]
        public void ConfigOperationOptions_DoesNotThrowsODataException()
        {
            // Arrange & Act & Assert
            ODataRouteOptions options = new ODataRouteOptions();
            options.EnableQualifiedOperationCall = false;
            options.EnableUnqualifiedOperationCall = true;

            // Arrange & Act & Assert
            options = new ODataRouteOptions();
            options.EnableUnqualifiedOperationCall = false;
            options.EnableQualifiedOperationCall = true;
        }

        [Fact]
        public void ConfigOperationOptions_ThrowsODataException()
        {
            // Arrange
            string expect = "The route option disables qualified and unqualified operation call. At least one option should enable.";

            // Act & Assert
            Action test = () =>
            {
                ODataRouteOptions options = new ODataRouteOptions();
                options.EnableQualifiedOperationCall = false;
                options.EnableUnqualifiedOperationCall = false;
            };

            ExceptionAssert.Throws<ODataException>(test, expect);

            // Act & Assert
            test = () =>
            {
                ODataRouteOptions options = new ODataRouteOptions();
                options.EnableUnqualifiedOperationCall = false;
                options.EnableQualifiedOperationCall = false;
            };

            ExceptionAssert.Throws<ODataException>(test, expect);
        }
    }
}
