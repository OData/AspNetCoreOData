//-----------------------------------------------------------------------------
// <copyright file="EdmModelMetadata.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm;

/// <summary>
/// Represents Edm model metadata used during OData query.
/// </summary>
public sealed class EdmModelMetadata : IEdmModelMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdmModelMetadata" /> class.
    /// </summary>
    /// <param name="model">The Edm model.</param>
    public EdmModelMetadata(IEdmModel model = null)
    {
        //ArgumentNullException.ThrowIfNull(model);

        Model = model;
    }

    /// <summary>
    /// Gets the <see cref="IEdmModel"/>.
    /// </summary>
    public IEdmModel Model { get; }
}