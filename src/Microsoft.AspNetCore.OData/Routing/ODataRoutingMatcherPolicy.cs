// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Defines a policy that applies behaviors to the OData Uri matcher.
    /// </summary>
    internal class ODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        /// <summary>
        /// Gets a value that determines the order of this policy.
        /// </summary>
        public override int Order => 1000 - 102;

        /// <summary>
        /// Returns a value that indicates whether the matcher applies to any endpoint in endpoints.
        /// </summary>
        /// <param name="endpoints">The set of candidate values.</param>
        /// <returns>true if the policy applies to any endpoint in endpoints, otherwise false.</returns>
        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            // Let's apply this matcher policy to all the endpoints.
            return true;
        }

        /// <summary>
        /// Applies the policy to the CandidateSet.
        /// </summary>
        /// <param name="httpContext">The context associated with the current request.</param>
        /// <param name="candidates">The CandidateSet.</param>
        /// <returns>The task.</returns>
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

                var oDataMetadata = candidate.Endpoint.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault();
                if (oDataMetadata == null)
                {
                    continue;
                }

                string serviceRoot = Test(httpContext.Request);

                string servieRoot2 = Test2(candidate.Endpoint, httpContext.Request);

                var originalValues = candidate.Values;
                var oPath = oDataMetadata.GenerateODataPath(httpContext.Request, originalValues, httpContext.Request.QueryString);
                if (oPath != null)
                {
                    var odata = httpContext.ODataFeature();
                    odata.Model = oDataMetadata.Model;
                    odata.Path = oPath;

                    // Double confirm whether it's required or not?
                    //candidates.SetValidity(i, true); 
                }
                else
                {
                    candidates.SetValidity(i, false);
                }
            }

            return Task.CompletedTask;
        }

        private static string Test(HttpRequest request)
        {
            // We need to call Uri.GetLeftPart(), which returns an encoded Url.
            // The ODL parser does not like raw values.
            Uri requestUri = new Uri(request.GetEncodedUrl());
            string requestLeftPart = requestUri.GetLeftPart(UriPartial.Path);


            return requestLeftPart;
        }

        private string Test2(Endpoint endPoint, HttpRequest request)
        {
            LinkGenerator linkGenerator = request.HttpContext.RequestServices.GetRequiredService<LinkGenerator>();
            if (linkGenerator != null)
            {
                //   string uri = linkGenerator.GetUriByAction(request.HttpContext);

               // Endpoint endPoint = request.HttpContext.GetEndpoint();
                EndpointNameMetadata name = endPoint.Metadata.GetMetadata<EndpointNameMetadata>();

                string aUri = linkGenerator.GetUriByName(request.HttpContext, name.EndpointName,
                    request.RouteValues, request.Scheme, request.Host, request.PathBase);

                return aUri;
            }

            return null;
        }
    }
}
