//-----------------------------------------------------------------------------
// <copyright file="IComputeWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Wrapper;

/// <summary>
/// Represents a wrapper for a source object with computed values in an OData query.
/// </summary>
/// <typeparam name="T">The type of the source object.</typeparam>
/// <remarks>
/// The source object type can either be the type of the wrapped entity or another instance of <see cref="IComputeWrapper{T}"/>
/// when compute expressions are chained.
/// For example, in the OData query:
/// <code>
/// /Sales?$apply=compute(Amount mul Product/TaxRate as Tax)/compute(Amount add Tax as SalesPrice)
/// </code>
/// In the first compute expression, the source object will be the wrapped "Sale" entity.
/// In the second compute expression, the source object will be an instance of <see cref="IComputeWrapper{T}"/> where T is "Sale".
/// </remarks>
public interface IComputeWrapper<T>
{
    /// <summary>
    /// Gets or sets the source object that provides the values used in the compute expression.
    /// </summary>
    public T Instance { get; set; }

    /// <summary>
    /// Gets or sets the Edm model associated with the wrapper.
    /// </summary>
    public IEdmModel Model { get; set; }
}
