//-----------------------------------------------------------------------------
// <copyright file="ODataQuerySettings.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This class describes the settings to use during query composition.
/// </summary>
public class ODataQuerySettings
{
    internal const int DefaultMaxFunctionCallDepth = 15;// the depth of function call expressions recursively in a query, such as 'length(tolower(name)) eq 5', the depth is 2.

    /// <summary>
    /// The default time span applied to a single <c>matchesPattern</c> filter function evaluation
    /// when <see cref="MatchesPatternTimeout"/> is not explicitly configured.
    /// </summary>
    internal static readonly TimeSpan DefaultMatchesPatternTimeout = TimeSpan.FromSeconds(1);

    private HandleNullPropagationOption _handleNullPropagationOption = HandleNullPropagationOption.Default;
    private int? _pageSize;
    private int? _modelBoundPageSize;
    private int _maxFunctionCallDepth = DefaultMaxFunctionCallDepth;
    private TimeSpan? _matchesPatternTimeout = DefaultMatchesPatternTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataQuerySettings" /> class.
    /// </summary>
    public ODataQuerySettings()
    {
        EnsureStableOrdering = true;
        EnableConstantParameterization = true;
    }

    /// <summary>
    /// Gets or sets the <see cref="TimeZoneInfo"/>.
    /// </summary>
    public TimeZoneInfo TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of query results to return based on the type or property.
    /// </summary>
    /// <value>
    /// The maximum number of query results to return based on the type or property,
    /// or <c>null</c> if there is no limit.
    /// </value>
    internal int? ModelBoundPageSize
    {
        get
        {
            return _modelBoundPageSize;
        }
        set
        {
            if (value.HasValue && value <= 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
            }

            _modelBoundPageSize = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether query composition should
    /// alter the original query when necessary to ensure a stable sort order.
    /// </summary>
    /// <value>A <c>true</c> value indicates the original query should
    /// be modified when necessary to guarantee a stable sort order.
    /// A <c>false</c> value indicates the sort order can be considered
    /// stable without modifying the query.  Query providers that ensure
    /// a stable sort order should set this value to <c>false</c>.
    /// The default value is <c>true</c>.</value>
    public bool EnsureStableOrdering { get; set; }

    /// <summary>
    /// Gets or sets a value indicating how null propagation should
    /// be handled during query composition.
    /// </summary>
    /// <value>
    /// The default is <see cref="HandleNullPropagationOption.Default"/>.
    /// </value>
    public HandleNullPropagationOption HandleNullPropagation
    {
        get
        {
            return _handleNullPropagationOption;
        }
        set
        {
            HandleNullPropagationOptionHelper.Validate(value, "value");
            _handleNullPropagationOption = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether constants should be parameterized. Parameterizing constants
    /// would result in better performance with Entity framework.
    /// </summary>
    /// <value>The default value is <c>true</c>.</value>
    public bool EnableConstantParameterization { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether queries with expanded navigations should be formulated
    /// to encourage correlated sub-query results to be buffered.
    /// Buffering correlated sub-query results can reduce the number of queries from N + 1 to 2
    /// by buffering results from the sub-query.
    /// </summary>
    /// <value>The default value is <c>false</c>.</value>
    public bool EnableCorrelatedSubqueryBuffering { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth for function calls in a query binding.
    /// </summary>
    public int MaxFunctionCallDepth
    {
        get => _maxFunctionCallDepth;
        set
        {
            if (value < 1)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
            }

            _maxFunctionCallDepth = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating which query options should be ignored when applying queries.
    /// </summary>
    public AllowedQueryOptions IgnoredQueryOptions { get; set; } = AllowedQueryOptions.None;

    /// <summary>
    /// Gets or sets a value indicating which nested query options should be ignored typically within select and expand.
    /// </summary>
    public AllowedQueryOptions IgnoredNestedQueryOptions { get; set; } = AllowedQueryOptions.None;

    /// <summary>
    /// Gets or sets the maximum number of query results to return.
    /// </summary>
    /// <value>
    /// The maximum number of query results to return, or <c>null</c> if there is no limit.
    /// </value>
    public int? PageSize
    {
        get
        {
            return _pageSize;
        }
        set
        {
            if (value.HasValue && value <= 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
            }

            _pageSize = value;
        }
    }

    /// <summary>
    /// Honor $filter inside $expand of non-collection navigation property.
    /// The expanded property is only populated when the filter evaluates to true.
    /// This setting is false by default.
    /// </summary>
    public bool HandleReferenceNavigationPropertyExpandFilter { get; set; }

    /// <summary>
    /// Gets or sets the time span that bounds a single <c>matchesPattern</c> filter evaluation. The bound is per
    /// evaluation, not per request, so a <c>$filter</c> over <c>N</c> elements may take up to <c>N</c> times this
    /// duration.
    /// </summary>
    /// <remarks>
    /// The evaluation is always stopped when the bound elapses, but the request completes as
    /// <c>400 (Bad Request)</c> only where the results are materialized during query execution (a configured page
    /// size or <c>$count</c>, or a <c>SingleResult</c> action). For a page-less <c>[EnableQuery]</c> action the
    /// predicate runs later, during response serialization, so the bound still applies but does not surface as
    /// <c>400</c>.
    /// </remarks>
    /// <value>
    /// The default is one second. A <c>null</c>, <see cref="TimeSpan.Zero"/>, or negative value applies no limit,
    /// consistent with <see cref="EnableQueryAttribute.MatchesPatternTimeoutMilliseconds"/>.
    /// </value>
    public TimeSpan? MatchesPatternTimeout
    {
        get => _matchesPatternTimeout;

        // A non-positive span opts out of the bound (no limit), consistent with
        // EnableQueryAttribute.MatchesPatternTimeoutMilliseconds treating 0 (or any non-positive value) as opting out.
        set => _matchesPatternTimeout = value.HasValue && value.Value <= TimeSpan.Zero ? null : value;
    }

    internal void CopyFrom(ODataQuerySettings settings)
    {
        TimeZone = settings.TimeZone;
        EnsureStableOrdering = settings.EnsureStableOrdering;
        EnableConstantParameterization = settings.EnableConstantParameterization;
        HandleNullPropagation = settings.HandleNullPropagation;
        PageSize = settings.PageSize;
        ModelBoundPageSize = settings.ModelBoundPageSize;
        HandleReferenceNavigationPropertyExpandFilter = settings.HandleReferenceNavigationPropertyExpandFilter;
        EnableCorrelatedSubqueryBuffering = settings.EnableCorrelatedSubqueryBuffering;
        IgnoredQueryOptions = settings.IgnoredQueryOptions;
        IgnoredNestedQueryOptions = settings.IgnoredNestedQueryOptions;
        MaxFunctionCallDepth = settings.MaxFunctionCallDepth;
        MatchesPatternTimeout = settings.MatchesPatternTimeout;
    }
}
