// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ControllerActionModelExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this ControllerModel controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            return controller.Attributes.Any(a => a is T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this ControllerModel controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            return controller.Attributes.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return action.Attributes.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="prefix"></param>
        /// <param name="model"></param>
        /// <param name="template"></param>
        public static void AddSelector(this ActionModel action, string prefix, IEdmModel model, ODataPathTemplate template)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            SelectorModel selectorModel = action.Selectors.FirstOrDefault(s => s.AttributeRouteModel == null);
            if (selectorModel == null)
            {
                selectorModel = new SelectorModel();
                action.Selectors.Add(selectorModel);
            }

            string templateStr = string.IsNullOrEmpty(prefix) ? template.Template : $"{prefix}/{template.Template}";

            selectorModel.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(templateStr) { Name = templateStr });
            selectorModel.EndpointMetadata.Add(new ODataEndpointMetadata(prefix, model, template));
        }
    }
}
