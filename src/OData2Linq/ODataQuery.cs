namespace OData2Linq
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class ODataQuery<T> : IQueryable<T>
    {
        internal ODataQuery(IQueryable inner, IServiceProvider serviceProvider)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            ServiceProvider = serviceProvider;
        }

        public IEdmModel EdmModel => ServiceProvider.GetRequiredService<IEdmModel>();

        public Type ElementType => Inner.ElementType;

        public Expression Expression => Inner.Expression;

        public IQueryProvider Provider => Inner.Provider;

        public IQueryable Inner { get; }
        public IServiceProvider ServiceProvider { get; }

        public IEnumerator GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (IEnumerator<T>)Inner.GetEnumerator();
        }

        public IQueryable<T> ToOriginalQuery()
        {
            return (IQueryable<T>)Inner;
        }
    }
}
