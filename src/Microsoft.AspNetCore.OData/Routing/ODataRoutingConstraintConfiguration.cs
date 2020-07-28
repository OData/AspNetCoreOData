// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// 
    /// </summary>
    internal class ODataRoutingConstraintConfiguration : IConfigureOptions<RouteOptions>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public void Configure(RouteOptions options)
        {
            options.ConstraintMap.Add("OdataFunctionParameters", typeof(ODataFunctionParameterConstraint));
        }
    }
}
