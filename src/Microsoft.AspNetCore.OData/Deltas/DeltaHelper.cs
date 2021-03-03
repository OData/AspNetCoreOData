// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
    }
}