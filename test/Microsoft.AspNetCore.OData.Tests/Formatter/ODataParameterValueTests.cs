//-----------------------------------------------------------------------------
// <copyright file="ODataParameterValueTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter;

public class ODataParameterValueTests
{
    [Fact]
    public void Ctor_ThrowsArgumentNull_ParameterType()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new ODataParameterValue(null, null), "paramType");
    }
}
