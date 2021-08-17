// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Default implementation for <see cref="IODataTemplateTranslator"/>.
    /// </summary>
    internal class DefaultODataTemplateTranslator : IODataTemplateTranslator
    {
        /// <inheritdoc />
        public virtual ODataPath Translate(ODataPathTemplate path, ODataTemplateTranslateContext context)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // calculate every time
            foreach (var segment in path)
            {
                if (!segment.TryTranslate(context))
                {
                    return null;
                }
            }

            return new ODataPath(context.Segments);
        }
    }
}