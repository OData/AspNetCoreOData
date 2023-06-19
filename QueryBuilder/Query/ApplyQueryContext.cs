using QueryBuilder.Abstracts;

namespace QueryBuilder.Query
{
    /// <summary>
    /// This defines some context information used to apply queries.
    /// </summary>
    public class ApplyQueryContext
    {
        public ApplyQueryContext(IQueryable query, IODataFeature odataFeature, ODataQuerySettings querySettings)
        {
            Query = query;
            ODataFeature = odataFeature;
            QuerySettings = querySettings; // > Context.GetODataQuerySettings();
        }

        public IQueryable Query { get; internal set; }

        public IODataFeature ODataFeature { get; internal set; }

        public ODataQuerySettings QuerySettings { get; internal set; }

        public int PreferredPageSize { get; internal set; }


        //

        public SkipTokenHandler SkipTokenHandler { get; set; }
    }
}
