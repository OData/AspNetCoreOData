using BenchmarkDotNet.Attributes;
using OData2Linq.Tests.SampleData;

namespace OData2Linq.Benchmark
{
    public class QueryOperations
    {
        private static readonly IQueryable<SimpleClass> query;

        static QueryOperations()
        {
            query = SimpleClass.CreateQuery();
        }

        [Benchmark]
        public SimpleClass[] ODataFilter()
        {
            return query.OData().Filter("Id eq 1").ToArray();
        }

        [Benchmark]
        public SimpleClass ODataOrderByIdDefault()
        {
            return query.OData().OrderBy("Id,Name").First();
        }
    }
}
