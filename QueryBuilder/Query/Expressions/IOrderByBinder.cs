using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace QueryBuilder.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $orderby represented by <see cref="OrderByClause"/> to the <see cref="Expression"/>
    /// wrappered in <see cref="OrderByBinderResult"/>.
    /// </summary>
    public interface IOrderByBinder
    {
        /// <summary>
        /// Translates an OData $orderby represented by <see cref="OrderByClause"/> to <see cref="Expression"/>.
        /// $orderby=Age,Name
        ///    |--  x => x.Age
        ///    |--  x => x.Name
        /// </summary>
        /// <param name="orderByClause">The orderby clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The OrderBy binder result, <see cref="OrderByBinderResult"/>.</returns>
        OrderByBinderResult BindOrderBy(OrderByClause orderByClause, QueryBinderContext context);
    }
}
