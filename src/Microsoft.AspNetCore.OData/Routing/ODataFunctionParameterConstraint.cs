// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts.Annotations;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// 
    /// </summary>
    public class ODataFunctionParameterConstraint : IRouteConstraint
    {
        private IEdmModel _model;
        private IEdmFunction _function;
        private ODataRoutingOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="modelName"></param>
        /// <param name="fullFunctionName"></param>
        /// <param name="isBound"></param>
        /// <param name="parameters"></param>
        public ODataFunctionParameterConstraint(
            IOptions<ODataRoutingOptions> options,
            string modelName, string fullFunctionName, bool isBound, string parameters)
        {
            _options = options.Value;

            _model = FindModel(modelName);

            var functions = _model.SchemaElements.OfType<IEdmFunction>().Where(f => f.IsBound == isBound && f.FullName() == fullFunctionName);

            _function = FindFunction(functions, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="route"></param>
        /// <param name="routeKey"></param>
        /// <param name="values"></param>
        /// <param name="routeDirection"></param>
        /// <returns></returns>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // {[parameters, minSalary=2,maxSalary=3]} ???
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                // Get Route key from values?
                //
                return true;
            }

            return false;
        }

        private IEdmModel FindModel(string modelName)
        {
            foreach (var route in _options.Models)
            {
                string name = route.Value.GetModelName();

                if (name == modelName)
                {
                    return route.Value;
                }
            }

            return null;
        }

        private static IEdmFunction FindFunction(IEnumerable<IEdmFunction> functions, string parameters)
        {
            HashSet<string> parameterNames = new HashSet<string>(parameters.Split(";"));
            int count = parameterNames.Count;
            foreach (var function in functions)
            {
                if (function.Parameters.Count() != count + 1)
                {
                    continue;
                }

                bool match = true;
                foreach (var parameter in function.Parameters.Skip(1))
                {
                    if (!parameterNames.Contains(parameter.Name))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return function;
                }
            }

            return null;
        }
    }
}
