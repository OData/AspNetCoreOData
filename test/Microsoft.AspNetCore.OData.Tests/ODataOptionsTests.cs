// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataOptionsTests
    {
        [Fact]
        public void SetMaxTop_Throws_ForWrongValue()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => options.SetMaxTop(-2), "Value must be greater than or equal to 0.");
        }
    }
}
