namespace OData2Linq
{
    using System;
    using System.Linq;

    public class ODataQueryOrdered<T> : ODataQuery<T>, IOrderedQueryable<T>
    {
        internal ODataQueryOrdered(IOrderedQueryable inner, IServiceProvider serviceProvider) : base(inner, serviceProvider)
        {
        }
    }
}
