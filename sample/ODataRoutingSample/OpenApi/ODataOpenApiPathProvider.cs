// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData;
using Microsoft.OpenApi.OData.Edm;

namespace ODataRoutingSample.OpenApi
{
    /// <summary>
    /// OData openapi path provider
    /// </summary>
    internal class ODataOpenApiPathProvider : IODataPathProvider
    {
        private IList<ODataPath> _paths = new List<ODataPath>();

        public bool CanFilter(IEdmElement element)
        {
            return true;
        }

        public IEnumerable<ODataPath> GetPaths(IEdmModel model, OpenApiConvertSettings settings)
        {
            return _paths;
        }

        public void Add(ODataPath path)
        {
            _paths.Add(path);
        }
    }
}
