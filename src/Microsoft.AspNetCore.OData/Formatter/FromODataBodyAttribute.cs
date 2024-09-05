//-----------------------------------------------------------------------------
// <copyright file="FromODataBodyAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.OData.Formatter;

/// <summary>
/// An implementation of <see cref="ModelBinderAttribute"/> that can bind URI parameters using OData conventions.
/// </summary>
/// <remarks>
/// I'd like to use this attribute for the action parameters whose value are from request body.
/// It's not finished yet.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
public sealed class FromODataBodyAttribute : ModelBinderAttribute
{
    /// <summary>
    /// Instantiates a new instance of the <see cref="FromODataUriAttribute"/> class.
    /// </summary>
    public FromODataBodyAttribute()
        : base(typeof(ODataBodyModelBinder))
    {
    }
}
