// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OData.Edm;

namespace Microsoft.OData.Abstractions.Annotations
{
    /// <summary>
    /// This annotation indicates the mapping from a <see cref="IEdmStructuredType"/> to a <see cref="PropertyInfo"/>.
    /// The <see cref="IEdmStructuredType"/> is an open type and the <see cref="PropertyInfo"/> is the specific
    /// property which is used in an open type to save/retrieve the dynamic properties.
    /// </summary>
    public class DynamicPropertyDictionaryAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DynamicPropertyDictionaryAnnotation"/> class.
        /// </summary>
        /// <param name="propertyInfo">The backing <see cref="PropertyInfo"/>.</param>
        public DynamicPropertyDictionaryAnnotation(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (!typeof(IDictionary<string, object>).IsAssignableFrom(propertyInfo.PropertyType))
            {
                string message = $"Type '{propertyInfo.PropertyType.Name}' is not supported as dynamic property annotation. Referenced property must be of type 'IDictionary<string, object>'.";
                throw new ArgumentException(message);
            }

            PropertyInfo = propertyInfo;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> which backs the dynamic properties of the open type.
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get;
            private set;
        }
    }
}
