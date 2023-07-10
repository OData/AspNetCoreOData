using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataQueryBuilder.Query.Expressions
{

    public class QueryBinders
    {
        public QueryBinders(IOrderByBinder orderByBinder = null, ISelectExpandBinder selectExpandBinder = null,
            ISearchBinder searchBinder = null, IFilterBinder filterBinder = null)
        {
            OrderByBinder = orderByBinder;
            SelectExpandBinder = selectExpandBinder;
            SearchBinder = searchBinder;
            FilterBinder = filterBinder;
        }

        IOrderByBinder OrderByBinder { get; set; }

        ISelectExpandBinder SelectExpandBinder { get; set; }

        ISearchBinder SearchBinder { get; set; }

        IFilterBinder FilterBinder { get; set; }

        public IOrderByBinder GetOrderByBinder()
        {
            return OrderByBinder ??= new OrderByBinder();
        }

        public ISelectExpandBinder GetSelectExpandBinder()
        {
            return SelectExpandBinder ??= new SelectExpandBinder(GetFilterBinder(), GetOrderByBinder());
        }

        public ISearchBinder GetSearchBinder()
        {
            // We don't provide the default implementation of ISearchBinder,
            // Actually, how to match is dependent upon the implementation.
            return SearchBinder;
        }

        public IFilterBinder GetFilterBinder()
        {
            return FilterBinder ??= new FilterBinder();
        }
    }
}
