using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using ODataQueryBuilder.Abstracts;
using ODataQueryBuilder.Query.Expressions;
using ODataQueryBuilder.Query.Validator;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ODataQueryBuilder.Query
{
    /// <summary>
    /// This defines some context information for the query request.
    /// </summary>
    public class RequestContext
    {
        public RequestContext(IAssemblyResolver assembliesResolver = null, ODataQuerySettings defaultQuerySettings = null, int pageSize = -1,
                              QueryValidators validators = null, QueryBinders binders = null, SkipTokenHandler skipTokenHandler = null,
                              DefaultQueryConfigurations defaultQueryConfigurations = null, bool? isNoDollarQueryEnable = null, Func<ODataUriResolver> uriResolverFactory = null)
        {
            if (assembliesResolver == null)
            {
                assembliesResolver = AssemblyResolverHelper.Default;
            }

            if (defaultQuerySettings == null) {
                 defaultQuerySettings = new ODataQuerySettings();
            }

            if (validators == null)
            {
                validators = new QueryValidators();
            }

            if (binders == null)
            {
                binders = new QueryBinders();
            }

            // TODO: Handle null SkipTokenHandler

            if (defaultQueryConfigurations == null)
            {
                defaultQueryConfigurations = new DefaultQueryConfigurations();
            }

            AssembliesResolver = assembliesResolver;
            DefaultQuerySettings = defaultQuerySettings;
            PageSize = pageSize;
            Validators = validators;
            Binders = binders;
            PagingSkipTokenHandler = skipTokenHandler;
            DefaultQueryConfigurations = defaultQueryConfigurations;
            IsNoDollarQueryEnable = isNoDollarQueryEnable;
            InitializeUriResolverFactory(uriResolverFactory);
        }

        public IAssemblyResolver AssembliesResolver { get; set; }

        public ODataQuerySettings DefaultQuerySettings { get; set; }

        public int PageSize { get; set; }

        public QueryValidators Validators { get; set; }

        public QueryBinders Binders { get; set; }

        public SkipTokenHandler PagingSkipTokenHandler { get; set; }

        public DefaultQueryConfigurations DefaultQueryConfigurations { get; set; }

        public bool? IsNoDollarQueryEnable { get; set; }

        public Func<ODataUriResolver> UriResolverFactory { get; set; }

        public static Func<ODataUriResolver> DefaultUriResolverFactory { get; } = () => new ODataUriResolver { EnableCaseInsensitive = true }; // one func to encapsulate all of the defaults forever; don't have to worry about too many objects

        // Avoid memory leak by specifically having true/false
        // Goal: Uri Resolver is source of truth for NoDollarSign (reduce complexity for customers + not for us).
        //       We can still have dependency injection without taking a dependency on the DI Container.
        private static readonly Func<ODataUriResolver> DefaultUriResolverFactoryTrue = () => {
            ODataUriResolver resolver = DefaultUriResolverFactory();
            resolver.EnableNoDollarQueryOptions = true;
            return resolver;
        };

        private static readonly Func<ODataUriResolver> DefaultUriResolverFactoryFalse = () => {
            ODataUriResolver resolver = DefaultUriResolverFactory();
            resolver.EnableNoDollarQueryOptions = false;
            return resolver;
        };

        private void InitializeUriResolverFactory(Func<ODataUriResolver> uriResolverFactory)
        {
            if (UriResolverFactory != null)
            {
                UriResolverFactory = uriResolverFactory;
            }
            else
            {
                if (IsNoDollarQueryEnable != null)
                {
                    if (IsNoDollarQueryEnable.Value)
                    {
                        UriResolverFactory = DefaultUriResolverFactoryTrue;
                    }
                    else
                    {
                        UriResolverFactory = DefaultUriResolverFactoryFalse;
                    }
                }
                else
                {
                    UriResolverFactory = DefaultUriResolverFactory;
                }
            }
        }
    }
}
