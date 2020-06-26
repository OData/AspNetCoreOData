// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.OData.Routing.Commons
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NonValidatingParameterBindingAttribute : ModelBinderAttribute, IPropertyValidationFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="parentEntry"></param>
        /// <returns></returns>
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
}
