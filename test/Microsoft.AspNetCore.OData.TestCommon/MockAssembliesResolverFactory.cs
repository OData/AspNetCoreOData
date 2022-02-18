//-----------------------------------------------------------------------------
// <copyright file="MockAssembliesResolverFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.OData.ModelBuilder;
using Moq;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    /// <summary>
    /// A mock to represent an assembly resolver
    /// </summary>
    public class MockAssembliesResolverFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IAssemblyResolver Create(MockAssembly assembly = null)
        {
            IAssemblyResolver resolver = null;
            if (assembly != null)
            {
                resolver = new TestAssemblyResolver(assembly);
            }
            else
            {
                Mock<IAssemblyResolver> mockAssembliesResolver = new Mock<IAssemblyResolver>();
                mockAssembliesResolver
                    .Setup(r => r.Assemblies)
                    .Returns(new Assembly[0]);

                resolver = mockAssembliesResolver.Object;
            }

            return resolver;
        }
    }
}
