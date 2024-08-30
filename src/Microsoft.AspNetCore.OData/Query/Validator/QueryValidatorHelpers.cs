//-----------------------------------------------------------------------------
// <copyright file="QueryValidatorHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Helper methods for validation.
/// </summary>
internal class QueryValidatorHelpers
{
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
