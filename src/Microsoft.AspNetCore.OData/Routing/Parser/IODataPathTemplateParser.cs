//-----------------------------------------------------------------------------
// <copyright file="IODataPathTemplateParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
