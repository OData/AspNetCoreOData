//-----------------------------------------------------------------------------
// <copyright file="IODataQueryEndpointFilter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// Provides an interface for implementing a filter to run codes before and after a route handler.
/// </summary>
public interface IODataQueryEndpointFilter : IEndpointFilter
{
    //bool IsOData { get; }

    /// <summary>
    /// Performs the OData query composition before route handler is executing.
    /// </summary>
    /// <param name="context">The OData query filter invocation context.</param>
    /// <returns></returns>
    ValueTask OnFilterExecutingAsync(ODataQueryFilterInvocationContext context);

    /// <summary>
    /// Performs the OData query composition after route handler is executed.
    /// </summary>
    /// <param name="responseValue">The response value from the route handler.</param>
    /// <param name="context">The OData query filter invocation context.</param>
    /// <returns></returns>
    ValueTask<object> OnFilterExecutedAsync(object responseValue, ODataQueryFilterInvocationContext context);
}
