// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// 
    /// </summary>
    internal class ODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        /// <summary>
        /// 
        /// </summary>
        public override int Order => 1000 - 102;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="candidates"></param>
        /// <returns></returns>
        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            // The goal of this method is to perform the final matching:
            // Map between route values matched by the template and the ones we want to expose to the action for binding. 
            // (tweaking the route values is fine here)
            // Invalidating the candidate if the key/function values are not valid/missing.
            // Perform overload resolution for functions by looking at the candidates and their metadata.
            for (var i = 0; i < candidates.Count; i++)
            {
                ref var candidate = ref candidates[i];
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                var oDataMetadata = candidate.Endpoint.Metadata.OfType<ODataEndpointMetadata>().FirstOrDefault();
                if (oDataMetadata == null)
                {
                    continue;
                }

                var originalValues = candidate.Values;
                var oPath = oDataMetadata.GenerateODataPath(originalValues, httpContext.Request.QueryString);
                if (oPath != null)
                {
                    var odata = httpContext.ODataFeature();
                    odata.Model = oDataMetadata.Model;
                    odata.Path = oPath;

                    //candidates.SetValidity(i, true); // Double confirm whether it's required or not?
                    continue;
                }
                else
                {
                    candidates.SetValidity(i, false);
                    continue;
                }
            }

            return Task.CompletedTask;
        }
    }
}
