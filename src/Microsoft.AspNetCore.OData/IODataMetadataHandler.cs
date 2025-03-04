//-----------------------------------------------------------------------------
// <copyright file="IODataMetadataHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// A handler used to output OData CSDL.
/// </summary>
public interface IODataMetadataHandler
{
    /// <summary>
    /// Implements the core logic associated with the filter given a <see cref="HttpContext"/> and the <see cref="IEdmModel"/>.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <param name="model">The Edm model.</param>
    /// <returns>An awaitable result of calling the handler.</returns>
    ValueTask InvokeAsync(HttpContext context, IEdmModel model);
}
