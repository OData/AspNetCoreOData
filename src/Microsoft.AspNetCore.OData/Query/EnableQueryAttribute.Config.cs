// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Query.Validator;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This partial class defines the configuration on <see cref="EnableQueryAttribute"/>.
    /// </summary>
    public partial class EnableQueryAttribute
    {
        private const char CommaSeparator = ',';

        // validation settings
        private ODataValidationSettings _validationSettings;
        private string _allowedOrderByProperties;

        // query settings
        private ODataQuerySettings _querySettings;

        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        public EnableQueryAttribute()
        {
            _validationSettings = new ODataValidationSettings();
            _querySettings = new ODataQuerySettings();
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
        public bool EnsureStableOrdering
        {
            get => _querySettings.EnsureStableOrdering;
            set => _querySettings.EnsureStableOrdering = value;
        }

        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition.
        /// </summary>
        /// <value>
        /// The default is <see cref="HandleNullPropagationOption.Default"/>.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation
        {
            get => _querySettings.HandleNullPropagation;
            set => _querySettings.HandleNullPropagation = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether constants should be parameterized. Parameterizing constants
        /// would result in better performance with Entity framework.
        /// </summary>
        /// <value>The default value is <c>true</c>.</value>
        public bool EnableConstantParameterization
        {
            get => _querySettings.EnableConstantParameterization;
            set => _querySettings.EnableConstantParameterization = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether queries with expanded navigations should be formulated
        /// to encourage correlated subquery results to be buffered.
        /// Buffering correlated subquery results can reduce the number of queries from N + 1 to 2
        /// by buffering results from the subquery.
        /// </summary>
        /// <value>The default value is <c>false</c>.</value>
        public bool EnableCorrelatedSubqueryBuffering
        {
            get => _querySettings.EnableCorrelatedSubqueryBuffering;
            set => _querySettings.EnableCorrelatedSubqueryBuffering = value;
        }

        /// <summary>
        /// Gets or sets the maximum depth of the Any or All elements nested inside the query. This limit helps prevent
        /// Denial of Service attacks.
        /// </summary>
        /// <value>
        /// The maximum depth of the Any or All elements nested inside the query. The default value is 1.
        /// </value>
        public int MaxAnyAllExpressionDepth
        {
            get => _validationSettings.MaxAnyAllExpressionDepth;
            set => _validationSettings.MaxAnyAllExpressionDepth = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of nodes inside the $filter syntax tree.
        /// </summary>
        /// <value>The default value is 100.</value>
        public int MaxNodeCount
        {
            get => _validationSettings.MaxNodeCount;
            set => _validationSettings.MaxNodeCount = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of query results to send back to clients.
        /// </summary>
        /// <value>
        /// The maximum number of query results to send back to clients.
        /// </value>
        public int PageSize
        {
            get => _querySettings.PageSize ?? default(int);
            set => _querySettings.PageSize = value;
        }

        /// <summary>
        /// Honor $filter inside $expand of non-collection navigation property.
        /// The expanded property is only populated when the filter evaluates to true.
        /// This setting is false by default.
        /// </summary>
        public bool HandleReferenceNavigationPropertyExpandFilter
        {
            get => _querySettings.HandleReferenceNavigationPropertyExpandFilter;
            set => _querySettings.HandleReferenceNavigationPropertyExpandFilter = value;
        }

        /// <summary>
        /// Gets or sets the query parameters that are allowed in queries.
        /// </summary>
        /// <value>The default includes all query options: $filter, $skip, $top, $orderby, $expand, $select, $count,
        /// $format, $skiptoken and $deltatoken.</value>
        public AllowedQueryOptions AllowedQueryOptions
        {
            get => _validationSettings.AllowedQueryOptions;
            set => _validationSettings.AllowedQueryOptions = value;
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed functions used in the $filter query. Supported
        /// functions include the following:
        /// <list type="definition">
        /// <item>
        /// <term>String related:</term>
        /// <description>contains, endswith, startswith, length, indexof, substring, tolower, toupper, trim,
        /// concat e.g. ~/Customers?$filter=length(CompanyName) eq 19</description>
        /// </item>
        /// <item>
        /// <term>DateTime related:</term>
        /// <description>year, month, day, hour, minute, second, fractionalseconds, date, time
        /// e.g. ~/Employees?$filter=year(BirthDate) eq 1971</description>
        /// </item>
        /// <item>
        /// <term>Math related:</term>
        /// <description>round, floor, ceiling</description>
        /// </item>
        /// <item>
        /// <term>Type related:</term>
        /// <description>isof, cast</description>
        /// </item>
        /// <item>
        /// <term>Collection related:</term>
        /// <description>any, all</description>
        /// </item>
        /// </list>
        /// </summary>
        public AllowedFunctions AllowedFunctions
        {
            get => _validationSettings.AllowedFunctions;
            set => _validationSettings.AllowedFunctions = value;
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed arithmetic operators including 'add', 'sub', 'mul',
        /// 'div', 'mod'.
        /// </summary>
        public AllowedArithmeticOperators AllowedArithmeticOperators
        {
            get => _validationSettings.AllowedArithmeticOperators;
            set => _validationSettings.AllowedArithmeticOperators = value;
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed logical Operators such as 'eq', 'ne', 'gt', 'ge',
        /// 'lt', 'le', 'and', 'or', 'not'.
        /// </summary>
        public AllowedLogicalOperators AllowedLogicalOperators
        {
            get => _validationSettings.AllowedLogicalOperators;
            set => _validationSettings.AllowedLogicalOperators = value;
        }

        /// <summary>
        /// <para>Gets or sets a string with comma separated list of property names. The queryable result can only be
        /// ordered by those properties defined in this list.</para>
        ///
        /// <para>Note, by default this string is null, which means it can be ordered by any property.</para>
        ///
        /// <para>For example, setting this value to null or empty string means that we allow ordering the queryable
        /// result by any properties. Setting this value to "Name" means we only allow queryable result to be ordered
        /// by Name property.</para>
        /// </summary>
        public string AllowedOrderByProperties
        {
            get => _allowedOrderByProperties;
            set
            {
                _allowedOrderByProperties = value;

                if (String.IsNullOrEmpty(value))
                {
                    _validationSettings.AllowedOrderByProperties.Clear();
                }
                else
                {
                    // now parse the value and set it to validationSettings
                    string[] properties = _allowedOrderByProperties.Split(CommaSeparator);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        _validationSettings.AllowedOrderByProperties.Add(properties[i].Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the max value of $skip that a client can request.
        /// </summary>
        public int MaxSkip
        {
            get => _validationSettings.MaxSkip ?? default(int);
            set => _validationSettings.MaxSkip = value;
        }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        public int MaxTop
        {
            get => _validationSettings.MaxTop ?? default(int);
            set => _validationSettings.MaxTop = value;
        }

        /// <summary>
        /// Gets or sets the max expansion depth for the $expand query option. To disable the maximum expansion depth
        /// check, set this property to 0.
        /// </summary>
        public int MaxExpansionDepth
        {
            get => _validationSettings.MaxExpansionDepth;
            set => _validationSettings.MaxExpansionDepth = value;
        }

        /// <summary>
        /// Gets or sets the maximum number of expressions that can be present in the $orderby.
        /// </summary>
        public int MaxOrderByNodeCount
        {
            get => _validationSettings.MaxOrderByNodeCount;
            set => _validationSettings.MaxOrderByNodeCount = value;
        }
    }
}
