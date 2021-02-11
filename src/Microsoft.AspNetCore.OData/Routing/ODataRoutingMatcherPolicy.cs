// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Defines a policy that applies behaviors to the OData Uri matcher.
    /// </summary>
    internal class ODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private IODataTemplateTranslator _translator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingMatcherPolicy" /> class.
        /// </summary>
        /// <param name="translator">The registered path template translator.</param>
        public ODataRoutingMatcherPolicy(IODataTemplateTranslator translator)
        {
            _translator = translator;
        }

        /// <summary>
        /// Gets a value that determines the order of this policy.
        /// </summary>
        public override int Order => 900;

        /// <summary>
        /// Returns a value that indicates whether the matcher applies to any endpoint in endpoints.
        /// </summary>
        /// <param name="endpoints">The set of candidate values.</param>
        /// <returns>true if the policy applies to any endpoint in endpoints, otherwise false.</returns>
        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return endpoints.Any(e => e.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() != null);
        }

        /// <summary>
        /// Applies the policy to the CandidateSet.
        /// </summary>
        /// <param name="httpContext">The context associated with the current request.</param>
        /// <param name="candidates">The CandidateSet.</param>
        /// <returns>The task.</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // The goal of this method is to perform the final matching:
            // Map between route values matched by the template and the ones we want to expose to the action for binding.
            // (tweaking the route values is fine here)
            // Invalidating the candidate if the key/function values are not valid/missing.
            // Perform overload resolution for functions by looking at the candidates and their metadata.
            for (var i = 0; i < candidates.Count; i++)
            {
                ref CandidateState candidate = ref candidates[i];
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                IODataRoutingMetadata metadata = candidate.Endpoint.Metadata.OfType<IODataRoutingMetadata>().FirstOrDefault();
                if (metadata == null)
                {
                    continue;
                }

                IHttpMethodMetadata httpMetadata = candidate.Endpoint.Metadata.GetMetadata<IHttpMethodMetadata>();
                if (httpMetadata == null)
                {
                    // Check the http method
                    if (metadata.HttpMethods != null && !metadata.HttpMethods.Contains(httpContext.Request.Method))
                    {
                        candidates.SetValidity(i, false);
                        continue;
                    }
                }

                ODataTemplateTranslateContext translatorContext =
                    new ODataTemplateTranslateContext(httpContext, candidate.Values, metadata.Model);

                try
                {
                    ODataPath odataPath = _translator.Translate(metadata.Template, translatorContext);
                    if (odataPath != null)
                    {
                        IODataFeature odataFeature = httpContext.ODataFeature();
                        odataFeature.PrefixName = metadata.Prefix;
                        odataFeature.Model = metadata.Model;
                        odataFeature.Path = odataPath;

                        MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                    }
                    else
                    {
                        candidates.SetValidity(i, false);
                    }
                }
#if DEBUG
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
#else
                catch (Exception)
                {
#endif
                    candidates.SetValidity(i, false);
                }
            }

            return Task.CompletedTask;
        }

        private static void MergeRouteValues(RouteValueDictionary updates, RouteValueDictionary source)
        {
            foreach (var data in updates)
            {
                source[data.Key] = data.Value;
            }
        }
    }
}
