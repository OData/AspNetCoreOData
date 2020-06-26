// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public class FunctionSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="function">The type containes the key.</param>
        public FunctionSegmentTemplate(IEdmFunction function)
            : this(function, unqualifiedFunctionCall: false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function">The type containes the key.</param>
        /// <param name="unqualifiedFunctionCall">Unqualified function call boolean value.</param>
        public FunctionSegmentTemplate(IEdmFunction function, bool unqualifiedFunctionCall)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));

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
            }

            if (unqualifiedFunctionCall)
            {
                Template = function.Name + "(" + string.Join(",", parametersMappings.Select(a => $"{a.Key}={a.Value}")) + ")";
            }
            else
            {
                Template = function.FullName() + "(" + string.Join(",", parametersMappings.Select(a => $"{a.Key}={a.Value}")) + ")";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEdmFunction Function { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model, IEdmNavigationSource previous,
            RouteValueDictionary routeValue, QueryString queryString)
        {
            // TODO: process the parameter alias
            int skip = Function.IsBound ? 1 : 0;

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
                        object newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, model, parameter.Type);

                        // for without FromODataUri, so update it, for example, remove the single quote for string value.
                        routeValue[parameter.Name] = newValue;

                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameter.Name;
                        routeValue[prefixName] = new ODataParameterValue(newValue, parameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameter.Name, newValue));
                    }
                }
            }

            IEdmNavigationSource targetset = Function.GetTargetEntitySet(previous, model);

            return new OperationSegment(Function, parameters, targetset as IEdmEntitySetBase);
        }
    }
}
