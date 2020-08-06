// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Edm
{
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
