// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query
{
    public class SelectExpandIncludePropertyTest
    {
        [Fact]
        public void Constructor_ThrowsPropertySegmentArgumentNull_IfMissPropertySegment()
        {
            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new SelectExpandIncludedProperty(null, null),
                "propertySegment");
        }
    }
}
