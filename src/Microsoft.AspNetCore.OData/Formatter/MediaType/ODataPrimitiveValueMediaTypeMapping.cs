﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// Media type mapping that associates requests with $value on primitive property.
    /// </summary>
    public class ODataPrimitiveValueMediaTypeMapping : ODataRawValueMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPrimitiveValueMediaTypeMapping"/> class.
        /// </summary>
        public ODataPrimitiveValueMediaTypeMapping()
            : base("text/plain")
        {
        }

        /// <inheritdoc/>
        protected override bool IsMatch(PropertySegment propertySegment)
        {
            return propertySegment != null &&
                   propertySegment.Property.Type.IsPrimitive() &&
                   !propertySegment.Property.Type.IsBinary() &&
                   !propertySegment.Property.Type.IsStream();
        }
    }
}
