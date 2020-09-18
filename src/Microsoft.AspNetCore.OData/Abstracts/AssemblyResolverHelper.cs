// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    internal static class AssemblyResolverHelper
    {
        public static IAssemblyResolver Default = new DefaultAssemblyResolver();
    }
}
