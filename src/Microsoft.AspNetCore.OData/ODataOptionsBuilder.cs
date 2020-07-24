// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Contains the details of a given OData request. These properties should all be mutable.
    /// None of these properties should ever be set to null.
    /// </summary>
    /// <summary>
    /// Provides programmatic configuration for the OData service.
    /// </summary>
    public class ODataOptionsBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOptionsBuilder" /> class.
        /// </summary>
        /// <param name="options"> The options to be configured. </param>
        public ODataOptionsBuilder(ODataOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the options being configured.
        /// </summary>
        public virtual ODataOptions Options { get; }
    }
}
