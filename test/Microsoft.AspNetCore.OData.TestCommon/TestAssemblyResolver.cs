//-----------------------------------------------------------------------------
// <copyright file="TestAssemblyResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    internal class TestAssemblyResolver : IAssemblyResolver
    {
        List<Assembly> _assemblies;

        public TestAssemblyResolver(MockAssembly assembly)
        {
            _assemblies = new List<Assembly>();
            _assemblies.Add(assembly);
        }

        public TestAssemblyResolver(params Type[] types)
            : this(new MockAssembly(types))
        {
        }

        public IEnumerable<Assembly> Assemblies => _assemblies;
    }
}
