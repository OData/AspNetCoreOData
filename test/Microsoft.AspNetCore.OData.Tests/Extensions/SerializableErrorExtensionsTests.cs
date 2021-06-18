// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class SerializableErrorExtensionsTests
    {
        [Fact]
        public void CreateODataError_ThrowsArgumentNull_SerializableError()
        {
            // Arrange & Act & Assert
            SerializableError serializableError = null;
            ExceptionAssert.ThrowsArgumentNull(() => SerializableErrorExtensions.CreateODataError(serializableError), "serializableError");
        }

        [Fact]
        public void CreateODataError_Creates_ODataError_UsingModelStateDictionary()
        {
            // Arrange & Act & Assert
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("key1", "Test Error 1");
            modelState.AddModelError("key1", "Test Error 2");
            modelState.AddModelError("key3", "Test Error 3");
            SerializableError serializableError = new SerializableError(modelState);

            // Act
            ODataError error = SerializableErrorExtensions.CreateODataError(serializableError);

            // Assert
            Assert.NotNull(error);
            Assert.Equal("key1:\r\nTest Error 1\r\nTest Error 2\r\n\r\nkey3:\r\nTest Error 3", error.Message);
            Assert.Null(error.ErrorCode);
            Assert.Null(error.InnerError);
            Assert.Equal(3, error.Details.Count);
        }

        [Fact]
        public void CreateODataError_Creates_BasicODataError_WithoutModelStateDictionary()
        {
            // Arrange & Act & Assert
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("key3", "Test Error 3");
            SerializableError innerSerializableError = new SerializableError(modelState);

            SerializableError serializableError = new SerializableError();
            serializableError["key1"] = "Test Error 1";
            serializableError["key2"] = "Test Error 2";
            serializableError["ModelState"] = innerSerializableError;

            // Act
            ODataError error = SerializableErrorExtensions.CreateODataError(serializableError);

            // Assert
            Assert.NotNull(error);
            Assert.Equal("key1:\r\nTest Error 1\r\n\r\nkey2:\r\nTest Error 2", error.Message);
            Assert.Null(error.ErrorCode);
            Assert.Equal("key3:\r\nTest Error 3", error.InnerError.Message);
            Assert.Equal(2, error.Details.Count);
        }

        [Fact]
        public void CreateODataError_Creates_AdvancedODataError()
        {
            // Arrange & Act & Assert
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("key1", "Test Error 1");
            SerializableError serializableError = new SerializableError(modelState);
            serializableError["ErrorCode"] = "Error Code 1";
            serializableError["Message"] = "Error Message 1";
            serializableError["ExceptionMessage"] = "Error ExceptionMessage 1";

            // Act
            ODataError error = SerializableErrorExtensions.CreateODataError(serializableError);

            // Assert
            Assert.NotNull(error);
            Assert.Equal("Error Message 1", error.Message);
            Assert.Equal("Error Code 1", error.ErrorCode);
            Assert.Equal("Error ExceptionMessage 1", error.InnerError.Message);
            Assert.Single(error.Details);
        }
    }
}