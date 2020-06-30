// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// 
    /// </summary>
    public interface IODataPathTemplateParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="odataPath"></param>
        /// <returns></returns>
        ODataPathTemplate Parse(IEdmModel model, string odataPath);
    }
}
