using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace ODataQueryBuilder.Query.Expressions
{
    /// <summary>
    /// Exposes the ability to translate an OData $filter represented by <see cref="FilterClause"/> to the <see cref="Expression"/>.
    /// </summary>
    public interface IFilterBinder
    {
        /// <summary>
        /// Translates an OData $filter represented by <see cref="FilterClause"/> to <see cref="Expression"/>.
        /// $filter=Name eq 'Sam'
        ///    |--  $it => $it.Name == "Sam"
        /// </summary>
        /// <param name="filterClause">The filter clause.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The filter binder result.</returns>
        /// <remarks>reconsider to return "LambdaExpression"? </remarks>
        Expression BindFilter(FilterClause filterClause, QueryBinderContext context);
    }
}
