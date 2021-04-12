// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
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
        /// Adds the OData selector model to the action.
        /// </summary>
        /// <param name="action">The given action model.</param>
        /// <param name="httpMethods">The supported http methods, if mulitple, using ',' to separate.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The OData path template.</param>
        /// <param name="options">The route build options.</param>
        public static void AddSelector(this ActionModel action, string httpMethods, string prefix, IEdmModel model, ODataPathTemplate path, ODataRouteOptions options = null)
        {
            if (action == null)
            {
                throw Error.ArgumentNull(nameof(action));
            }

            if (string.IsNullOrEmpty(httpMethods))
            {
                throw Error.ArgumentNullOrEmpty(nameof(httpMethods));
            }

            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            // If the methods have different case sensitive, for example, "get", "Get", in the ASP.NET Core 3.1,
            // It will throw "An item with the same key has already been added. Key: GET", in
            // HttpMethodMatcherPolicy.BuildJumpTable(Int32 exitDestination, IReadOnlyList`1 edges)
            // Another root cause is that in attribute routing, we reuse the HttpMethodMetadata, the method name is always "upper" case.
            // Therefore, we upper the http method name always.
            string[] methods = httpMethods.ToUpperInvariant().Split(',');
            foreach (string template in path.GetTemplates(options))
            {
                // Be noted: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ApplicationModels/ActionAttributeRouteModel.cs#L74-L75
                // No matter whether the action selector model is absolute route template, the controller's attribute will apply automatically
                // So, let's only create/update the action selector model
                SelectorModel selectorModel = action.Selectors.FirstOrDefault(s => s.AttributeRouteModel == null);
                if (selectorModel == null)
                {
                    // Create a new selector model.
                    selectorModel = CreateSelectorModel(action, methods);
                    action.Selectors.Add(selectorModel);
                }
                else
                {
                    // Update the existing non attribute routing selector model.
                    selectorModel = UpdateSelectorModel(selectorModel, methods);
                }

                ODataRoutingMetadata odataMetadata = new ODataRoutingMetadata(prefix, model, path);
                selectorModel.EndpointMetadata.Add(odataMetadata);

                string templateStr = string.IsNullOrEmpty(prefix) ? template : $"{prefix}/{template}";

                selectorModel.AttributeRouteModel = new AttributeRouteModel
                {
                    // OData convention route template doesn't get combined with the route template applied to the controller.
                    // Route templates applied to an action that begin with / or ~/ don't get combined with route templates applied to the controller.
                    Template = $"/{templateStr}",
                    Name = templateStr // do we need this?
                };

                // Check with .NET Team whether the "Endpoint name metadata" needed?
                selectorModel.EndpointMetadata.Add(new EndpointNameMetadata(Guid.NewGuid().ToString())); // Do we need this?
            }
        }

        internal static SelectorModel UpdateSelectorModel(SelectorModel selectorModel, string[] httpMethods)
        {
            Contract.Assert(selectorModel != null);

            // remove the unused constraints (just for safe)
            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is IRouteTemplateProvider)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is HttpMethodActionConstraint)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            // remove the unused metadata
            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IRouteTemplateProvider)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IHttpMethodMetadata)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            // append the http method metadata.
            Contract.Assert(httpMethods.Length >= 1);
            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
            selectorModel.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods));

            // append controller attributes to action selector model? -- NO
            // Be noted: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ApplicationModels/ActionAttributeRouteModel.cs#L74-L75

            return selectorModel;
        }

        internal static SelectorModel CreateSelectorModel(ActionModel actionModel, string[] httpMethods)
        {
            Contract.Assert(actionModel != null);

            SelectorModel selectorModel = new SelectorModel();
            IReadOnlyList<object> attributes = actionModel.Attributes;

            AddRange(selectorModel.ActionConstraints, attributes.OfType<IActionConstraintMetadata>());

            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is IRouteTemplateProvider)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is HttpMethodActionConstraint)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            AddRange(selectorModel.EndpointMetadata, attributes);
            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IRouteTemplateProvider)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IHttpMethodMetadata)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            Contract.Assert(httpMethods.Length >= 1);
            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
            selectorModel.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods));

            // append controller attributes to action selector model? -- NO
            // Be noted: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ApplicationModels/ActionAttributeRouteModel.cs#L74-L75
            return selectorModel;
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
