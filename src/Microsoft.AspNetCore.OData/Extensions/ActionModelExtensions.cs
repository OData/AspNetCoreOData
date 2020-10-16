// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// The extension methods for <see cref="ActionModel"/>.
    /// </summary>
    public static class ActionModelExtensions
    {
        /// <summary>
        /// Gets the collection of supported HTTP methods for conventions.
        /// </summary>
        private static readonly string[] SupportedHttpMethodConventions = new string[]
        {
            "GET",
            "PUT",
            "POST",
            "DELETE",
            "PATCH",
            "HEAD",
            "OPTIONS",
        };

        /// <summary>
        /// Tests whether the action is not suitable for OData action.
        /// </summary>
        /// <param name="action">The given action model.</param>
        /// <returns>True/False.</returns>
        public static bool IsNonODataAction(this ActionModel action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            return action.Attributes.Any(a => a is NonODataActionAttribute);
        }

        /// <summary>
        /// Tests whether the action has the given parameter with the given type.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="action">The given action model.</param>
        /// <param name="parameterName">The given parameter name.</param>
        /// <returns>True/False.</returns>
        public static bool HasParameter<T>(this ActionModel action, string parameterName)
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
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
                throw Error.ArgumentNull(nameof(action));
            }

            return action.Attributes.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Test whether the action has the key parameters defined.
        /// </summary>
        /// <param name="action">The action model.</param>
        /// <param name="entityType">The Edm entity type.</param>
        /// <param name="keyPrefix">The key prefix for the action parameter.</param>
        /// <returns>True/false.</returns>
        public static bool HasODataKeyParameter(this ActionModel action, IEdmEntityType entityType, string keyPrefix = "key")
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull(nameof(entityType));
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
        /// Gets the supported Http method on the action or by convention using the action name.
        /// </summary>
        /// <param name="action">The action model.</param>
        /// <returns>The supported http methods.</returns>
        public static IEnumerable<string> GetSupportedHttpMethods(this ActionModel action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            // Determine the supported methods.
            IEnumerable<string> httpMethods = action.Attributes.OfType<IActionHttpMethodProvider>()
                .FirstOrDefault()?.HttpMethods;

            if (httpMethods == null)
            {
                // If no IActionHttpMethodProvider is specified, fall back to convention the way AspNet does.
                httpMethods = SupportedHttpMethodConventions
                    .Where(method => action.ActionMethod.Name.StartsWith(method, StringComparison.OrdinalIgnoreCase));

                // Use POST as the default method.
                if (!httpMethods.Any())
                {
                    httpMethods = new string[] { "POST" };
                }
            }

            return httpMethods;
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
                throw Error.ArgumentNull(nameof(action));
            }

            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            var httpMethods = action.GetSupportedHttpMethods();

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

                ODataRoutingMetadata odataMetadata = new ODataRoutingMetadata(prefix, model, path);
                selectorModel.EndpointMetadata.Add(odataMetadata);

                // Check with .NET Team whether the "Endpoint name metadata"
                // selectorModel.EndpointMetadata.Add(new EndpointNameMetadata(templateStr));
                foreach (var httpMethod in httpMethods)
                {
                    odataMetadata.HttpMethods.Add(httpMethod);
                }
            }
        }

        /// <summary>
        /// Adds the OData selector model to the action.
        /// </summary>
        /// <param name="action">The given action model.</param>
        /// <param name="httpMethod">The supported http methods, if mulitple, using ',' to separate.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The OData path template.</param>
        public static void AddSelector(this ActionModel action, string httpMethod, string prefix, IEdmModel model, ODataPathTemplate path)
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
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

                ODataRoutingMetadata odataMetadata = new ODataRoutingMetadata(prefix, model, path);
                selectorModel.EndpointMetadata.Add(odataMetadata);

                AddHttpMethod(odataMetadata, httpMethod);

                // Check with .NET Team whether the "Endpoint name metadata"
                selectorModel.EndpointMetadata.Add(new EndpointNameMetadata(Guid.NewGuid().ToString()));
            }
        }

        /// <summary>
        /// Gets the supported Http method on the action or by convention using the action name.
        /// </summary>
        /// <param name="action">The action model.</param>
        /// <returns>The supported http methods.</returns>
        internal static bool HasHttpMethod(this ActionModel action)
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            // Determine the supported methods.
            return action.Attributes.Any(a => a is IActionHttpMethodProvider);
        }

        private static void AddHttpMethod(ODataRoutingMetadata metadata, string httpMethod)
        {
            if (string.IsNullOrEmpty(httpMethod))
            {
                return;
            }

            string[] methods = httpMethod.Split(',');
            foreach (var method in methods)
            {
                metadata.HttpMethods.Add(method);
            }
        }
    }
}

