// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// 
    /// </summary>
    internal class DefaultAssemblyResolver : IAssemblyResolver
    {
        private Assembly[] _assemblies = GetAssembliesInteral();

        /// <summary>
        /// This static instance is used in the shared code in places where the request container context
        /// is not known or does not contain an instance of IWebApiAssembliesResolver.
        /// </summary>
        public static readonly IAssemblyResolver Default = new DefaultAssemblyResolver();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Assembly> Assemblies => _assemblies;

        private static Assembly[] GetAssembliesInteral()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }
}
