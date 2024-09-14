//-----------------------------------------------------------------------------
// <copyright file="AssemblyResolverHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Abstracts;

internal static class AssemblyResolverHelper
{
    public static IAssemblyResolver Default = new DefaultAssemblyResolver();
}
