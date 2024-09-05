//-----------------------------------------------------------------------------
// <copyright file="AutoSelectExpandHelperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm;

public class AutoSelectExpandHelperTests
{
    [Fact]
    public void HasAutoSelectProperty_ThrowsArgumentNull_ForParameters()
    {
        IEdmModel edmModel = null;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.HasAutoSelectProperty(null, null), "edmModel");

        edmModel = EdmCoreModel.Instance;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.HasAutoSelectProperty(null, null), "structuredType");
    }

    [Fact]
    public void HasAutoExpandProperty_ThrowsArgumentNull_ForParameters()
    {
        IEdmModel edmModel = null;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.HasAutoExpandProperty(null, null), "edmModel");

        edmModel = EdmCoreModel.Instance;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.HasAutoExpandProperty(null, null), "structuredType");
    }

    [Fact]
    public void GetAutoSelectPaths_ThrowsArgumentNull_ForParameters()
    {
        IEdmModel edmModel = null;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.GetAutoSelectPaths(null, null), "edmModel");

        edmModel = EdmCoreModel.Instance;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.GetAutoSelectPaths(null, null), "structuredType");
    }

    [Fact]
    public void GetAutoExpandPaths_ThrowsArgumentNull_ForParameters()
    {
        IEdmModel edmModel = null;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.GetAutoExpandPaths(null, null), "edmModel");

        edmModel = EdmCoreModel.Instance;
        ExceptionAssert.ThrowsArgumentNull(() => edmModel.GetAutoExpandPaths(null, null), "structuredType");
    }
}
