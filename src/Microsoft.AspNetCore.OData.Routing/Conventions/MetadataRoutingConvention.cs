// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Template;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class MetadataRoutingConvention : IODataControllerActionConvention
    {
        private static TypeInfo metadataTypeInfo = typeof(MetadataController).GetTypeInfo();

        /// <summary>
        /// 
        /// </summary>
        public virtual int Order => 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            return context?.Controller?.ControllerType == metadataTypeInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.Controller != null);
            Debug.Assert(context.Action != null);
            ActionModel action = context.Action;

            if (action.Controller.ControllerType != typeof(MetadataController).GetTypeInfo())
            {
                return false;
            }

            if (action.ActionMethod.Name == "GetMetadata")
            {
                ODataPathTemplate template = new ODataPathTemplate(MetadataSegmentTemplate.Instance);
                action.AddSelector(context.Prefix, context.Model, template);

                return true;
            }

            if (action.ActionMethod.Name == "GetServiceDocument")
            {
                ODataPathTemplate template = new ODataPathTemplate();
                action.AddSelector(context.Prefix, context.Model, template);

                return true;
            }

            return false;
        }
    }
}
