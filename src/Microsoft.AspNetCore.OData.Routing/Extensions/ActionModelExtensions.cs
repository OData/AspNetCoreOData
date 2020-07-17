// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The extension methods for <see cref="ActionModel"/>.
    /// </summary>
    public static class ActionModelExtensions
    {
        /// <summary>
        /// Test whether the action is not suitable for OData action.
        /// </summary>
        /// <param name="action">The given action model.</param>
        /// <returns>True/False.</returns>
        public static bool IsNonODataAction(this ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return action.Attributes.Any(a => a is NonODataActionAttribute);
        }

        /// <summary>
        /// Test whether the action has the given parameter with the given type.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="action">The given action model.</param>
        /// <param name="parameterName">The given parameter name.</param>
        /// <returns>True/False.</returns>
        public static bool HasParameter<T>(this ActionModel action, string parameterName)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // parameter name is unique?
            ParameterModel parameter = action.Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
            {
                return false;
            }

            return parameter.ParameterType == typeof(T);
        }

        /// <summary>
        /// Gets the attribute on an action model.
        /// </summary>
        /// <typeparam name="T">The required attribute type.</typeparam>
        /// <param name="action">The given action model.</param>
        /// <returns>Null or the corresponing attribute.</returns>
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
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAttributes<T>(this ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return action.Attributes.OfType<T>();
        }

        /// <summary>
        /// Test whether the action has the key parameters defined.
        /// </summary>
        /// <param name="action">The action model.</param>
        /// <param name="entityType">The Edm entity type.</param>
        /// <param name="keyPrefix">The key prefix for the actio parameter.</param>
        /// <returns>True/false.</returns>
        public static bool HasODataKeyParameter(this ActionModel action, IEdmEntityType entityType, string keyPrefix = "key")
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            // TODO: shall we make sure the type is matching?
            var keys = entityType.Key().ToArray();
            if (keys.Length == 1)
            {
                // one key
                return action.Parameters.Any(p => p.ParameterInfo.Name == keyPrefix);
            }
            else
            {
                // multipe keys
                foreach (var key in keys)
                {
                    string keyName = $"{keyPrefix}{key.Name}";
                    if (!action.Parameters.Any(p => p.ParameterInfo.Name == keyName))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Adds the OData selector model to the action.
        /// </summary>
        /// <param name="action">The given action model.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The OData path template.</param>
        public static void AddSelector(this ActionModel action, string prefix, IEdmModel model, ODataPathTemplate path)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            foreach (var template in path.GetTemplates())
            {
                SelectorModel selectorModel = action.Selectors.FirstOrDefault(s => s.AttributeRouteModel == null);
                if (selectorModel == null)
                {
                    selectorModel = new SelectorModel();
                    action.Selectors.Add(selectorModel);
                }

                string templateStr = string.IsNullOrEmpty(prefix) ? template : $"{prefix}/{template}";

                selectorModel.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(templateStr) { Name = templateStr });
                selectorModel.EndpointMetadata.Add(new ODataRoutingMetadata(prefix, model, path));

                // Check with .NET Team whether the "Endpoint name metadata"
                // selectorModel.EndpointMetadata.Add(new EndpointNameMetadata(templateStr));
            }
        }
    }
}
