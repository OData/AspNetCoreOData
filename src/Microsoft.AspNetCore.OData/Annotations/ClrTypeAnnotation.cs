// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts.Annotations
{
    /// <summary>
    /// Represents a mapping from an <see cref="IEdmType"/> to a CLR type.
    /// </summary>
    public class ClrTypeAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ClrTypeAnnotation"/> class.
        /// </summary>
        /// <param name="clrType">The backing CLR type for the EDM type.</param>
        public ClrTypeAnnotation(Type clrType)
        {
            ClrType = clrType;
        }

        /// <summary>
        /// Gets the backing CLR type for the EDM type.
        /// </summary>
        public Type ClrType { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ModelNameAnnotation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ModelNameAnnotation(string name)
        {
            ModelName = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the backing CLR type for the EDM type.
        /// </summary>
        public string ModelName { get; }
    }
}
