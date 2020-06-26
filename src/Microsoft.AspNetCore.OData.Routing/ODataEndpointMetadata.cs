// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// 
    /// </summary>
    internal class ODataEndpointMetadata
    {
        public ODataEndpointMetadata(string prefix, IEdmModel model, ODataPathTemplate template)
        {
            Prefix = prefix;
            Model = model;
            Template = template;
        }

        public ODataEndpointMetadata(string prefix, IEdmModel model, string template)
        {
            Prefix = prefix;
            Model = model;
            StrTemplate = template;
        }

        public string Prefix { get; }

        public IEdmModel Model { get; }

        public string StrTemplate { get; }

        public ODataPathTemplate Template { get; }

        // { { "$filter", "IntProp eq @p1" }, { "@p1", "@p2" }, { "@p2", "123" } });
        public ODataPath GenerateODataPath(RouteValueDictionary values, QueryString queryString)
        {
            if (Template != null)
            {
                return Template.GenerateODataPath(Model, values, queryString);
            }

            return null;
        }
    }
}
