//-----------------------------------------------------------------------------
// <copyright file="OmitValuesKind.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// Omit-Values kind
    /// </summary>
    public enum OmitValuesKind
    {
        /// <summary>
        /// Not set, unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// If nulls is specified, then the service MAY omit properties containing null values from the response,
        /// in which case it MUST specify the Preference-Applied response header with omit-values=nulls.
        /// </summary>
        Nulls,

        /// <summary>
        /// If defaults is specified, then the service MAY omit properties containing default values from the response, including nulls for properties that have no other defined default value.
        /// Nulls MUST be included for properties that have a non-null default value defined.
        /// If the service omits default values it MUST specify the Preference-Applied response header with omit-values=defaults.
        /// </summary>
        Defaults
    }
}
