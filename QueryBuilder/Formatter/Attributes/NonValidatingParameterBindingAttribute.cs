using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QueryBuilder.Abstracts
{
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
}
