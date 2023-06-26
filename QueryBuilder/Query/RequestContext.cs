using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using QueryBuilder.Abstracts;
using QueryBuilder.Query.Expressions;
using QueryBuilder.Query.Validator;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QueryBuilder.Query
{
    /// <summary>
    /// This defines some context information for the query request.
    /// </summary>
    public class RequestContext
    {
        public RequestContext(IAssemblyResolver assembliesResolver = null, ODataQuerySettings defaultQuerySettings = null, int pageSize = -1,
                              QueryValidators queryValidators = null, QueryBinders queryBinders = null, SkipTokenHandler skipTokenHandler = null,
                              DefaultQueryConfigurations defaultQueryConfigurations = null, bool? isNoDollarQueryEnable = null, Func<ODataUriResolver> uriResolverFactory = null)
        {
            if (assembliesResolver == null)
            {
                assembliesResolver = AssemblyResolverHelper.Default;
            }

            if (defaultQuerySettings == null) {
                 defaultQuerySettings = new ODataQuerySettings();
            }

            if (queryValidators == null)
            {
                queryValidators = new QueryValidators();
            }

            if (queryBinders == null)
            {
                queryBinders = new QueryBinders();
            }

            //if (skipTokenHandler == null)
            //{
            //    skipTokenHandler = DefaultSkipTokenHandler.Instance;
            //}

            if (defaultQueryConfigurations == null)
            {
                defaultQueryConfigurations = new DefaultQueryConfigurations();
            }

            AssembliesResolver = assembliesResolver;
            DefaultQuerySettings = defaultQuerySettings;
            PageSize = pageSize;
            QueryValidators = queryValidators;
            QueryBinders = queryBinders;
            SkipTokenHandler = skipTokenHandler;
            DefaultQueryConfigurations = defaultQueryConfigurations;
            IsNoDollarQueryEnable = isNoDollarQueryEnable;
            UriResolverFactory = uriResolverFactory;
        }

        public IAssemblyResolver AssembliesResolver { get; set; }

        public ODataQuerySettings DefaultQuerySettings { get; set; }

        public int PageSize { get; set; }

        public QueryValidators QueryValidators { get; set; }

        public QueryBinders QueryBinders { get; set; }

        public SkipTokenHandler SkipTokenHandler { get; set; }

        public DefaultQueryConfigurations DefaultQueryConfigurations { get; set; }

        public bool? IsNoDollarQueryEnable { get; set; }

        public Func<ODataUriResolver> UriResolverFactory { get; set; }
    }
}
