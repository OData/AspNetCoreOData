﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Value
{
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
                throw Error.ArgumentNull("type");
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
                throw Error.ArgumentNull("resource");
            }

            return (resource is EdmDeltaResourceObject || resource is EdmDeltaComplexObject);
        }
    }
}
