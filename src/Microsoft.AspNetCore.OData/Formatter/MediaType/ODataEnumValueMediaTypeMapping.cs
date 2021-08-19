//-----------------------------------------------------------------------------
// <copyright file="ODataEnumValueMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// Media type mapping that associates requests with $count.
    /// </summary>
    public class ODataEnumValueMediaTypeMapping : ODataRawValueMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEnumValueMediaTypeMapping"/> class.
        /// </summary>
        public ODataEnumValueMediaTypeMapping()
            : base("text/plain")
        {
        }

        /// <inheritdoc/>
        protected override bool IsMatch(PropertySegment propertySegment)
        {
            return propertySegment != null && propertySegment.Property.Type.IsEnum();
        }
    }
}
