// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public interface IODataPathTemplateParser
    {
        ///// <summary>
        ///// Parses the specified OData path template as an <see cref="ODataPathTemplate"/>.
        ///// </summary>
        ///// <param name="model">The Edm model.</param>
        ///// <param name="odataPath">The OData path template to parse.</param>
        ///// <returns>A parsed representation of the template, or <c>null</c> if the template does not match the model.</returns>
        //ODataPathTemplate Parse(IEdmModel model, string odataPath);

        /// <summary>
        /// Parses the specified OData path template as an <see cref="ODataPathTemplate"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="odataPath">The OData path template to parse.</param>
        /// <param name="requestProvider">The OData service provider.</param>
        /// <returns>A parsed representation of the template, or <c>null</c> if the template does not match the model.</returns>
        ODataPathTemplate Parse(IEdmModel model, string odataPath, IServiceProvider requestProvider);
    }
}
