//-----------------------------------------------------------------------------
// <copyright file="ODataMinimalMatcherPolicy.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
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
    internal class ODataMinimalMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private IODataTemplateTranslator translator;

        public ODataMinimalMatcherPolicy(IODataTemplateTranslator translator)
        {
            this.translator = translator;
        }

        public override int Order => 999;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return true;
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull(nameof(httpContext));
            }

            IODataFeature odataFeature = httpContext.ODataFeature();
            if (odataFeature.Path != null)
            {
                // If we have the OData path setting, it means there's some Policy working.
                // Let's skip this default OData matcher policy.
                return Task.CompletedTask;
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

                if (odataFeature.Path != null)
                {
                    // If it's odata endpoint, and we have a path set, let other odata endpoints invalid.
                    candidates.SetValidity(i, false);
                    continue;
                }


                ODataTemplateTranslateContext translatorContext =
                    new ODataTemplateTranslateContext(httpContext, candidate.Endpoint, candidate.Values ?? new RouteValueDictionary(), metadata.Model);

                ODataPath odataPath = translator.Translate(metadata.Template, translatorContext);
                if (odataPath != null)
                {
                    odataFeature.RoutePrefix = metadata.Prefix;
                    odataFeature.Model = metadata.Model;
                    odataFeature.Path = odataPath;

                    foreach (var data in translatorContext.UpdatedValues)
                    {
                        candidate.Values[data.Key] = data.Value;
                    }

                    // Shall we break the remaining candidates?
                    // So far the answer is no. Because we can use this matcher to obsolete the unmatched endpoint.
                    // break;
                }
                else
                {
                    candidates.SetValidity(i, false);
                }
            }

            return Task.CompletedTask;
        }
    }
}
