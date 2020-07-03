// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Microsoft.AspNetCore.OData.Routing
{
    internal class ODataRoutingApplicationModelDebugProvider : IApplicationModelProvider
    {
        public int Order => -100;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            Console.WriteLine("OnProvidersExecuted of ODataEndpointModelDebugProvider <==");

            foreach (var controller in context.Result.Controllers)
            {
                foreach (var action in controller.Actions)
                {
                    foreach (var selector in action.Selectors)
                    {
                        ODataEndpointMetadata metadata = selector.EndpointMetadata.OfType<ODataEndpointMetadata>().FirstOrDefault();
                        if (metadata == null)
                        {
                            continue;
                        }

                        /*{metadata.Template.Template}*/
                        Console.WriteLine($"{action.ActionMethod.Name} in {controller.ControllerName}Controller: '{selector.AttributeRouteModel.Template}' ");
                    }
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            Console.WriteLine("OnProvidersExecuting of ODataEndpointModelDebugProvider ==>");
        }
    }
}
