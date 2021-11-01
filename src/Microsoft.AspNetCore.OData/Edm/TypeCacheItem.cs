//-----------------------------------------------------------------------------
// <copyright file="TypeCacheItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal class TypeCacheItem
    {
        #region ClrType => EdmType
        /// <summary>
        /// <see cref="Type"/> to <see cref="IEdmTypeReference"/>.
        /// </summary>
        public ConcurrentDictionary<Type, IEdmTypeReference> ClrToEdmTypeCache { get; } = new ConcurrentDictionary<Type, IEdmTypeReference>();

        public bool TryFindEdmType(Type clrType, out IEdmTypeReference edmType)
        {
            edmType = null;
            if (clrType == null)
            {
                return false;
            }

            return ClrToEdmTypeCache.TryGetValue(clrType, out edmType);
        }

        public void AddClrToEdmMap(Type clrType, IEdmTypeReference edmType)
        {
            ClrToEdmTypeCache[clrType] = edmType;
        }
        #endregion

        #region EdmType => ClrType
        /// <summary>
        /// <see cref="IEdmType"/> to <see cref="Type"/>.
        /// item1: non-nullable
        /// item2: nullable
        /// </summary>
        public ConcurrentDictionary<IEdmType, (Type, Type)> EdmToClrTypeCache { get; } = new ConcurrentDictionary<IEdmType, (Type, Type)>();

        public bool TryFindClrType(IEdmType edmType, bool isNullable, out Type clrType)
        {
            if (edmType == null)
            {
                clrType = null;
                return false;
            }

            clrType = null;
            if (EdmToClrTypeCache.TryGetValue(edmType, out (Type, Type) clrTypes))
            {
                if (isNullable)
                {
                    clrType = clrTypes.Item2;
                }
                else
                {
                    clrType = clrTypes.Item1;
                }
            }

            return clrType != null;
        }

        public void AddEdmToClrMap(IEdmType edmType, bool isNullable, Type clrType)
        {
            if (isNullable)
            {
                EdmToClrTypeCache.AddOrUpdate(edmType, (null, clrType), (k, v) => (v.Item1, clrType));
            }
            else
            {
                EdmToClrTypeCache.AddOrUpdate(edmType, (clrType, null), (k, v) => (clrType, v.Item2));
            }
        }
        #endregion
    }
}
