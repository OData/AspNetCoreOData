//-----------------------------------------------------------------------------
// <copyright file="EdmObjectExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value;

/// <summary>
/// Extension methods for the <see cref="IEdmType"/> interface.
/// </summary>
public static class EdmTypeExtensions
{
    /// <summary>
    /// Method to determine whether the current type is a Delta resource set.
    /// </summary>
    /// <param name="type">IEdmType to be compared</param>
    /// <returns>True or False if type is same as <see cref="EdmDeltaCollectionType"/></returns>
    public static bool IsDeltaResourceSet(this IEdmType type)
    {
        if (type == null)
        {
            throw Error.ArgumentNull(nameof(type));
        }

        return (type.GetType() == typeof(EdmDeltaCollectionType));
    }

    /// <summary>
    /// Method to determine whether the current Edm object is a Delta resource
    /// </summary>
    /// <param name="resource">IEdmObject to be compared</param>
    /// <returns>True or False if type is same as <see cref="EdmDeltaResourceObject"/> or <see cref="EdmDeltaComplexObject"/></returns>
    public static bool IsDeltaResource(this IEdmObject resource)
    {
        if (resource == null)
        {
            throw Error.ArgumentNull(nameof(resource));
        }

        TypedEdmEntityObject obj = resource as TypedEdmEntityObject;
        if (obj != null)
        {
            if (obj.Instance is IDeltaSetItem)
            {
                return true;
            }
        }

        if (resource is IDeltaSetItem)
        {
            return true;
        }

        return (resource is EdmDeltaResourceObject || resource is EdmDeltaComplexObject);
    }
}
