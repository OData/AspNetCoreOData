// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    public interface IWebHostTestFixture
    {
        /// <summary>
        /// Gets or sets a value indicating whether error details should be included.
        /// </summary>
        bool IncludeErrorDetail { get; set; }

        // Action<WebRouteConfiguration> ConfigurationAction { get; }
    }
}
