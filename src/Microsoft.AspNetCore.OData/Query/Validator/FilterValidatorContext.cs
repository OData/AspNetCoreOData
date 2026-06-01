//-----------------------------------------------------------------------------
// <copyright file="FilterValidatorContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.Contracts;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// The metadata context for $filter validator.
/// </summary>
public class FilterValidatorContext : QueryValidatorContext
{
    private int _currentNodeCount = 0;
    private int _currentAnyAllExpressionDepth = 0;
    private int _currentFunctionCallDepth = 0;

    /// <summary>
    /// The top level $filter query option.
    /// </summary>
    public FilterQueryOption Filter { get; set; }

    /// <summary>
    /// Gets current node count.
    /// </summary>
    public int CurrentNodeCount => _currentNodeCount;

    /// <summary>
    /// Gets current any and all expression depth.
    /// </summary>
    public int CurrentAnyAllExpressionDepth => _currentAnyAllExpressionDepth;

    /// <summary>
    /// This depth is for function call expressions recursively in a query, such as 'length(tolower(name)) eq 5', the depth is 2.
    /// </summary>
    public int CurrentFunctionCallDepth => _currentFunctionCallDepth;

    /// <summary>
    /// Clone the context.
    /// </summary>
    /// <returns>The cloned context.</returns>
    public FilterValidatorContext Clone()
    {
        return new FilterValidatorContext
        {
            Filter = this.Filter,
            Context = this.Context,
            ValidationSettings = this.ValidationSettings,
            Property = this.Property,
            StructuredType = this.StructuredType,
            CurrentDepth = this.CurrentDepth,
            _currentNodeCount = this._currentNodeCount,
            _currentAnyAllExpressionDepth = this._currentAnyAllExpressionDepth,
            _currentFunctionCallDepth = this._currentFunctionCallDepth
        };
    }

    /// <summary>
    /// Increment node count.
    /// </summary>
    /// <exception cref="ODataException">Throw OData exception.</exception>
    public void IncrementNodeCount()
    {
        if (_currentNodeCount >= ValidationSettings.MaxNodeCount)
        {
            throw new ODataException(Error.Format(SRResources.MaxNodeLimitExceeded, ValidationSettings.MaxNodeCount, "MaxNodeCount"));
        }

        _currentNodeCount++;
    }

    /// <summary>
    /// Enter lambda expression.
    /// </summary>
    /// <exception cref="ODataException">Throw OData exception.</exception>
    public void EnterLambda()
    {
        if (_currentAnyAllExpressionDepth >= ValidationSettings.MaxAnyAllExpressionDepth)
        {
            throw new ODataException(
                Error.Format(SRResources.MaxAnyAllExpressionLimitExceeded, ValidationSettings.MaxAnyAllExpressionDepth, "MaxAnyAllExpressionDepth"));
        }

        _currentAnyAllExpressionDepth++;
    }

    /// <summary>
    /// Exit lambda expression.
    /// </summary>
    public void ExitLambda()
    {
        Contract.Assert(_currentAnyAllExpressionDepth > 0);
        _currentAnyAllExpressionDepth--;
    }

    /// <summary>
    /// Enter function call.
    /// </summary>
    /// <exception cref="ODataException">Throw OData exception.</exception>
    public void EnterFunctionCall()
    {
        if (_currentFunctionCallDepth >= ValidationSettings.MaxFunctionCallDepth)
        {
            throw new ODataException(Error.Format(SRResources.MaxFunctionCallDepthExceeded, ValidationSettings.MaxFunctionCallDepth, "MaxFunctionCallDepth"));
        }

        _currentFunctionCallDepth++;
    }

    /// <summary>
    /// Exit function call.
    /// </summary>
    public void ExitFunctionCall()
    {
        Contract.Assert(_currentFunctionCallDepth > 0);
        _currentFunctionCallDepth--;
    }
}
