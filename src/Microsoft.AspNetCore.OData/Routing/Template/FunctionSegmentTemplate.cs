// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a bound <see cref="IEdmFunction"/>.
    /// </summary>
    public class FunctionSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="function">The Edm function, it should be bound function.</param>
        /// <param name="navigationSource">The Edm navigation source of this function return. It could be null.</param>
        public FunctionSegmentTemplate(IEdmFunction function, IEdmNavigationSource navigationSource)
            : this(function, navigationSource, function?.GetFunctionParamters())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="function">The Edm function, it should be bound function.</param>
        /// <param name="navigationSource">The Edm navigation source of this function return. It could be null.</param>
        /// <param name="requiredParameters">The required parameters of this function.</param>
        public FunctionSegmentTemplate(IEdmFunction function, IEdmNavigationSource navigationSource, ISet<string> requiredParameters)
        {
            Function = function ?? throw Error.ArgumentNull(nameof(function));

            NavigationSource = navigationSource;

            RequiredParameters = requiredParameters ?? throw Error.ArgumentNull(nameof(requiredParameters));

            // Only accept the bound function
            if (!function.IsBound)
            {
                throw new ODataException(string.Format(CultureInfo.CurrentCulture, SRResources.FunctionIsNotBound, function.Name));
            }

            // make sure the input parameter is subset of the function paremeters.
            ISet<string> functionParameters = function.GetFunctionParamters();
            if (!requiredParameters.IsSubsetOf(functionParameters))
            {
                string required = string.Join(",", requiredParameters);
                string actual = string.Join(",", functionParameters);
                throw new ODataException(Error.Format(SRResources.RequiredParametersNotSubsetOfFunctionParameters, required, actual));
            }

            // Join the parameters as p1={p1}
            string parameters = "(" + string.Join(",", requiredParameters.Select(a => $"{a}={{{a}}}")) + ")";

            UnqualifiedIdentifier = function.Name + parameters;

            Literal = function.FullName() + parameters;

            // Function will always have the return type
            IsSingle = function.ReturnType.TypeKind() != EdmTypeKind.Collection;
        }

        /// <inheritdoc />
        public override string Literal { get; }

        /// <inheritdoc />
        public override IEdmType EdmType => Function.ReturnType.Definition;

        /// <summary>
        /// Gets the wrapped Edm function.
        /// </summary>
        public IEdmFunction Function { get; }

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Key=value, Key=value
        /// </summary>
        internal string UnqualifiedIdentifier { get; }

        internal bool HasOptional { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Function;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <summary>
        /// Gets the required parameter names.
        /// </summary>
        public ISet<string> RequiredParameters { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            RouteValueDictionary routeValue = context.RouteValues;

            // TODO: process the parameter alias
            int skip = Function.IsBound ? 1 : 0;

            //if (!TestParameters(context, out IDictionary<string, string> actualParameters))
            //{
            //    return null;
            //}

            if (!IsAllParameters(routeValue))
            {
                if (!TestParameters(context, out IDictionary<string, string> actualParameters))
                {
                    return null;
                }

                routeValue = new RouteValueDictionary();
                foreach (var item in context.RouteValues)
                {
                    routeValue.Add(item.Key, item.Value);
                }

                // Replace
                foreach (var item in actualParameters)
                {
                    routeValue[item.Key] = item.Value;
                }
            }

            IList<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>();
            foreach (var parameter in Function.Parameters.Skip(skip))
            {
                if (routeValue.TryGetValue(parameter.Name, out object rawValue))
                {
                    // for resource or collection resource, this method will return "ODataResourceValue, ..." we should support it.
                    if (parameter.Type.IsResourceOrCollectionResource())
                    {
                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameter.Name;
                        routeValue[prefixName] = new ODataParameterValue(rawValue, parameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameter.Name, rawValue));
                    }
                    else
                    {
                        string strValue = rawValue as string;
                        object newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, context.Model, parameter.Type);

                        // for without FromODataUri, so update it, for example, remove the single quote for string value.
                        routeValue[parameter.Name] = newValue;

                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameter.Name;
                        routeValue[prefixName] = new ODataParameterValue(newValue, parameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameter.Name, newValue));
                    }
                }
            }

            return new OperationSegment(Function, parameters, NavigationSource as IEdmEntitySetBase);
        }

        internal bool IsAllParameters(RouteValueDictionary routeValue)
        {
            foreach (var parameter in RequiredParameters)
            {
                if (!routeValue.ContainsKey(parameter))
                {
                    return false;
                }
            }

            return true;
        }

        private bool TestParameters(ODataTemplateTranslateContext context, out IDictionary<string, string> actualParameters)
        {
            // If we have a function(p1, p2, p3), where p3 is optinal parameter.
            // In controller, we may have two functions:
            // IActionResult function(p1, p2)   --> #1
            // IActionResult function(p1, p2, p3)  --> #2
            // #1  can match ~/function(p1=a, p2=b) , where p1=a, p2=b
            // It also match ~/function(p1=a,p2=b,p3=c), where p2="b,p3=c".
            // In this case, we should skip this.
            // Here is a workaround:
            // 1) We get all the parameters from the function and all parameter values from routeValue.
            // Combine them as a string. so, actualParameters = "p1=a,p2=b,p3=c"
            var routeValue = context.RouteValues;

            int skip = Function.IsBound ? 1 : 0;
            actualParameters = new Dictionary<string, string>();
            foreach (var parameter in Function.Parameters.Skip(skip))
            {
                if (routeValue.TryGetValue(parameter.Name, out object rawValue))
                {
                    actualParameters[parameter.Name] = rawValue as string;
                }
            }

            if (!actualParameters.Any())
            {
                if (RequiredParameters.Any())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            string combintes = string.Join(",", actualParameters.Select(kvp => kvp.Key + "=" + kvp.Value));

            // 2) Extract the key/value pairs
            //   p1=a    p2=b    p3=c
            if (!combintes.TryExtractKeyValuePairs(out actualParameters))
            {
                return false;
            }

            // 3) now the RequiredParameters (p1, p3) is not equal to actualParameters (p1, p2, p3)
            return RequiredParameters.SetEquals(actualParameters.Keys);
        }
    }
}
