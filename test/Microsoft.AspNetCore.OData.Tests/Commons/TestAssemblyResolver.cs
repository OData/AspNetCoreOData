// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Abstracts;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class TestAssemblyResolver : IAssemblyResolver
    {
        public IEnumerable<Assembly> Assemblies => new[] { typeof(TestAssemblyResolver).Assembly };
    }
}
