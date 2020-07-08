// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Edm
{
    internal static class OperationHelper
    {
        public static (IList<IEdmActionImport>, IList<IEdmFunctionImport>) Split(this IEnumerable<IEdmOperationImport> operationImports)
        {
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

        public static string TargetName(this IEdmOperation operation)
        {
            if (operation.IsFunction())
            {
                IEdmFunction function = (IEdmFunction)operation;
                int skip = 0;
                if (function.IsBound)
                {
                    skip = 1;
                }

                return function.Name + "(" + String.Join(",", function.Parameters.Skip(skip).Select(p => p.Name)) + ")";
            }
            else
                return operation.Name; ;
        }

        public static IEdmOperation ResolveOperations(string identifer, IList<string> parameterNames, IEdmType bindingType, IEdmModel model,
            bool enableCaseInsensitive)
        {
            IEnumerable<IEdmOperation> candidates = model.ResolveOperations(identifer, bindingType, enableCaseInsensitive);
            if (!candidates.Any())
            {
                return null;
            }

            bool hasParameters = parameterNames.Count() > 0;
            if (hasParameters)
            {
                candidates = candidates.Where(o => o.IsFunction()).FilterOperationsByParameterNames(parameterNames, true); // remove actions
            }
            else if (bindingType != null)
            {
                // Filter out functions with more than one parameter. Actions should not be filtered as the parameters are in the payload not the uri
                candidates = candidates.Where(o =>
                (o.IsFunction() && (o.Parameters.Count() == 1 || o.Parameters.Skip(1).All(p => p is IEdmOptionalParameter))) || o.IsAction());
            }
            else
            {
                // Filter out functions with any parameters
                candidates = candidates.Where(o => (o.IsFunction() && !o.Parameters.Any()) || o.IsAction());
            }

            // Only filter if there is more than one and its needed.
            if (candidates.Count() > 1)
            {
                // candidates = candidates.FilterBoundOperationsWithSameTypeHierarchyToTypeClosestToBindingType(bindingType);
            }

            if (candidates.Any(f => f.IsAction()))
            {
                return candidates.Single();
            }

            // If more than one overload matches, try to select based on optional parameters
            if (candidates.Count() > 1)
            {
                // candidates = candidates.FindBestOverloadBasedOnParameters(parameterNames);
            }

            if (candidates.Count() > 1)
            {
                throw new Exception("TODO:");
            }

            return candidates.First();
        }

        public static IEdmOperationImport ResolveOperationImports(string identifer, IList<string> parameterNames, IEdmModel model,
            bool enableCaseInsensitive = false)
        {
            IEnumerable<IEdmOperationImport> candidates = model.ResolveOperationImports(identifer, enableCaseInsensitive);
            if (!candidates.Any())
            {
                return null;
            }

            // only action import, without (...)
            if (parameterNames == null)
            {
                candidates = candidates.Where(i => i.IsActionImport());
                if (candidates.Count() > 1)
                {
                    throw new Exception($"Multiple action import overloads for '{identifer}' were found.");
                }

                return candidates.SingleOrDefault();
            }
            else
            {
                candidates = candidates.Where(i => i.IsFunctionImport()).FilterOperationImportsByParameterNames(parameterNames, true);
            }

            // If parameter count is zero and there is one function import whoese parameter count is zero, return this function import.
            if (candidates.Count() > 1 && parameterNames.Count == 0)
            {
                candidates = candidates.Where(operationImport => operationImport.Operation.Parameters.Count() == 0);
            }

            if (!candidates.Any())
            {
                return null;
            }

            if (candidates.Count() > 1)
            {
                throw new Exception($"Multiple function import overloads for '{identifer}' were found.");
            }

            return candidates.First();
        }

        internal static IEnumerable<IEdmOperation> FilterOperationsByParameterNames(this IEnumerable<IEdmOperation> operations, IEnumerable<string> parameters,
            bool enableCaseInsensitive)
        {
            IList<string> parameterNameList = parameters.ToList();

            // TODO: update code that is duplicate between operation and operation import, add more tests.
            foreach (IEdmOperation operation in operations)
            {
                if (!ParametersSatisfyFunction(operation, parameterNameList, enableCaseInsensitive))
                {
                    continue;
                }

                yield return operation;
            }
        }

        internal static IEnumerable<IEdmOperationImport> FilterOperationImportsByParameterNames(this IEnumerable<IEdmOperationImport> operationImports,
            IEnumerable<string> parameterNames, bool enableCaseInsensitive)
        {
            IList<string> parameterNameList = parameterNames.ToList();

            foreach (IEdmOperationImport operationImport in operationImports)
            {
                if (!ParametersSatisfyFunction(operationImport.Operation, parameterNameList, enableCaseInsensitive))
                {
                    continue;
                }

                yield return operationImport;
            }
        }

        private static bool ParametersSatisfyFunction(IEdmOperation operation, IList<string> parameterNameList, bool enableCaseInsensitive)
        {
            IEnumerable<IEdmOperationParameter> parametersToMatch = operation.Parameters;

            // bindable functions don't require the first parameter be specified, since its already implied in the path.
            if (operation.IsBound)
            {
                parametersToMatch = parametersToMatch.Skip(1);
            }

            List<IEdmOperationParameter> functionParameters = parametersToMatch.ToList();

            // if any required parameters are missing, don't consider it a match.
            if (functionParameters.Where(
                p => !(p is IEdmOptionalParameter)).Any(p => parameterNameList.All(k => !string.Equals(k, p.Name,
                enableCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
                ))))
            {
                return false;
            }

            // if any specified parameters don't match, don't consider it a match.
            if (parameterNameList.Any(k => functionParameters.All(p => !string.Equals(k, p.Name,
                enableCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))))
            {
                return false;
            }

            return true;
        }
    }
}
