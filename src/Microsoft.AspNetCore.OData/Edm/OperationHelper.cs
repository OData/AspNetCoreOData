//-----------------------------------------------------------------------------
// <copyright file="OperationHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Edm;

/// <summary>
/// Helpers for Edm operation.
/// </summary>
internal static class OperationHelper
{
    /// <summary>
    /// Verify and build the function parameters
    /// </summary>
    /// <param name="function">The Edm function.</param>
    /// <param name="parameters">The input parameter template mapping.</param>
    /// <returns>The build function parameter mapping.</returns>
    public static IDictionary<string, string> VerifyAndBuildParameterMappings(this IEdmFunction function, IDictionary<string, string> parameters)
    {
        if (function == null)
        {
            throw Error.ArgumentNull(nameof(function));
        }

        if (parameters == null)
        {
            throw Error.ArgumentNull(nameof(parameters));
        }

        Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

        int skip = function.IsBound ? 1 : 0;
        ISet<string> funcParameters = new HashSet<string>();
        foreach (var parameter in function.Parameters.Skip(skip))
        {
            funcParameters.Add(parameter.Name);

            IEdmOptionalParameter optionalParameter = parameter as IEdmOptionalParameter;
            if (optionalParameter != null)
            {
                // skip verification for optional parameter
                continue;
            }

            // for required parameter, it should be in the parameter template mapping.
            if (!parameters.ContainsKey(parameter.Name))
            {
                throw new ODataException(Error.Format(SRResources.MissingRequiredParameterInOperation, parameter.Name, function.FullName()));
            }
        }

        foreach (var parameter in parameters)
        {
            if (!funcParameters.Contains(parameter.Key))
            {
                throw new ODataException(Error.Format(SRResources.CannotFindParameterInOperation, parameter.Key, function.FullName()));
            }

            string templateName = parameter.Value;
            if (templateName == null || !templateName.IsValidTemplateLiteral())
            {
                throw new ODataException(Error.Format(SRResources.ParameterTemplateMustBeInCurlyBraces, parameter.Value, function.FullName()));
            }

            templateName = templateName.Substring(1, templateName.Length - 2).Trim();
            if (string.IsNullOrEmpty(templateName))
            {
                throw new ODataException(Error.Format(SRResources.EmptyParameterAlias, parameter.Key, function.FullName()));
            }

            parameterMappings[parameter.Key] = templateName;
        }

        return parameterMappings;
    }

    /// <summary>
    /// Build the function parameter mapping.
    /// </summary>
    /// <param name="parameters">The given function parameter</param>
    /// <param name="segment">The segment string.</param>
    /// <returns>The build function parameter mapping.</returns>
    public static IDictionary<string, string> BuildParameterMappings(this IEnumerable<OperationSegmentParameter> parameters, string segment)
    {
        if (parameters == null)
        {
            throw Error.ArgumentNull(nameof(parameters));
        }

        Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

        foreach (OperationSegmentParameter parameter in parameters)
        {
            string parameterName = parameter.Name;
            string nameInRouteData = null;

            ConstantNode node = parameter.Value as ConstantNode;
            if (node != null)
            {
                UriTemplateExpression uriTemplateExpression = node.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    nameInRouteData = uriTemplateExpression.LiteralText.Trim();
                }
            }
            else
            {
                // Just for easy constructor the function parameters
                nameInRouteData = parameter.Value as string;
            }

            if (nameInRouteData == null || !nameInRouteData.IsValidTemplateLiteral())
            {
                throw new ODataException(Error.Format(SRResources.ParameterTemplateMustBeInCurlyBraces, parameter.Value, segment));
            }

            nameInRouteData = nameInRouteData.Substring(1, nameInRouteData.Length - 2).Trim();
            if (string.IsNullOrEmpty(nameInRouteData))
            {
                throw new ODataException(Error.Format(SRResources.EmptyParameterAlias, parameter.Name, segment));
            }

            parameterMappings[parameterName] = nameInRouteData;
        }

        return parameterMappings;
    }

    /// <summary>
    /// Gets the function parameter sets.
    /// </summary>
    /// <param name="function">The input function.</param>
    /// <returns>The set of parameter name.</returns>
    public static IDictionary<string, string> GetFunctionParamterMappings(this IEdmFunction function)
    {
        if (function == null)
        {
            throw Error.ArgumentNull(nameof(function));
        }

        int skip = function.IsBound ? 1 : 0;
        return function.Parameters.Skip(skip).ToDictionary(p => p.Name, p => $"{{{p.Name}}}");
    }

    /// <summary>
    /// Gets the function import parameter sets.
    /// </summary>
    /// <param name="functionImport">The input function import.</param>
    /// <returns>The set of parameter name.</returns>
    public static IDictionary<string, string> GetFunctionParamterMappings(this IEdmFunctionImport functionImport)
    {
        if (functionImport == null)
        {
            throw Error.ArgumentNull(nameof(functionImport));
        }

        return functionImport.Function.Parameters.ToDictionary(p => p.Name, p => $"{{{p.Name}}}");
    }

    /// <summary>
    /// Split the operation into function and action.
    /// </summary>
    /// <param name="operationImports">The operation imports</param>
    /// <returns></returns>
    public static (IList<IEdmActionImport>, IList<IEdmFunctionImport>) SplitOperationImports(this IEnumerable<IEdmOperationImport> operationImports)
    {
        if (operationImports == null)
        {
            throw Error.ArgumentNull(nameof(operationImports));
        }

        IList<IEdmActionImport> actions = new List<IEdmActionImport>();
        IList<IEdmFunctionImport> functions = new List<IEdmFunctionImport>();
        foreach (var import in operationImports)
        {
            if (import.IsActionImport())
            {
                actions.Add((IEdmActionImport)import);
            }
            else
            {
                functions.Add((IEdmFunctionImport)import);
            }
        }

        return (actions, functions);
    }
}
