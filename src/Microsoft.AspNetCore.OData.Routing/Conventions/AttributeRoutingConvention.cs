// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual int Order => -100;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // Apply to all controllers
            return true;
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

            ActionModel action = context.Action;
            ODataRouteAttribute routeAttr = action.GetAttribute<ODataRouteAttribute>();
            if (routeAttr == null)
            {
                return false;
            }
            string prefix = context.Prefix;
            IEdmModel model = context.Model;

            string routeTemplate = "";
            ODataRoutePrefixAttribute prefixAttr = action.Controller.GetAttribute<ODataRoutePrefixAttribute>();
            if (prefixAttr != null)
            {
                routeTemplate = prefixAttr.Prefix + "/";
            }
            routeTemplate += routeAttr.PathTemplate;

            SelectorModel selectorModel = action.Selectors.FirstOrDefault(s => s.AttributeRouteModel == null);
            if (selectorModel == null)
            {
                selectorModel = new SelectorModel();
                action.Selectors.Add(selectorModel);
            }

            string templateStr = string.IsNullOrEmpty(prefix) ? routeTemplate : $"{prefix}/{routeTemplate}";

            selectorModel.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(templateStr) { Name = templateStr });
            selectorModel.EndpointMetadata.Add(new ODataEndpointMetadata(prefix, model, templateStr));

            return true;
        }
    }
}
