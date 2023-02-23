//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This defines a $skiptoken OData query option for querying.
    /// </summary>
    public class SkipTokenQueryOption
    {
        /// <summary>
        /// Initialize a new instance of <see cref="SkipQueryOption"/> based on the raw $skiptoken value and
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $skiptoken query.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
        public SkipTokenQueryOption(string rawValue, ODataQueryContext context)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty(nameof(rawValue));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            RawValue = rawValue;
            Validator = context.GetSkipTokenQueryValidator();
            Handler = context.GetSkipTokenHandler();
            Context = context;
        }

        /// <summary>
        /// Gets the raw $skiptoken value.
        /// </summary>
        public string RawValue { get; }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; }

        /// <summary>
        /// Gets the SkipToken Query Validator.
        /// </summary>
        public ISkipTokenQueryValidator Validator { get; }

        /// <summary>
        /// Gets the skip token handler.
        /// </summary>
        public SkipTokenHandler Handler { get; }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="queryOptions">Information about the other query options.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public virtual IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            return Handler.ApplyTo(query, this, querySettings, queryOptions) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="queryOptions">Information about the other query options.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public virtual IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            return Handler.ApplyTo(query, this, querySettings, queryOptions);
        }

        /// <summary>
        /// Validate the skiptoken query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        public void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }
    }
}
