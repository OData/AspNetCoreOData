//-----------------------------------------------------------------------------
// <copyright file="TestAssemblyResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class TestAssemblyResolver : IAssemblyResolver
    {
        public static TestAssemblyResolver Instance = new TestAssemblyResolver();

        public IEnumerable<Assembly> Assemblies => new[] { typeof(TestAssemblyResolver).Assembly };
    }
}
