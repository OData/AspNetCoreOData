// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// Provides the mapping between CLR type and Edm type.
    /// </summary>
    public interface IODataTypeMappingProvider
    {
        /// <summary>
        /// Maps a CLR type to standard CLR type.
        /// </summary>
        /// <param name="clrType">The potential non-standard CLR type.</param>
        /// <returns>The standard CLR type or the input CLR type itself.</returns>
        Type MapTo(Type clrType);

        /// <summary>
        /// Gets the corresponding Edm primitive type for the given CLR type.
        /// </summary>
        /// <param name="clrType">The given CLR type.</param>
        /// <returns>Null or the Edm primitive type.</returns>
        IEdmPrimitiveTypeReference GetEdmPrimitiveType(Type clrType);

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm primitive type.
        /// </summary>
        /// <param name="edmPrimitiveType">The given Edm primitive type.</param>
        /// <returns>Null or the CLR type.</returns>
        Type GetClrPrimitiveType(IEdmPrimitiveTypeReference edmPrimitiveType);

        /// <summary>
        /// Gets the corresponding CLR type for a given Edm type.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <returns>Null or the CLR type.</returns>
        Type GetClrType(IEdmModel model, IEdmTypeReference edmType);

        /// <summary>
        /// Gets the corresponding Edm type for the given CLR type.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="clrType">The CLR type.</param>
        /// <returns>Null or the corresponding Edm type.</returns>
        IEdmTypeReference GetEdmType(IEdmModel model, Type clrType);
    }
}