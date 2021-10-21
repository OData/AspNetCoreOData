//-----------------------------------------------------------------------------
// <copyright file="IODataTypeMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// Provides the mapping between CLR type and Edm type.
    /// </summary>
    public interface IODataTypeMapper
    {
        /// <summary>
        /// Gets the corresponding Edm primitive type <see cref="IEdmPrimitiveTypeReference"/> for a given <see cref="Type"/>.
        /// </summary>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the Edm primitive type.</returns>
        IEdmPrimitiveTypeReference GetPrimitiveType(Type clrType);

        /// <summary>
        /// Gets the corresponding <see cref="Type"/> for a given Edm primitive type <see cref="IEdmPrimitiveType"/>.
        /// </summary>
        /// <param name="primitiveType">The given Edm primitive type.</param>
        /// <param name="nullable">The nullable or not.</param>
        /// <returns>Null or the CLR type.</returns>
        Type GetPrimitiveType(IEdmPrimitiveType primitiveType, bool nullable);

        /// <summary>
        /// Gets the corresponding Edm type <see cref="IEdmTypeReference"/> for the given CLR type <see cref="Type"/>.
        /// </summary>
        /// <param name="edmModel">The given Edm model.</param>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the corresponding Edm type reference.</returns>
        IEdmTypeReference GetEdmTypeReference(IEdmModel edmModel, Type clrType);

        /// <summary>
        /// Gets the corresponding <see cref="Type"/> for a given Edm type <see cref="IEdmType"/>.
        /// </summary>
        /// <param name="edmModel">The given Edm model.</param>
        /// <param name="edmType">The given Edm type.</param>
        /// <param name="nullable">The nullable or not.</param>
        /// <param name="assembliesResolver">The assembly resolver.</param>
        /// <returns>Null or the CLR type.</returns>
        Type GetClrType(IEdmModel edmModel, IEdmType edmType, bool nullable, IAssemblyResolver assembliesResolver);
    }
}
