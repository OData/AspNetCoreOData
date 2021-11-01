//-----------------------------------------------------------------------------
// <copyright file="IODataTypeMapperExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// Extension methods for <see cref="IODataTypeMapper"/>.
    /// </summary>
    public static class IODataTypeMapperExtensions
    {
        /// <summary>
        /// Gets the corresponding <see cref="Type"/> for a given Edm primitive type <see cref="IEdmPrimitiveTypeReference"/>.
        /// </summary>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="primitiveType">The Edm primitive type reference.</param>
        /// <returns>Null or the CLR type.</returns>
        public static Type GetPrimitiveType(this IODataTypeMapper mapper, IEdmPrimitiveTypeReference primitiveType)
        {
            if (mapper == null)
            {
                throw Error.ArgumentNull(nameof(mapper));
            }

            if (primitiveType == null)
            {
                throw Error.ArgumentNull(nameof(primitiveType));
            }

            return mapper.GetPrimitiveType(primitiveType.PrimitiveDefinition(), primitiveType.IsNullable);
        }

        /// <summary>
        /// Gets the corresponding Edm type <see cref="IEdmType"/> for the given CLR type <see cref="Type"/>.
        /// </summary>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="edmModel">The given Edm model.</param>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the corresponding Edm type.</returns>
        public static IEdmType GetEdmType(this IODataTypeMapper mapper, IEdmModel edmModel, Type clrType)
        {
            if (mapper == null)
            {
                throw Error.ArgumentNull(nameof(mapper));
            }

            return mapper.GetEdmTypeReference(edmModel, clrType)?.Definition;
        }

        /// <summary>
        /// Gets the corresponding <see cref="Type"/> for a given Edm type <see cref="IEdmTypeReference"/>.
        /// </summary>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmType">The Edm type reference.</param>
        /// <returns>Null or the CLR type.</returns>
        public static Type GetClrType(this IODataTypeMapper mapper, IEdmModel edmModel, IEdmTypeReference edmType)
        {
            return mapper.GetClrType(edmModel, edmType, AssemblyResolverHelper.Default);
        }

        /// <summary>
        /// Gets the corresponding <see cref="Type"/> for a given Edm type <see cref="IEdmTypeReference"/>.
        /// </summary>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <param name="assembliesResolver">The assembly resolver.</param>
        /// <returns>Null or the CLR type.</returns>
        public static Type GetClrType(this IODataTypeMapper mapper, IEdmModel edmModel, IEdmTypeReference edmType, IAssemblyResolver assembliesResolver)
        {
            if (mapper == null)
            {
                throw Error.ArgumentNull(nameof(mapper));
            }

            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            return mapper.GetClrType(edmModel, edmType.Definition, edmType.IsNullable, assembliesResolver);
        }
    }
}
