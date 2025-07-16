//-----------------------------------------------------------------------------
// <copyright file="ODataQueryFilterInvocationContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Extensions;

/// <summary>
/// Provides an abstraction for wrapping the <see cref="EndpointFilterInvocationContext"/> and the <see cref="System.Reflection.MethodInfo"/> associated with a route handler.
/// </summary>
public class ODataQueryFilterInvocationContext
{
    /// <summary>
    /// The <see cref="MethodInfo"/> associated with the current route handler.
    /// </summary>
    public MethodInfo MethodInfo { get; init; }

    /// <summary>
    /// The <see cref="EndpointFilterInvocationContext"/> associated with the current route filter.
    /// </summary>
    public EndpointFilterInvocationContext InvocationContext { get; init; }

    /// <summary>
    /// Gets the <see cref="HttpContext"/>.
    /// </summary>
    public HttpContext HttpContext => InvocationContext.HttpContext;
}
