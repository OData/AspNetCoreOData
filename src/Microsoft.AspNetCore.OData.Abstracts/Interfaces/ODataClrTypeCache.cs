// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IODataClrTypeCache
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clrType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        IEdmTypeReference GetEdmType(Type clrType, IEdmModel model);
    }

    /// <summary>
    /// 
    /// </summary>
    internal class ODataClrTypeCache
    {
        private IODataTypeMappingProvider _typeProvider;
        private ConcurrentDictionary<Type, IEdmTypeReference> _cache = new ConcurrentDictionary<Type, IEdmTypeReference>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClrTypeCache"/> class.
        /// </summary>
        /// <param name="typeProvider">The registered type provider.</param>
        public ODataClrTypeCache(IODataTypeMappingProvider typeProvider)
        {
            _typeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        public IEdmTypeReference GetEdmType(Type clrType, IEdmModel model)
        {
            IEdmTypeReference edmType;
            if (!_cache.TryGetValue(clrType, out edmType))
            {
                edmType = _typeProvider.GetEdmType(model, clrType);
                _cache[clrType] = edmType;
            }

            return edmType;
        }
    }
}