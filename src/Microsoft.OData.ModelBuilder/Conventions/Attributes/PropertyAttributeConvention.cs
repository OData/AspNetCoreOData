// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.OData.ModelBuilder.Conventions.Attributes
{
    /// <summary>
    /// Base class for all attribute based conventions.
    /// </summary>
    public abstract class PropertyAttributeConvention<TAttribute> : AttributeConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public PropertyAttributeConvention()
            : base(attribute => attribute.GetType() == typeof(TAttribute), allowMultiple: false)
        {
        }
    }
}
