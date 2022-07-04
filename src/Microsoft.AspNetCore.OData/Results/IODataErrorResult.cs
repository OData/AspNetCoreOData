//-----------------------------------------------------------------------------
// <copyright file="IODataErrorResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Results
{
    /// <summary>
    /// Provide the interface for the details of a given OData error result.
    /// </summary>
    public interface IODataErrorResult
    {
        /// <summary>
        /// OData error.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        ODataError Error { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}
