//-----------------------------------------------------------------------------
// <copyright file="SelectExpandValidatorContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// The metadata context for $select and $expand validator.
/// </summary>
public class SelectExpandValidatorContext : QueryValidatorContext
{
    /// <summary>
    /// The running count of projection ($select and $expand) items visited across the whole tree. The counter is a StrongBox so it can be shared by
    /// reference through Clone, letting every level of the projection tree contribute to a single total. A null counter means the check is disabled.
    /// </summary>
    private StrongBox<int> _selectExpandNodeCount;

    /// <summary>
    /// The effective maximum number of projection items allowed for the request. A non-positive maximum disables the check.
    /// </summary>
    private int _maxSelectExpandNodeCount;

    /// <summary>
    /// The weight of each projection item counted at this level; that is, how many materialized items a single count
    /// represents. Defaults to 1 (the multiplicative identity, so an item counts once). Descending through a $levels
    /// expansion scales it, because $levels replicates the $expand node and its subtree once per level. Copied by value
    /// in Clone, so each level keeps its own weight while the running count stays shared by reference.
    /// </summary>
    private long _projectionWeight = 1;

    /// <summary>
    /// The top level $select and $expand query option.
    /// </summary>
    public SelectExpandQueryOption SelectExpand { get; set; }

    /// <summary>
    /// The remaining depth on property.
    /// It's weird logic in current implementation. Need to improve it later.
    /// </summary>
    public int? RemainingDepth { get; set; } = null;

    /// <summary>
    /// Initializes the projection ($select and $expand) item counter from the effective maximum resolved for the
    /// request, and resets the per-level weight to its baseline of 1. A non-positive maximum disables the check. This
    /// is called once on the root context; contexts produced by <see cref="Clone"/> share the same counter so the
    /// whole projection tree is counted together.
    /// </summary>
    /// <param name="maxSelectExpandNodeCount">The effective maximum number of projection items allowed.</param>
    internal void InitializeSelectExpandNodeCount(int maxSelectExpandNodeCount)
    {
        _maxSelectExpandNodeCount = maxSelectExpandNodeCount;
        _projectionWeight = 1;
        _selectExpandNodeCount = maxSelectExpandNodeCount > 0 ? new StrongBox<int>(0) : null;
    }

    /// <summary>
    /// Adds the current projection weight to the running count of projection ($select and $expand) items and throws
    /// when the total would exceed the effective maximum. The weight is 1 for an ordinary item but greater under a
    /// $levels expansion, so a single call can account for several materialized items. The comparison is performed in
    /// a widening type so the addition cannot overflow, and the whole method is a no-op when the check is disabled (a
    /// non-positive maximum).
    /// </summary>
    /// <exception cref="ODataException">Thrown when the number of projection items exceeds the maximum.</exception>
    internal void IncrementSelectExpandNodeCount()
    {
        if (_selectExpandNodeCount == null)
        {
            return;
        }

        if ((long)_selectExpandNodeCount.Value + _projectionWeight > _maxSelectExpandNodeCount)
        {
            throw new ODataException(Error.Format(SRResources.MaxSelectExpandNodeCountExceeded, _maxSelectExpandNodeCount, "MaxSelectExpandNodeCount"));
        }

        _selectExpandNodeCount.Value += (int)_projectionWeight;
    }

    /// <summary>
    /// Returns a context whose projection weight accounts for <paramref name="levels"/> copies of the items about to
    /// be counted, so a $levels expansion is counted as though every level were written out explicitly.
    /// </summary>
    /// <param name="levels">
    /// The number of $levels copies of the items about to be counted (from GetProjectionLevels). A value of 1 or less
    /// means no scaling.
    /// </param>
    /// <returns>
    /// The same context when no scaling is needed - the check is disabled, or there is at most one copy - so the
    /// common path allocates nothing; otherwise a clone, so the sibling items counted through the original context
    /// keep the current weight. The scaled weight is clamped to just past the maximum, so the count stays
    /// overflow-safe while any excess still forces rejection on the next counted item.
    /// </returns>
    internal SelectExpandValidatorContext WithProjectionWeightForLevels(long levels)
    {
        if (_selectExpandNodeCount == null || levels <= 1)
        {
            return this;
        }

        long ceiling = (long)_maxSelectExpandNodeCount + 1;
        long boundedLevels = levels < ceiling ? levels : ceiling;
        long scaledWeight = _projectionWeight * boundedLevels;

        SelectExpandValidatorContext scoped = Clone();
        scoped._projectionWeight = scaledWeight < ceiling ? scaledWeight : ceiling;
        return scoped;
    }

    /// <summary>
    /// Clone the context.
    /// </summary>
    /// <returns>The cloned context.</returns>
    public SelectExpandValidatorContext Clone()
    {
        return new SelectExpandValidatorContext
        {
            SelectExpand = this.SelectExpand,
            Context = this.Context,
            ValidationSettings = this.ValidationSettings,
            Property = this.Property,
            StructuredType = this.StructuredType,
            RemainingDepth = this.RemainingDepth,
            CurrentDepth = this.CurrentDepth,
            _selectExpandNodeCount = this._selectExpandNodeCount,
            _maxSelectExpandNodeCount = this._maxSelectExpandNodeCount,
            _projectionWeight = this._projectionWeight
        };
    }
}
