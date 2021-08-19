//-----------------------------------------------------------------------------
// <copyright file="IODataTemplateTranslator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Defines a contract used to translate the OData path template to OData path.
    /// </summary>
    public interface IODataTemplateTranslator
    {
        /// <summary>
        /// Translate the <see cref="ODataPathTemplate"/> to <see cref="ODataPath"/>
        /// </summary>
        /// <param name="path">The OData path template.</param>
        /// <param name="context">The translate context.</param>
        /// <returns>The OData Path.</returns>
        ODataPath Translate(ODataPathTemplate path, ODataTemplateTranslateContext context);
    }
}
