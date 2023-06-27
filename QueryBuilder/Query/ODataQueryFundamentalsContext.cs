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
        public ODataQueryFundamentalsContext(IEdmModel model, Type elementClrType, ODataPath path, RequestContext requestContext)
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

            Initialize(model, ElementType, elementClrType, path, requestContext);
        }

        /// <summary>
        /// Constructs an instance of <see cref="ODataQueryFundamentalsContext"/> with <see cref="IEdmModel" />, element EDM type,
        /// and <see cref="ODataPath" />.
        /// </summary>
        /// <param name="model">The EDM model the given EDM type belongs to.</param>
        /// <param name="elementType">The EDM type of the element of the collection being queried.</param>
        /// <param name="path">The parsed <see cref="ODataPath"/>.</param>
        public ODataQueryFundamentalsContext(IEdmModel model, IEdmType elementType, ODataPath path, RequestContext requestContext)
        {
            if (elementType == null)
            {
                throw Error.ArgumentNull(nameof(elementType));
            }

            Initialize(model, elementType, null, path, requestContext);
        }

        private void Initialize(IEdmModel model, IEdmType elementType, Type elementClrType, ODataPath path, RequestContext requestContext)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            Model = model;
            ElementType = elementType;
            ElementClrType = elementClrType;
            Path = path;
            RequestContext = requestContext;
            NavigationSource = GetNavigationSource(Model, ElementType, path);
            GetPathContext();
        }

        internal ODataQueryFundamentalsContext(IEdmModel model, Type elementClrType, RequestContext requestContext)
            : this(model, elementClrType, path: null, requestContext)
        {
        }

        internal ODataQueryFundamentalsContext(IEdmModel model, IEdmType elementType, RequestContext requestContext)
            : this(model, elementType, path: null, requestContext)
        {
        }

        internal ODataQueryFundamentalsContext()
        { }

        /// <summary>
        /// Gets the given <see cref="DefaultQueryConfigurations"/>.
        /// </summary>
        public DefaultQueryConfigurations DefaultQueryConfigurations { get; internal set; }

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

        internal Request Request { get; set; }

        public RequestContext RequestContext { get; internal set; }

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

        #region Validator Methods

        public IComputeQueryValidator GetComputeQueryValidator()
        {
            return RequestContext.Validators.GetComputeQueryValidator();
        }

        public ICountQueryValidator GetCountQueryValidator()
        {
            return RequestContext.Validators.GetCountQueryValidator();
        }

        public IFilterQueryValidator GetFilterQueryValidator()
        {
             return RequestContext.Validators.GetFilterQueryValidator();
        }

        public IODataQueryValidator GetODataQueryValidator()
        {
            return RequestContext.Validators.GetODataQueryValidator();
        }

        public IOrderByQueryValidator GetOrderByQueryValidator()
        {
            return RequestContext.Validators.GetOrderByQueryValidator();
        }

        public ISelectExpandQueryValidator GetSelectExpandQueryValidator()
        {
            return RequestContext.Validators.GetSelectExpandQueryValidator();
        }

        public ISkipQueryValidator GetSkipQueryValidator()
        {
            return RequestContext.Validators.GetSkipQueryValidator();
        }

        public ISkipTokenQueryValidator GetSkipTokenQueryValidator()
        {
            return RequestContext.Validators.GetSkipTokenQueryValidator();
        }

        public ITopQueryValidator GetTopQueryValidator()
        {
            return RequestContext.Validators.GetTopQueryValidator();
        }

        #endregion

        #region Binder Methods

        public IOrderByBinder GetOrderByBinder()
        {
            return RequestContext.Binders.GetOrderByBinder();
        }

        public ISelectExpandBinder GetSelectExpandBinder()
        {
            return RequestContext.Binders.GetSelectExpandBinder();
        }

        public ISearchBinder GetSearchBinder()
        {
            return RequestContext.Binders.GetSearchBinder();
        }

        public IFilterBinder GetFilterBinder()
        {
            return RequestContext.Binders.GetFilterBinder();
        }

        #endregion
    }
}
