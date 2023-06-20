using System.Diagnostics.Contracts;
using QueryBuilder.Edm;
using QueryBuilder.Query.Validator;
using QueryBuilder.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using QueryBuilder.Query.Expressions;

namespace QueryBuilder.Query
{
    /// <summary>
    /// This defines some context information used to perform query composition.
    /// </summary>
    public class ODataQueryFundamentalsContext
    {

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryFundamentalsContext"/> with <see cref="IEdmModel" />, element CLR type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EdmModel that includes the <see cref="IEdmType"/> corresponding to
        /// the given <paramref name="elementClrType"/>.</param>
        /// <param name="elementClrType">The CLR type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        /// <remarks>
        /// This is a public constructor used for stand-alone scenario; in this case, the services
        /// container may not be present.
        /// </remarks>
        // QUESTION: Pass in whole "ODataUriResolver uriResolver" or just "bool IsNoDollarQueryEnable"?
        public ODataQueryFundamentalsContext(IEdmModel model, Type elementClrType, ODataPath path,
                                  QueryValidators validators = null, QueryBinders binders = null, DefaultQueryConfigurations defaultQueryConfigurations = null, 
                                  bool? isNoDollarQueryEnable = null, Func<ODataUriResolver> uriResolverFactory = null, SkipTokenHandler skipTokenHandler = null)
        {
            if (elementClrType == null)
            {
                throw Error.ArgumentNull(nameof(elementClrType));
            }

            IEdmType elementType = model.GetEdmTypeReference(elementClrType)?.Definition;

            if (elementType == null)
            {
                throw Error.Argument(nameof(elementClrType), SRResources.ClrTypeNotInModel, elementClrType.FullName);
            }

            Initialize(model, ElementType, elementClrType, path, validators, binders, defaultQueryConfigurations, isNoDollarQueryEnable, uriResolverFactory, skipTokenHandler);
        }

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

        public Func<ODataUriResolver> UriResolverFactory { get; private set; }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryFundamentalsContext"/> with <see cref="IEdmModel" />, element EDM type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EDM model the given EDM type belongs to.</param>
        /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        public ODataQueryFundamentalsContext(IEdmModel model, IEdmType elementType, ODataPath path,
                                  QueryValidators validators = null, QueryBinders binders = null, DefaultQueryConfigurations defaultQueryConfigurations = null, 
                                  bool? isNoDollarQueryEnable = null, Func<ODataUriResolver> uriResolverFactory = null, SkipTokenHandler skipTokenHandler = null)
        {
            if (elementType == null)
            {
                throw Error.ArgumentNull(nameof(elementType));
            }

            Initialize(model, elementType, null, path, validators, binders, defaultQueryConfigurations, isNoDollarQueryEnable, uriResolverFactory, skipTokenHandler);
        }

        private void Initialize(IEdmModel model, IEdmType elementType, Type elementClrType, ODataPath path, 
                           QueryValidators validators, QueryBinders binders, DefaultQueryConfigurations defaultQueryConfigurations, bool? isNoDollarQueryEnable, Func<ODataUriResolver> uriResolverFactory, SkipTokenHandler skipTokenHandler)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (validators == null)
            {
                Validators = new QueryValidators();
            }

            if (binders == null)
            {
                Binders = new QueryBinders();
            }

            if (defaultQueryConfigurations == null)
            {
                // QUESTION: Should this be initialized at construction vs. at get?
                DefaultQueryConfigurations = new DefaultQueryConfigurations(); // instead of GetDefaultQuerySettings();
            }

            Model = model;
            ElementType = elementType;
            ElementClrType = elementClrType;
            Path = path;
            UriResolverFactory = uriResolverFactory ?? DefaultUriResolverFactory;
            PagingSkipTokenHandler = skipTokenHandler;
            NavigationSource = GetNavigationSource(Model, ElementType, path);
            GetPathContext();

            if (uriResolverFactory != null)
            {
                UriResolverFactory = uriResolverFactory;
            }
            else
            {
                if (isNoDollarQueryEnable != null)
                {
                    if (isNoDollarQueryEnable.Value)
                    {
                        UriResolverFactory = DefaultUriResolverFactoryTrue;
                    } else
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

        internal ODataQueryFundamentalsContext(IEdmModel model, Type elementClrType)
            : this(model, elementClrType, path: null)
        {
        }

        internal ODataQueryFundamentalsContext(IEdmModel model, IEdmType elementType)
            : this(model, elementType, path: null)
        {
        }

        internal ODataQueryFundamentalsContext()
        { }

        /// <summary>
        /// Gets the given <see cref="DefaultQueryConfigurations"/>.
        /// </summary>
        public DefaultQueryConfigurations DefaultQueryConfigurations { get; internal set; }

        public QueryValidators Validators { get; internal set; }

        public QueryBinders Binders { get; internal set; }

        /// <summary>
        /// Gets the given <see cref="IEdmModel"/> that contains the EntitySet.
        /// </summary>
        public IEdmModel Model { get; internal set; }

        /// <summary>
        /// Gets the <see cref="IEdmType"/> of the element.
        /// </summary>
        public IEdmType ElementType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmNavigationSource"/> that contains the element.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; private set; }

        /// <summary>
        /// Gets the CLR type of the element.
        /// </summary>
        public Type ElementClrType { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ODataPath"/>.
        /// </summary>
        public ODataPath Path { get; private set; }

        public SkipTokenHandler PagingSkipTokenHandler { get; private set; }

        internal Request Request { get; set; }

        internal IEdmProperty TargetProperty { get; set; }

        internal IEdmStructuredType TargetStructuredType { get; set; }

        internal string TargetName { get; set; }

        internal ODataValidationSettings ValidationSettings { get; set; }

        private static IEdmNavigationSource GetNavigationSource(IEdmModel model, IEdmType elementType, ODataPath odataPath)
        {
            Contract.Assert(model != null);
            Contract.Assert(elementType != null);

            IEdmNavigationSource navigationSource = (odataPath != null) ? odataPath.GetNavigationSource() : null;
            if (navigationSource != null)
            {
                return navigationSource;
            }

            IEdmEntityContainer entityContainer = model.EntityContainer;
            if (entityContainer == null)
            {
                return null;
            }

            List<IEdmEntitySet> matchedNavigationSources =
                entityContainer.EntitySets().Where(e => e.EntityType() == elementType).ToList();

            return (matchedNavigationSources.Count != 1) ? null : matchedNavigationSources[0];
        }

        private void GetPathContext()
        {
            if (Path != null)
            {
                (TargetProperty, TargetStructuredType, TargetName) = Path.GetPropertyAndStructuredTypeFromPath();
            }
            else
            {
                TargetStructuredType = ElementType as IEdmStructuredType;
            }
        }
    }
}
