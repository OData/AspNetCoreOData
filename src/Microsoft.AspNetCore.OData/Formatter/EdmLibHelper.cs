//-----------------------------------------------------------------------------
// <copyright file="EdmLibHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Deltas;

namespace Microsoft.AspNetCore.OData.Formatter;

internal static class EdmLibHelper
{
    /// <summary>
    /// Get the expected payload type of an OData path.
    /// </summary>
    /// <param name="type">The Type to use.</param>
    /// <param name="path">The path to use.</param>
    /// <param name="model">The EdmModel to use.</param>
    /// <returns>The expected payload type of an OData path.</returns>
    internal static IEdmTypeReference GetExpectedPayloadType(Type type, ODataPath path, IEdmModel model)
    {
        IEdmTypeReference expectedPayloadType = null;

        if (typeof(IEdmObject).IsAssignableFrom(type))
        {
            // typeless mode. figure out the expected payload type from the OData Path.
            IEdmType edmType = path.LastSegment.EdmType;
            if (edmType != null)
            {
                expectedPayloadType = edmType.ToEdmTypeReference(isNullable: false);
                if (expectedPayloadType.TypeKind() == EdmTypeKind.Collection)
                {
                    IEdmTypeReference elementType = expectedPayloadType.AsCollection().ElementType();
                    if (elementType.IsEntity())
                    {
                        // collection of entities cannot be CREATE/UPDATEd. Instead, the request would contain a single entry.
                        expectedPayloadType = elementType;
                    }
                }
            }
        }
        else
        {
            TryGetInnerTypeForDelta(ref type);
            expectedPayloadType = model.GetEdmTypeReference(type);
        }

        return expectedPayloadType;
    }

    /// <summary>
    /// Try to return the inner type of a generic Delta.
    /// </summary>
    /// <param name="type">in: The type to test; out: inner type of a generic Delta.</param>
    /// <returns>True if the type was generic Delta; false otherwise.</returns>
    internal static bool TryGetInnerTypeForDelta(ref Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Delta<>))
        {
            type = type.GetGenericArguments()[0];
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DeltaSet<>))
        {
            type = type.GetGenericArguments()[0];
            return true;
        }

        return false;
    }
}
