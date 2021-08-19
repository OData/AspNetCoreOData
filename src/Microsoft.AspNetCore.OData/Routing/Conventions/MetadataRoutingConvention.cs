//-----------------------------------------------------------------------------
// <copyright file="MetadataRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Template;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for $metadata.
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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // This convention only applies to "MetadataController".
            return context.Controller.ControllerType == metadataTypeInfo;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Debug.Assert(context.Controller != null);
            Debug.Assert(context.Action != null);
            ActionModel action = context.Action;
            string actionName = action.ActionName;

            // for ~$metadata
            if (actionName == "GetMetadata")
            {
                ODataPathTemplate template = new ODataPathTemplate(MetadataSegmentTemplate.Instance);
                action.AddSelector("Get", context.Prefix, context.Model, template, context.Options?.RouteOptions);
                return true;
            }

            // for ~/
            if (actionName == "GetServiceDocument")
            {
                ODataPathTemplate template = new ODataPathTemplate();
                action.AddSelector("Get", context.Prefix, context.Model, template, context.Options?.RouteOptions);
                return true;
            }

            return false;
        }
    }
}
