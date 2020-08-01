// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        /// <param name="function">The Edm function, it should be bound function..</param>
        /// <param name="navigationSource">The Edm navigation source of this function return.</param>
        public FunctionSegmentTemplate(IEdmFunction function, IEdmNavigationSource navigationSource)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));

            NavigationSource = navigationSource;

            if (!function.IsBound)
            {
                // TODO: shall we need this check?
                throw new InvalidOperationException($"The input function {function.Name} is not a bound function.");
            }

            int skip = function.IsBound ? 1 : 0;

            IDictionary<string, string> parametersMappings = new Dictionary<string, string>();
            foreach (var parameter in function.Parameters.Skip(skip))
            {
                parametersMappings[parameter.Name] = $"{{{parameter.Name}}}";

                if (parameter is IEdmOptionalParameter)
                {
                    HasOptional = true;
                }
            }

            if (HasOptional)
            {
                string parameters = string.Join(";", parametersMappings.Select(a => a.Key));
                string constaint = $"({{parameters:OdataFunctionParameters(MODELNAME,{function.FullName()},true,{parameters})}})";

               // string constaint = "({parameters:OdataFunctionParameters(abc,efg,true,ijk)})";
                UnqualifiedIdentifier = function.Name + constaint;
                Literal = function.FullName() + constaint;
            }
            else
            {
                string parameters = "(" + string.Join(",", parametersMappings.Select(a => $"{a.Key}={a.Value}")) + ")";
                UnqualifiedIdentifier = function.Name + parameters;
                Literal = function.FullName() + parameters;
            }
/* 
            string parameters = "(" + string.Join(",", parametersMappings.Select(a => $"{a.Key}={a.Value}")) + ")";
            UnqualifiedIdentifier = function.Name + parameters;
            Literal = function.FullName() + parameters;
*/
            IsSingle = function.ReturnType.TypeKind() != EdmTypeKind.Collection;

            RequiredParameters = new HashSet<string>(parametersMappings.Select(e => e.Key));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="function">The Edm function.</param>
        /// <param name="navigationSource">Unqualified function call boolean value.</param>
        internal FunctionSegmentTemplate(IEdmFunction function, IEdmNavigationSource navigationSource, ISet<string> requiredParameters)
            : this (function, navigationSource)
        {
            RequiredParameters = requiredParameters;

            string parameters = "(" + string.Join(",", requiredParameters.Select(a => $"{a}={{{a}}}")) + ")";
            UnqualifiedIdentifier = function.Name + parameters;
            Literal = function.FullName() + parameters;
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

        internal ISet<string> RequiredParameters { get; set; }

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

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            var routeValue = context.RouteValues;
            // TODO: process the parameter alias
            int skip = Function.IsBound ? 1 : 0;

            if (!TestParameters(context, out IDictionary<string, string> actualParameters))
            {
                return null;
            }

            if (!IsAllParameters(routeValue))
            {
                return null;
            }

            if (HasOptional)
            {
                if (routeValue.TryGetValue("parameters", out object value))
                {
                    string parametersStr = value as string;
                    if (!parametersStr.TryExtractKeyValuePairs(out IDictionary<string, string> pairs))
                    {
                        return null;
                    }

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

       //     IEdmNavigationSource targetset = Function.GetTargetEntitySet(previous, model);

            return new OperationSegment(Function, parameters, NavigationSource as IEdmEntitySetBase);
        }


        private bool TestParameters(ODataTemplateTranslateContext context, out IDictionary<string, string> actualParameters)
        {
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

            string combintes = string.Join(",", actualParameters.Select(kvp => kvp.Key + "=" + kvp.Value));

            if (!combintes.TryExtractKeyValuePairs(out actualParameters))
            {
                return false;
            }

            return RequiredParameters.SetEquals(actualParameters.Keys);
        }
    }
}
