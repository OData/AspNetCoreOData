// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Formatter;
using System;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataInputFormatterTests
    {
        [Fact]
        public void ConstructorThrowsIfPayloadsIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>("payloadKinds", () => new ODataInputFormatter(payloadKinds: null));
        }
    }
}