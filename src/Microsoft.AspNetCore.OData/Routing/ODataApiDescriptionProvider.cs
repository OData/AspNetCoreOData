// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Defines a contract for specifying <see cref="ApiDescription"/> instances.
    /// </summary>
    public class ODataApiDescriptionProvider : IApiDescriptionProvider
    {
        /// <summary>
        /// Gets the order value for determining the order of execution of providers.
        /// </summary>
        public int Order => -900;

        /// <summary>
        /// Execute for second pass for OData API description.
        /// </summary>
        /// <param name="context">The ApiDescriptionProviderContext.</param>
        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            foreach (var action in context?.Actions)
            {
                IODataRoutingMetadata odataMetadata = action.EndpointMetadata.OfType<IODataRoutingMetadata>().FirstOrDefault();
                if (odataMetadata != null)
                {
                    ApiDescription apiDes = context.Results.FirstOrDefault(r => r.ActionDescriptor == action);
                    if (apiDes != null)
                    {
                        apiDes.RelativePath = odataMetadata.TemplateDisplayName;
                    }
                }
            }
        }

        /// <summary>
        /// Execute for first pass for OData API description.
        /// </summary>
        /// <param name="context">The ApiDescriptionProviderContext.</param>
        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
        }
    }
}
