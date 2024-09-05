//-----------------------------------------------------------------------------
// <copyright file="NonValidatingParameterBindingAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.OData.Abstracts;

/// <summary>
/// An attribute to disable model validation for a particular type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class NonValidatingParameterBindingAttribute : ModelBinderAttribute, IPropertyValidationFilter
{
    /// <inheritdoc />
    public bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry)
    {
        return false;
    }

    /// <inheritdoc/>
    public override BindingSource BindingSource
    {
        get
        {
            return BindingSource.Body;
        }
    }
}
