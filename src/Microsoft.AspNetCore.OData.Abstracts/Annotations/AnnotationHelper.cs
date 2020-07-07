// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts.Annotations
{
    /// <summary>
    /// 
    /// </summary>
    public static class AnnotationHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmModel"></param>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static ClrEnumMemberAnnotation GetClrEnumMemberAnnotation(this IEdmModel edmModel, IEdmEnumType enumType)
        {
            if (edmModel == null)
            {
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            ClrEnumMemberAnnotation annotation = edmModel.GetAnnotationValue<ClrEnumMemberAnnotation>(enumType);
            if (annotation != null)
            {
                return annotation;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmModel"></param>
        /// <param name="edmProperty"></param>
        /// <returns></returns>
        public static string GetClrPropertyName(this IEdmModel edmModel, IEdmProperty edmProperty)
        {
            if (edmModel == null)
            {
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (edmProperty == null)
            {
                throw new ArgumentNullException(nameof(edmProperty));
            }

            string propertyName = edmProperty.Name;
            ClrPropertyInfoAnnotation annotation = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>(edmProperty);
            if (annotation != null)
            {
                PropertyInfo propertyInfo = annotation.ClrPropertyInfo;
                if (propertyInfo != null)
                {
                    propertyName = propertyInfo.Name;
                }
            }

            return propertyName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmModel"></param>
        /// <param name="edmType"></param>
        /// <returns></returns>
        public static PropertyInfo GetDynamicPropertyDictionary(this IEdmModel edmModel, IEdmStructuredType edmType)
        {
            if (edmModel == null)
            {
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
            }

            DynamicPropertyDictionaryAnnotation annotation =
                edmModel.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.PropertyInfo;
            }

            return null;
        }

    }
}
