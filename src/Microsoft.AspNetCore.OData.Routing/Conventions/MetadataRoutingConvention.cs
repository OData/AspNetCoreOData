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
    /// The convention for metadata.
    /// </summary>
    public class MetadataRoutingConvention : IODataControllerActionConvention
    {
        private static TypeInfo metadataTypeInfo = typeof(MetadataController).GetTypeInfo();

        /// <summary>
        /// Gets the order value for determining the order of execution of conventions.
        /// Metadata routing convention has 0 order.
        /// </summary>
        public virtual int Order => 0;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // This convention only applies to "MetadataController".
            return context?.Controller?.ControllerType == metadataTypeInfo;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.Controller != null);
            Debug.Assert(context.Action != null);
            ActionModel action = context.Action;

            string actionName = action.ActionMethod.Name;

            // for ~$metadata
            if (actionName == "GetMetadata")
            {
                ODataPathTemplate template = new ODataPathTemplate(MetadataSegmentTemplate.Instance);
                action.AddSelector(context.Prefix, context.Model, template);
                return true;
            }

            // for ~/
            if (actionName == "GetServiceDocument")
            {
                ODataPathTemplate template = new ODataPathTemplate();
                action.AddSelector(context.Prefix, context.Model, template);
                return true;
            }

            return false;
        }
    }
}
