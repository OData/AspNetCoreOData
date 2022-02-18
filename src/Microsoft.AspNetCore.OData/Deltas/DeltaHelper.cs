//-----------------------------------------------------------------------------
// <copyright file="DeltaHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using System;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// The delta helpers.
    /// </summary>
    internal static class DeltaHelper
    {
        /// <summary>
        /// Helper method to check whether the given type is Delta generic type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it is a Delta generic type; false otherwise.</returns>
        public static bool IsDeltaOfT(Type type)
        {
            return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Delta<>);
        }

        /// <summary>
        /// Helper method to check whether the given object is Delta resource set.
        /// </summary>
        /// <param name="result">The given object.</param>
        /// <returns>True/False.</returns>
        public static bool IsDeltaResourceSet(object result)
        {
            if (result == null)
            {
                return false;
            }

            Type resultType = result.GetType();
            if (typeof(IDeltaSet).IsAssignableFrom(resultType))
            {
                return true;
            }
            else if (typeof(EdmChangedObjectCollection).IsAssignableFrom(resultType))
            {
                return true;
            }

            return false;
        }
    }
}
