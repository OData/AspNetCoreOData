// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    /// Helper methods for segment template
    /// </summary>
    internal static class SegmentTemplateHelpers
    {
        /// <summary>
        /// Builds the route key. The route key pattern:  parameterTemp;parameterTemp;parameterTemp;...
        /// It could be an empty string if this function has no parameter
        /// RouteKey is used to retrieve the route value from request route data.
        /// </summary>
        /// <param name="parameterMappings">The parameter (request key) and argument (parameter name in the action) mapping.</param>
        /// <returns>the route key string.</returns>
        public static string BuildRouteKey(this IDictionary<string, string> parameterMappings)
        {
            if (parameterMappings == null)
            {
                throw Error.ArgumentNull(nameof(parameterMappings));
            }

            // use the template name directly
            return string.Join(";", parameterMappings.Select(p => p.Value));
        }

        /// <summary>
        /// Try to parse the route key value from route values and save the parsed values into update values.
        /// </summary>
        /// <param name="routeValues">The original route value from request.</param>
        /// <param name="updateValues">The updated/parsed route value.</param>
        /// <param name="parameterMappings">The parameter (request key) and argument (parameter name in the action) mapping.</param>
        /// <returns>true/false.</returns>
        public static bool TryParseRouteKey(RouteValueDictionary routeValues, RouteValueDictionary updateValues, IDictionary<string, string> parameterMappings)
        {
            if (routeValues == null)
            {
                throw Error.ArgumentNull(nameof(routeValues));
            }

            if (updateValues == null)
            {
                throw Error.ArgumentNull(nameof(updateValues));
            }

            // format the route key
            string routeKey = parameterMappings.BuildRouteKey();

            if (routeValues.TryGetValue(routeKey, out var value) && value != null)
            {
                string parameterValueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (parameterValueString == null)
                {
                    return false;
                }

                // Parse the string from request into key/value pairs
                if (KeyValuePairParser.TryParse(parameterValueString, out IDictionary<string, string> parsedKeyValues))
                {
                    // for single key pattern
                    if (parsedKeyValues.Count == 1 && parsedKeyValues.ContainsKey(string.Empty) &&
                        parameterMappings.Count == 1)
                    {
                        updateValues[parameterMappings.First().Value] = parsedKeyValues[string.Empty];
                        return true;
                    }

                    // We should compare the parameters from request and parameter/template mapping.
                    if (!IsRouteKeyAndParameterMatch(parameterMappings, parsedKeyValues))
                    {
                        return false;
                    }

                    foreach (var parameter in parsedKeyValues)
                    {
                        // We don't need to verify {parameter.Key} whether it's in the parameterMappings or not.
                        // Because we did it in "IsRouteKeyAndParameterMatch" method
                        string templateName = parameterMappings[parameter.Key];
                        updateValues[templateName] = parameter.Value;
                    }

                    return true;
                }
            }

            return false;
        }

        public static IList<OperationSegmentParameter> Match(ODataTemplateTranslateContext context, IEdmFunction function,
           IDictionary<string, string> parameterMappings)
        {
            Contract.Assert(context != null);
            Contract.Assert(function != null);
            Contract.Assert(parameterMappings != null);

            RouteValueDictionary routeValues = context.UpdatedValues;

            IList<OperationSegmentParameter> parameters = new List<OperationSegmentParameter>();
            foreach (var parameter in parameterMappings)
            {
                string parameterName = parameter.Key;
                string parameterTemp = parameter.Value;

                IEdmOperationParameter edmParameter = function.Parameters.FirstOrDefault(p => p.Name == parameterName);
                Contract.Assert(edmParameter != null);

                // For a parameter mapping like: minSalary={min}
                // and a request like: ~/MyFunction(minSalary=2)
                // the routeValue includes the [min=2], so we should use the mapping name to retrieve the value.
                if (routeValues.TryGetValue(parameterTemp, out object rawValue))
                {
                    string strValue = rawValue as string;
                    string newStrValue = context.GetParameterAliasOrSelf(strValue);
                    if (newStrValue != strValue)
                    {
                        routeValues[parameterTemp] = newStrValue;
                        strValue = newStrValue;
                    }

                    // for resource or collection resource, this method will return "ODataResourceValue, ..." we should support it.
                    if (edmParameter.Type.IsResourceOrCollectionResource())
                    {
                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameterTemp;
                        routeValues[prefixName] = new ODataParameterValue(strValue, edmParameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameterName, strValue));
                    }
                    else
                    {
                        object newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, context.Model, edmParameter.Type);

                        // for without FromODataUri, so update it, for example, remove the single quote for string value.
                        routeValues[parameterTemp] = newValue;

                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameterTemp;
                        routeValues[prefixName] = new ODataParameterValue(newValue, edmParameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameterName, newValue));
                    }
                }
                else
                {
                    return null;
                }
            }

            return parameters;
        }

        internal static bool IsMatchParameters(RouteValueDictionary routeValues, IDictionary<string, string> parameterMappings)
        {
            Contract.Assert(routeValues != null);
            Contract.Assert(parameterMappings != null);

            // If we have a function(p1, p2, p3), where p3 is optinal parameter.
            // In controller, we may have two functions:
            // IActionResult function(p1, p2)   --> #1
            // IActionResult function(p1, p2, p3)  --> #2
            // #1  can match request like: ~/function(p1=a, p2=b) , where p1=a, p2=b   (----a)
            // It also match request like: ~/function(p1=a,p2=b,p3=c), where p2="b,p3=c".  (----b)
            // However, b request should match the #2 method and skip the #1 method.
            // Here is a workaround:
            // 1) We get all the parameters from the function and all parameter values from routeValue.
            // Combine them as a string. so, actualParameters = "p1=a,p2=b,p3=c"

            IDictionary<string, string>  actualParameters = new Dictionary<string, string>();
            foreach (var parameter in parameterMappings)
            {
                // For a parameter mapping like: minSalary={min}
                // and a request like: ~/MyFunction(minSalary=2)
                // the routeValue includes the [min=2], so we should use the mapping name to retrieve the value.
                string parameterTemp = parameter.Value;
                if (routeValues.TryGetValue(parameterTemp, out object rawValue))
                {
                    actualParameters[parameterTemp] = rawValue as string;
                }
            }

            if (!actualParameters.Any())
            {
                if (parameterMappings.Any())
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
            return parameterMappings.Count == actualParameters.Keys.Count;
        }

        /// <summary>
        /// Compare the route key and the key/value from request
        /// </summary>
        /// <param name="parameterMappings">The route key map.</param>
        /// <param name="requestKeyValues">The key value map from request.</param>
        /// <returns>True/False.</returns>
        private static bool IsRouteKeyAndParameterMatch(IDictionary<string, string> parameterMappings, IDictionary<string, string> requestKeyValues)
        {
            Contract.Assert(parameterMappings != null);
            Contract.Assert(requestKeyValues != null);

            // the number of each dict should match
            if (parameterMappings.Count != requestKeyValues.Count)
            {
                return false;
            }

            // We can convert the keys into HashSet and use SetEquals to compare the keys as
            // parameterMappings.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Shall we ignore the string case? if yes, we can use "StringComparer.OrdinalIgnoreCase"
            ISet<string> paremterKeySet = parameterMappings.Keys.ToHashSet();
            ISet<string> requestKeySet = requestKeyValues.Keys.ToHashSet();

            return paremterKeySet.SetEquals(requestKeySet);
        }
    }
}
