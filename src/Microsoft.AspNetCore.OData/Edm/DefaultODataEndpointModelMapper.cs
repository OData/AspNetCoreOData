//-----------------------------------------------------------------------------
// <copyright file="DefaultODataEndpointModelMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal class DefaultODataEndpointModelMapper : IODataEndpointModelMapper
    {
        public ConcurrentDictionary<Endpoint, IEdmModel> Maps { get; } = new ConcurrentDictionary<Endpoint, IEdmModel>();
    }
}
