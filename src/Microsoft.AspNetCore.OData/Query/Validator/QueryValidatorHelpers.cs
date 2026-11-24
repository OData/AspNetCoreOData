//-----------------------------------------------------------------------------
// <copyright file="QueryValidatorHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Helper methods for validation.
/// </summary>
internal class QueryValidatorHelpers
{
    internal static void ValidateAnyAllExpressionDepth(QueryNode node, ODataValidationSettings settings, int currentDepth = 0)
    {
        // Count only any()/all() nesting; this walk does not enforce the validators' other query settings.
        if (node == null)
        {
            return;
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();

        IEnumerable<QueryNode> parameters;
        switch (node)
        {
            case LambdaNode lambda:
                if (currentDepth >= settings.MaxAnyAllExpressionDepth)
                {
                    throw new ODataException(Error.Format(SRResources.MaxAnyAllExpressionLimitExceeded, settings.MaxAnyAllExpressionDepth, "MaxAnyAllExpressionDepth"));
                }

                // This lambda's children receive the increased depth; siblings keep their parent's depth.
                currentDepth++;
                ValidateAnyAllExpressionDepth(lambda.Source, settings, currentDepth);
                ValidateAnyAllExpressionDepth(lambda.Body, settings, currentDepth);
                return;
            case BinaryOperatorNode binary:
                ValidateAnyAllExpressionDepth(binary.Left, settings, currentDepth);
                ValidateAnyAllExpressionDepth(binary.Right, settings, currentDepth);
                return;
            case UnaryOperatorNode unary:
                ValidateAnyAllExpressionDepth(unary.Operand, settings, currentDepth);
                return;
            case ConvertNode convert:
                ValidateAnyAllExpressionDepth(convert.Source, settings, currentDepth);
                return;
            case InNode inNode:
                ValidateAnyAllExpressionDepth(inNode.Left, settings, currentDepth);
                ValidateAnyAllExpressionDepth(inNode.Right, settings, currentDepth);
                return;
            case CountNode count:
                ValidateAnyAllExpressionDepth(count.Source, settings, currentDepth);
                ValidateAnyAllExpressionDepth(count.FilterClause?.Expression, settings, currentDepth);
                return;
            case SingleValuePropertyAccessNode property:
                ValidateAnyAllExpressionDepth(property.Source, settings, currentDepth);
                return;
            case CollectionPropertyAccessNode collectionProperty:
                ValidateAnyAllExpressionDepth(collectionProperty.Source, settings, currentDepth);
                return;
            case SingleValueOpenPropertyAccessNode openProperty:
                ValidateAnyAllExpressionDepth(openProperty.Source, settings, currentDepth);
                return;
            case CollectionOpenPropertyAccessNode collectionOpenProperty:
                ValidateAnyAllExpressionDepth(collectionOpenProperty.Source, settings, currentDepth);
                return;
            case SingleComplexNode complex:
                ValidateAnyAllExpressionDepth(complex.Source, settings, currentDepth);
                return;
            case CollectionComplexNode collectionComplex:
                ValidateAnyAllExpressionDepth(collectionComplex.Source, settings, currentDepth);
                return;
            case SingleNavigationNode navigation:
                ValidateAnyAllExpressionDepth(navigation.Source, settings, currentDepth);
                return;
            case CollectionNavigationNode collectionNavigation:
                ValidateAnyAllExpressionDepth(collectionNavigation.Source, settings, currentDepth);
                return;
            case SingleResourceCastNode resourceCast:
                ValidateAnyAllExpressionDepth(resourceCast.Source, settings, currentDepth);
                return;
            case CollectionResourceCastNode collectionResourceCast:
                ValidateAnyAllExpressionDepth(collectionResourceCast.Source, settings, currentDepth);
                return;
            case AggregatedCollectionPropertyNode aggregatedProperty:
                ValidateAnyAllExpressionDepth(aggregatedProperty.Source, settings, currentDepth);
                return;
            case SingleValueFunctionCallNode function:
                ValidateAnyAllExpressionDepth(function.Source, settings, currentDepth);
                parameters = function.Parameters;
                break;
            case CollectionFunctionCallNode collectionFunction:
                ValidateAnyAllExpressionDepth(collectionFunction.Source, settings, currentDepth);
                parameters = collectionFunction.Parameters;
                break;
            case SingleResourceFunctionCallNode resourceFunction:
                ValidateAnyAllExpressionDepth(resourceFunction.Source, settings, currentDepth);
                parameters = resourceFunction.Parameters;
                break;
            case CollectionResourceFunctionCallNode collectionResourceFunction:
                ValidateAnyAllExpressionDepth(collectionResourceFunction.Source, settings, currentDepth);
                parameters = collectionResourceFunction.Parameters;
                break;
            case NamedFunctionParameterNode parameter:
                ValidateAnyAllExpressionDepth(parameter.Value, settings, currentDepth);
                return;
            default:
                return;
        }

        // Function arguments can contain lambdas, so check them as well as the function's source.
        if (parameters is IList<QueryNode> arguments)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                ValidateAnyAllExpressionDepth(arguments[i], settings, currentDepth);
            }
        }
        else
        {
            foreach (QueryNode parameter in parameters)
            {
                ValidateAnyAllExpressionDepth(parameter, settings, currentDepth);
            }
        }
    }

    internal static void ValidateAnyAllExpressionDepth(OrderByClause clause, ODataValidationSettings settings)
    {
        while (clause != null)
        {
            ValidateAnyAllExpressionDepth(clause.Expression, settings);
            clause = clause.ThenBy;
        }
    }

    internal static void ValidateAnyAllExpressionDepth(ComputeClause clause, ODataValidationSettings settings)
    {
        if (clause == null)
        {
            return;
        }

        if (clause.ComputedItems is IList<ComputeExpression> expressions)
        {
            for (int i = 0; i < expressions.Count; i++)
            {
                ValidateAnyAllExpressionDepth(expressions[i].Expression, settings);
            }
        }
        else
        {
            foreach (ComputeExpression expression in clause.ComputedItems)
            {
                ValidateAnyAllExpressionDepth(expression.Expression, settings);
            }
        }
    }

    /// <summary>
    /// Validates a single value function call.
    /// </summary>
    /// <param name="functionCallNode">The function name.</param>
    /// <param name="settings">The validation settings.</param>
    public static void ValidateFunction(SingleValueFunctionCallNode functionCallNode, ODataValidationSettings settings)
    {
        if (functionCallNode == null)
        {
            throw Error.ArgumentNull(nameof(functionCallNode));
        }

        if (settings == null)
        {
            throw Error.ArgumentNull(nameof(settings));
        }

        string functionName = functionCallNode.Name;

        AllowedFunctions convertedFunction = ToODataFunction(functionName);
        if (convertedFunction != AllowedFunctions.None &&
            (settings.AllowedFunctions & convertedFunction) != convertedFunction)
        {
            // this means the given function is not allowed
            throw new ODataException(Error.Format(SRResources.NotAllowedFunction, functionName, "AllowedFunctions"));
        }
    }

    private static AllowedFunctions ToODataFunction(string functionName)
    {
        AllowedFunctions result = AllowedFunctions.None;
        switch (functionName)
        {
            case "any":
                result = AllowedFunctions.Any;
                break;
            case "all":
                result = AllowedFunctions.All;
                break;
            case "cast":
                result = AllowedFunctions.Cast;
                break;
            case ClrCanonicalFunctions.CeilingFunctionName:
                result = AllowedFunctions.Ceiling;
                break;
            case ClrCanonicalFunctions.ConcatFunctionName:
                result = AllowedFunctions.Concat;
                break;
            case ClrCanonicalFunctions.ContainsFunctionName:
                result = AllowedFunctions.Contains;
                break;
            case ClrCanonicalFunctions.DayFunctionName:
                result = AllowedFunctions.Day;
                break;
            case ClrCanonicalFunctions.EndswithFunctionName:
                result = AllowedFunctions.EndsWith;
                break;
            case ClrCanonicalFunctions.FloorFunctionName:
                result = AllowedFunctions.Floor;
                break;
            case ClrCanonicalFunctions.HourFunctionName:
                result = AllowedFunctions.Hour;
                break;
            case ClrCanonicalFunctions.IndexofFunctionName:
                result = AllowedFunctions.IndexOf;
                break;
            case ClrCanonicalFunctions.IsofFunctionName:
                result = AllowedFunctions.IsOf;
                break;
            case ClrCanonicalFunctions.LengthFunctionName:
                result = AllowedFunctions.Length;
                break;
            case ClrCanonicalFunctions.MatchesPatternFunctionName:
                result = AllowedFunctions.MatchesPattern;
                break;
            case ClrCanonicalFunctions.MinuteFunctionName:
                result = AllowedFunctions.Minute;
                break;
            case ClrCanonicalFunctions.MonthFunctionName:
                result = AllowedFunctions.Month;
                break;
            case ClrCanonicalFunctions.RoundFunctionName:
                result = AllowedFunctions.Round;
                break;
            case ClrCanonicalFunctions.SecondFunctionName:
                result = AllowedFunctions.Second;
                break;
            case ClrCanonicalFunctions.StartswithFunctionName:
                result = AllowedFunctions.StartsWith;
                break;
            case ClrCanonicalFunctions.SubstringFunctionName:
                result = AllowedFunctions.Substring;
                break;
            case ClrCanonicalFunctions.TolowerFunctionName:
                result = AllowedFunctions.ToLower;
                break;
            case ClrCanonicalFunctions.ToupperFunctionName:
                result = AllowedFunctions.ToUpper;
                break;
            case ClrCanonicalFunctions.TrimFunctionName:
                result = AllowedFunctions.Trim;
                break;
            case ClrCanonicalFunctions.YearFunctionName:
                result = AllowedFunctions.Year;
                break;
            case ClrCanonicalFunctions.DateFunctionName:
                result = AllowedFunctions.Date;
                break;
            case ClrCanonicalFunctions.TimeFunctionName:
                result = AllowedFunctions.Time;
                break;
            case ClrCanonicalFunctions.FractionalSecondsFunctionName:
                result = AllowedFunctions.FractionalSeconds;
                break;
            default:
                // Originally, we think it should never be here.
                // But, ODL supports the customized function, if we are here, it might mean it's a customized function.
                // Since we don't have configuration for customized function, let's return the 'None'.
                break;
        }

        return result;
    }
}
