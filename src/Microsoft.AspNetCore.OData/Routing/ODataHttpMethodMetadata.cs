// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Routing
{
    internal class ODataHttpMethodMetadata
    {
        public ODataHttpMethodMetadata(params string[] httpMethods)
        {
            Methods = new HashSet<string>(httpMethods, StringComparer.OrdinalIgnoreCase);
        }

        public ISet<string> Methods { get; }
    }
}
