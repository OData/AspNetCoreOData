//-----------------------------------------------------------------------------
// <copyright file="SegmentTemplateHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Helper methods for segment template
    /// </summary>
    internal static class SegmentTemplateHelpers
    {
        /// <summary>
        /// Match the function parameter
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="function">The Edm function.</param>
        /// <param name="parameterMappings">The parameter mapping.</param>
        /// <returns></returns>
        public static IList<OperationSegmentParameter> Match(ODataTemplateTranslateContext context,
            IEdmFunction function,
            IDictionary<string, string> parameterMappings)
        {
            Contract.Assert(context != null);
            Contract.Assert(function != null);
            Contract.Assert(parameterMappings != null);

            RouteValueDictionary routeValues = context.RouteValues;
            RouteValueDictionary updatedValues = context.UpdatedValues;

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
                    newStrValue = Uri.UnescapeDataString(newStrValue);
                    if (newStrValue != strValue)
                    {
                        updatedValues[parameterTemp] = newStrValue;
                        strValue = newStrValue;
                    }

                    string originalStrValue = strValue;

                    // for resource or collection resource, this method will return "ODataResourceValue, ..." we should support it.
                    if (edmParameter.Type.IsResourceOrCollectionResource())
                    {
                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameterTemp;
                        updatedValues[prefixName] = new ODataParameterValue(strValue, edmParameter.Type);

                        parameters.Add(new OperationSegmentParameter(parameterName, strValue));
                    }
                    else
                    {
                        if (edmParameter.Type.IsEnum() && strValue.StartsWith("'", StringComparison.Ordinal) && strValue.EndsWith("'", StringComparison.Ordinal))
                        {
                            // related implementation at: https://github.com/OData/odata.net/blob/master/src/Microsoft.OData.Core/UriParser/Resolver/StringAsEnumResolver.cs#L131
                            strValue = edmParameter.Type.FullName() + strValue;
                        }

                        object newValue;
                        try
                        {
                            newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, context.Model, edmParameter.Type);
                        }
                        catch (ODataException ex)
                        {
                            string message = Error.Format(SRResources.InvalidParameterValueInUriFound, originalStrValue, edmParameter.Type.FullName());
                            throw new ODataException(message, ex);
                        }

                        // for without FromODataUri, so update it, for example, remove the single quote for string value.
                        updatedValues[parameterTemp] = newValue;

                        // For FromODataUri
                        string prefixName = ODataParameterValue.ParameterValuePrefix + parameterTemp;
                        updatedValues[prefixName] = new ODataParameterValue(newValue, edmParameter.Type);

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

        /// <summary>
        /// Match the parameters
        /// </summary>
        /// <param name="routeValues">The route values</param>
        /// <param name="parameterMappings">The parameter mappings.</param>
        /// <returns>True/False.</returns>
        internal static bool IsMatchParameters(RouteValueDictionary routeValues, IDictionary<string, string> parameterMappings)
        {
            Contract.Assert(routeValues != null);
            Contract.Assert(parameterMappings != null);

            // If we have a function(p1, p2, p3), where p3 is optional parameter.
            // In controller, we may have two functions:
            // IActionResult function(p1, p2)   --> #1
            // IActionResult function(p1, p2, p3)  --> #2
            // #1  can match request like: ~/function(p1=a, p2=b) , where p1=a, p2=b   (----a)
            // It also match request like: ~/function(p1=a,p2=b,p3=c), where p2="b,p3=c".  (----b)
            // However, b request should match the #2 method and skip the #1 method.
            // Here is a workaround:
            // 1) We get all the parameters from the function and all parameter values from routeValue.
            // Combine them as a string. so, actualParameters = "p1=a,p2=b,p3=c"

            IDictionary<string, string> actualParameters = new Dictionary<string, string>();
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

            string combinates = string.Join(",", actualParameters.Select(kvp => kvp.Key + "=" + kvp.Value));

            // 2) Extract the key/value pairs
            //   p1=a    p2=b    p3=c
            if (!KeyValuePairParser.TryParse(combinates, out IDictionary<string, string> parsedKeyValues))
            {
                return false;
            }

            // 3) now the parsedKeyValues (p1, p3) is not equal to actualParameters (p1, p2, p3)
            return parameterMappings.Count == parsedKeyValues.Count;
        }

        /// <summary>
        /// Gets the navigation source from an Edm operation.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="operation">The Edm operation.</param>
        /// <returns>
        /// The navigation source or null if the annotation indicating the mapping from an Edm operation to an entity set is not found.
        /// </returns>
        internal static IEdmNavigationSource GetNavigationSourceFromEdmOperation(IEdmModel model, IEdmOperation operation)
        {
            ReturnedEntitySetAnnotation entitySetAnnotation = model?.GetAnnotationValue<ReturnedEntitySetAnnotation>(operation);

            if (entitySetAnnotation != null)
            {
                return model.EntityContainer.FindEntitySet(entitySetAnnotation.EntitySetName);
            }

            return null;
        }
    }
}
