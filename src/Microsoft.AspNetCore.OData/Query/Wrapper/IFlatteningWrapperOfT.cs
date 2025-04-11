//-----------------------------------------------------------------------------
// <copyright file="IFlatteningWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Wrapper;

/// <summary>
/// Represents the result of flattening properties referenced in aggregate clause of a $apply query.
/// </summary>
/// <typeparam name="T">The type of the source object that contains the properties to be flattened.</typeparam>
/// <remarks>Flattening is necessary to avoid generation of nested queries by Entity Framework.</remarks>
public interface IFlatteningWrapper<T>
{
    /// <summary>Gets or sets the source object that contains the properties to be flattened.</summary>
    T Source { get; set; }
}
