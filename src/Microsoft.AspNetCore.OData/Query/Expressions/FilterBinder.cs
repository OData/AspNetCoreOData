//-----------------------------------------------------------------------------
// <copyright file="FilterBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Translates an OData $filter parse tree represented by <see cref="FilterClause"/> to
    /// an <see cref="Expression"/> and applies it to an <see cref="IQueryable"/>.
    /// </summary>
    public class FilterBinder : FilterOrderByBinderBase, IFilterBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterBinder"/> class.
        /// </summary>
        /// <param name="requestContainer">The request container.</param>
        [SuppressMessage("ApiDesign", "RS0022:Constructor make noninheritable base class inheritable", Justification = "We have multi-level inheritance")]
        public FilterBinder(IServiceProvider requestContainer)
            : base(requestContainer)
        {
        }

        internal FilterBinder(ODataQuerySettings settings, IAssemblyResolver assembliesResolver, IEdmModel model)
            : base(model, assembliesResolver, settings)
        {
        }

        /// <summary>
        /// For testing purposes only.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal FilterBinder(ODataQuerySettings querySettings, IAssemblyResolver assembliesResolver, IEdmModel model, Type filterType)
            : base(model, assembliesResolver, querySettings)
        {
            _filterType = filterType;
        }

        /// <inheritdoc/>
        public virtual LambdaExpression Bind(FilterBinderContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            Model = context.QueryContext.Model;
            QuerySettings = context.QuerySettings;
            InternalAssembliesResolver = context.QueryContext.GetAssemblyResolver();
            QueryContext = context.QueryContext;
            //BaseQuery = source;

            _filterType = context.ElementClrType;

            return BindFilterClause(context.FilterClause, context.ElementClrType);
        }
    }
}
