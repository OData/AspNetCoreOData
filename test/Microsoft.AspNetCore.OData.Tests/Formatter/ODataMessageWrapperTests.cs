//-----------------------------------------------------------------------------
// <copyright file="ODataMessageWrapperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataMessageWrapperTests
    {
        [Fact]
        public void DefaultCtor_SetsProperties()
        {
            // Arrange & Act & Assert
            ODataMessageWrapper wrapper = new ODataMessageWrapper();
            Assert.Null(wrapper.GetStream());
            Assert.NotNull(wrapper.Headers);
        }

        [Fact]
        public void Url_ThrowsNotImplementException()
        {
            // Arrange & Act & Assert
            ODataMessageWrapper wrapper = new ODataMessageWrapper();
            Assert.Throws<NotImplementedException>(() => wrapper.Url = new Uri("http://any"));
            Assert.Throws<NotImplementedException>(() => wrapper.Url);
        }

        [Fact]
        public void Method_ThrowsNotImplementException()
        {
            // Arrange & Act & Assert
            ODataMessageWrapper wrapper = new ODataMessageWrapper();
            Assert.Throws<NotImplementedException>(() => wrapper.Method = "GET");
            Assert.Throws<NotImplementedException>(() => wrapper.Method);
        }

        [Fact]
        public void StatusCode_ThrowsNotImplementException()
        {
            // Arrange & Act & Assert
            ODataMessageWrapper wrapper = new ODataMessageWrapper();
            Assert.Throws<NotImplementedException>(() => wrapper.StatusCode = 200);
            Assert.Throws<NotImplementedException>(() => wrapper.StatusCode);
        }

        [Fact]
        public void GetHeader_ReturnsHead_NoMatterCaseSensitive()
        {
            // Arrange & Act & Assert
            ODataMessageWrapper wrapper = new ODataMessageWrapper();
            wrapper.SetHeader("MyHead", "HeadValue");

            Assert.Equal("HeadValue", wrapper.GetHeader("MyHead"));
            Assert.Equal("HeadValue", wrapper.GetHeader("myhead"));
        }

        [Fact]
        public void ConvertPayloadUri_ThrowsArgumentNull_PayloadUri()
        {
            // Arrange
            ODataMessageWrapper wrapper = new ODataMessageWrapper();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => wrapper.ConvertPayloadUri(null, null), "payloadUri");
        }
    }
}
