//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This defines a $compute OData query option for querying.
    /// The $compute system query option allows clients to define computed properties that can be used in a $select or within a $filter or $orderby expression.
    /// Computed properties SHOULD be included as dynamic properties in the result and MUST be included if $select is specified with the computed property name, or star (*).
    /// </summary>
    public class ComputeQueryOption
    {
        private ComputeClause _computeClause;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="ComputeQueryOption"/> based on the raw $compute value and
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public ComputeQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty(nameof(rawValue));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull(nameof(queryOptionParser));
            }

            Context = context;
            RawValue = rawValue;
            _queryOptionParser = queryOptionParser;
            ResultClrType = Context.ElementClrType;
        }

        // This constructor is intended for unit testing only.
        internal ComputeQueryOption(string rawValue, ODataQueryContext context)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty(nameof(rawValue));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Context = context;
            RawValue = rawValue;

            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$compute", rawValue } },
                context.RequestContainer);
        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; }

        /// <summary>
        /// ClrType for result of transformations
        /// </summary>
        public Type ResultClrType { get; }

        /// <summary>
        /// Gets the parsed <see cref="ComputeClause"/> for this query option.
        /// </summary>
        public ComputeClause ComputeClause
        {
            get
            {
                if (_computeClause == null)
                {
                    _computeClause = _queryOptionParser.ParseCompute();
                }

                return _computeClause;
            }
        }

        /// <summary>
        ///  Gets the raw $compute value.
        /// </summary>
        public string RawValue { get; }
    }
}
