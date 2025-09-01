//-----------------------------------------------------------------------------
// <copyright file="ODataSpatialDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.Formatter.Deserialization;

// TODO: Add to declared API when finalized
/// <summary>
/// Represents an <see cref="ODataDeserializer"/> that can read OData spatial types.
/// </summary>
public class ODataSpatialDeserializer : ODataPrimitiveDeserializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataSpatialDeserializer"/> class.
    /// </summary>
    public ODataSpatialDeserializer()
        : base()
    {
    }

    /// <inheritdoc />
    public override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
    {
        if (item == null)
        {
            return null;
        }

        if (readContext == null)
        {
            throw Error.ArgumentNull(nameof(readContext));
        }

        ODataProperty property = item as ODataProperty;
        if (property != null)
        {
            return base.ReadInline(property, edmType, readContext);
        }

        if (!(item is ISpatial))
        {
            // TODO: Use resource manager for error messages
            throw new ArgumentException($"The item must be of type ISpatial, but was '{item.GetType().FullName}'.", nameof(item));
        }

        return item;
    }
}
