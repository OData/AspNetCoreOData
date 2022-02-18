//-----------------------------------------------------------------------------
// <copyright file="EdmObjectExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmObjectExtensionsTests
    {
        [Fact]
        public void IsDeltaResourceSet_ThrowsArgumentNull_Type()
        {
            // Arrange & Act & Assert
            IEdmType edmType = null;
            ExceptionAssert.ThrowsArgumentNull(() => edmType.IsDeltaResourceSet(), "type");
        }

        [Fact]
        public void IsDeltaResource_ThrowsArgumentNull_Resource()
        {
            // Arrange & Act & Assert
            IEdmObject resource = null;
            ExceptionAssert.ThrowsArgumentNull(() => resource.IsDeltaResource(), "resource");
        }
    }
}
