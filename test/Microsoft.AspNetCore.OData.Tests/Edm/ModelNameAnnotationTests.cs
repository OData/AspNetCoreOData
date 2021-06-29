// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class ModelNameAnnotationTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Name()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ModelNameAnnotation(null), "name");
        }

        [Fact]
        public void Ctor_SetsProperties()
        {
            // Arrange & Act & Assert
            ModelNameAnnotation annotation = new ModelNameAnnotation("any");

            Assert.Equal("any", annotation.ModelName);
        }
    }
}
