// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Conventions;
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
        /// <param name="httpMethod">The supported http methods, if mulitple, using ',' to separate.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The OData path template.</param>
        /// <param name="options">The route build options.</param>
        public static void AddSelector(this ActionModel action, string httpMethod, string prefix, IEdmModel model, ODataPathTemplate path, ODataRouteOptions options = null)
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

            foreach ((string template, string display) in path.GetTemplates(options))
            {
                // We have to check the selector model on controller?
                SelectorModel selectorModel = action.Selectors.FirstOrDefault(s => s.AttributeRouteModel == null);
                if (selectorModel == null)
                {
                    selectorModel = CreateSelectorModel(action.Attributes);
                    action.Selectors.Add(selectorModel);
                }

                ODataRoutingMetadata odataMetadata = new ODataRoutingMetadata(prefix, model, path)
                {
                    TemplateDisplayName = string.IsNullOrEmpty(prefix) ? display : $"{prefix}/{display}"
                };

                AddHttpMethod(odataMetadata, httpMethod);

                selectorModel.EndpointMetadata.Add(odataMetadata);

                string templateStr = string.IsNullOrEmpty(prefix) ? template : $"{prefix}/{template}";

                // OData convention route template doesn't get combined with the route template applied to the controller.
                // Route templates applied to an action that begin with / or ~/ don't get combined with route templates applied to the controller.
                templateStr = "/" + templateStr;

                selectorModel.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(templateStr) { Name = templateStr });

                // Check with .NET Team whether the "Endpoint name metadata"
                selectorModel.EndpointMetadata.Add(new EndpointNameMetadata(Guid.NewGuid().ToString()));
            }
        }

        // this method refers to the similar method in ASP.NET Core
        internal static SelectorModel CreateSelectorModel(IReadOnlyList<object> attributes)
        {
            var selectorModel = new SelectorModel();

            AddRange(selectorModel.ActionConstraints, attributes.OfType<IActionConstraintMetadata>());
            AddRange(selectorModel.EndpointMetadata, attributes);

            // Simple case, all HTTP method attributes apply
            var httpMethods = attributes
                .OfType<IActionHttpMethodProvider>()
                .SelectMany(a => a.HttpMethods)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (httpMethods.Length > 0)
            {
                selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
                selectorModel.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods));
            }

            return selectorModel;
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

        private static void AddRange<T>(IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}

