// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// Represents an <see cref="IQueryable"/> containing zero or one entities. Use together with an
    /// <c>[EnableQuery]</c>.
    /// </summary>
    public abstract class SingleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResult"/> class.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable"/> containing zero or one entities.</param>
        protected SingleResult(IQueryable queryable)
        {
            Queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
        }

        /// <summary>
        /// The <see cref="IQueryable"/> containing zero or one entities.
        /// </summary>
        public IQueryable Queryable { get;}

        /// <summary>
        /// Creates a <see cref="SingleResult{T}"/> from an <see cref="IQueryable{T}"/>. A helper method to instantiate
        /// a <see cref="SingleResult{T}"/> object without having to explicitly specify the type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data in the data source.</typeparam>
        /// <param name="queryable">The <see cref="IQueryable{T}"/> containing zero or one entities.</param>
        /// <returns>The created <see cref="SingleResult{T}"/>.</returns>
        public static SingleResult<T> Create<T>(IQueryable<T> queryable)
        {
            return new SingleResult<T>(queryable);
        }
    }
}
