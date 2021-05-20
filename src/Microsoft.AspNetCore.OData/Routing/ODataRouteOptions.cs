// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Represents the configurable options on a conventional routing template building.
    /// </summary>
    public class ODataRouteOptions
    {
        // The default route options.
        internal static readonly ODataRouteOptions Default = new ODataRouteOptions();

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/entityset({key}).
        /// Used in conventional routing.
        /// </summary>
        public bool EnableKeyInParenthesis { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/entityset/{key}
        /// Used in conventional routing.
        /// </summary>
        public bool EnableKeyAsSegment { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/Namespace.MyFunction(parameters...)
        /// Used in conventional routing.
        /// </summary>
        public bool EnableQualifiedOperationCall { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to generate odata path template as ~/MyFunction(parameters...)
        /// Used in conventional routing.
        /// </summary>
        public bool EnableUnqualifiedOperationCall { get; set; } = true;
    }
}
