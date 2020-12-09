// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// CLR type cache
    /// </summary>
    internal class ClrTypeCache
    {
        private ConcurrentDictionary<Type, IEdmTypeReference> _clrToEdmTypeCache =
            new ConcurrentDictionary<Type, IEdmTypeReference>();

        private ConcurrentDictionary<IEdmTypeReference, Type> _edmToClrTypeCache = new ConcurrentDictionary<IEdmTypeReference, Type>();

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            IEdmTypeReference edmType;
            if (!_clrToEdmTypeCache.TryGetValue(clrType, out edmType))
            {
                edmType = model.GetEdmTypeReference(clrType);
                _clrToEdmTypeCache[clrType] = edmType;
            }

            return edmType;
        }

        public Type GetClrType(IEdmTypeReference edmType, IEdmModel edmModel)
        {
            Type clrType;

            if (!_edmToClrTypeCache.TryGetValue(edmType, out clrType))
            {
                clrType = edmModel.GetClrType(edmType);
                _edmToClrTypeCache[edmType] = clrType;
            }

            return clrType;
        }
    }
}
