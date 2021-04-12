// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Template;

namespace ODataCustomizedSample.Extensions
{
    public class MyEntitySetRoutingConvention : IODataControllerActionConvention
    {
        public virtual int Order => 0;

        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            return context.Controller.ControllerName == "GenericOData";
        }

        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context.Prefix == "v{version}")
            {
                // let's only allow this prefix
                return false;
            }

            if (context.Action.ActionName != "GetTest")
            {
                return false;
            }

            ODataPathTemplate path = new ODataPathTemplate(
                new EntitySetTemplateSegment()
                );

            context.Action.AddSelector("Get", context.Prefix, context.Model, path);
            return true;
        }
    }
}
