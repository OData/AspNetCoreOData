using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace QueryBuilder.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $search represented by <see cref="SearchClause"/> to the <see cref="Expression"/>.
    /// The $search system query option restricts the result to include only those items matching the specified search expression.
    /// The definition of what it means to match is dependent upon the implementation.
    /// Therefore, there's no default implementation of $search binder.
    /// Developer should implement this interface and inject the search binder into service collection.
    /// </summary>
    public interface ISearchBinder
    {
        /// <summary>
        /// Translates an OData $search represented by <see cref="SearchClause"/> to <see cref="Expression"/>.
        /// ~/Products?$search=mountain AND bike
        /// </summary>
        /// <param name="searchClause">The search clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The search clause binder result. It should be a lambda expression.</returns>
        Expression BindSearch(SearchClause searchClause, QueryBinderContext context);
    }
}
