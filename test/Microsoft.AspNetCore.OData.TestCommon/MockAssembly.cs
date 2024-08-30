//-----------------------------------------------------------------------------
// <copyright file="MockAssembly.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.OData.TestCommon;

/// <summary>
/// A mock to represent an assembly
/// </summary>
public sealed class MockAssembly : Assembly
{
    private Type[] _types;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockAssembly"/> class.
    /// </summary>
    /// <param name="types">The types in this assembly.</param>
    public MockAssembly(params Type[] types)
    {
        _types = types;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockAssembly"/> class.
    /// </summary>
    /// <param name="types">The mock types in this assembly.</param>
    public MockAssembly(params MockType[] types)
    {
        foreach (var type in types)
        {
            type.SetupGet(t => t.Assembly).Returns(this);
        }

        _types = types.Select(t => t.Object).ToArray();
    }

    /// <remarks>
    /// AspNet uses GetTypes as opposed to DefinedTypes()
    /// </remarks>
    public override Type[] GetTypes()
    {
        return _types;
    }

    /// <remarks>
    /// AspNetCore uses DefinedTypes as opposed to GetTypes()
    /// </remarks>
    public override IEnumerable<TypeInfo> DefinedTypes
    {
        get { return _types.AsEnumerable().Select(a => a.GetTypeInfo()); }
    }
}
