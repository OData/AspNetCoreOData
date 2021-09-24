//-----------------------------------------------------------------------------
// <copyright file="FilterBinderContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Wrapper for properties used by the <see cref="IFilterBinder"/>.
    /// </summary>
    public class FilterBinderContext
    {
        /// <summary>
        /// Gets or sets the parsed <see cref="Microsoft.OData.UriParser.FilterClause"/> for this query option.
        /// </summary>
        public FilterClause FilterClause { get; set; }

        /// <summary>
        /// Gets or sets the parsed <see cref="Microsoft.OData.UriParser.OrderByClause"/> for this query option.
        /// </summary>
        public OrderByClause OrderByClause { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataQuerySettings"/> that contains all the query application related settings.
        /// </summary>
        public ODataQuerySettings QuerySettings { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.
        /// </summary>
        public ODataQueryContext QueryContext { get; set; }

        /// <summary>
        /// Gets or sets the CLR type of the element.
        /// </summary>
        public Type ElementClrType { get; set; }
    }
}
