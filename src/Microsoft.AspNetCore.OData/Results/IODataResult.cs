//-----------------------------------------------------------------------------
// <copyright file="IODataResult.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.Results;

/// <summary>
/// A contracts for OData result 
/// </summary>
public interface IODataResult
{
    /// <summary>
    /// Gets the real value.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Gets the expected type.
    /// </summary>
    Type ExpectedType { get; }
}

