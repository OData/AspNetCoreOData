//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing the raw value of an <see cref="IEdmPrimitiveType"/>.
    /// </summary>
    public class ODataSerializerContext
    {
        private IDictionary<object, object> _items;
        private ODataQueryContext _queryContext;
        private SelectExpandClause _selectExpandClause;
        private bool _isSelectExpandClauseSet;
        internal Type Type { get; set; }
        private bool? _isDeltaOfT;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        public ODataSerializerContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        /// <param name="resource">The resource whose property is being nested.</param>
        /// <param name="selectExpandClause">The <see cref="SelectExpandClause"/> for the property being nested.</param>
        /// <param name="edmProperty">The complex property being nested or the navigation property being expanded.
        /// If the resource property is the dynamic complex, the resource property is null.
        /// </param>
        /// <remarks>This constructor is used to construct the serializer context for writing nested and expanded properties.</remarks>
        public ODataSerializerContext(ResourceContext resource, SelectExpandClause selectExpandClause, IEdmProperty edmProperty)
            : this(resource, edmProperty, null, null)
        {
            SelectExpandClause = selectExpandClause;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class for nested resources.
        /// </summary>
        /// <param name="resource">The resource whose property is being nested.</param>
        /// <param name="edmProperty">The complex property being nested or the navigation property being expanded.
        /// If the resource property is the dynamic complex, the resource property is null.
        /// </param>
        /// <param name="queryContext">The <see cref="ODataQueryContext"/> for the property being nested.</param>
        /// <param name="currentSelectItem">The <see cref="SelectItem"/> for the property being nested.></param>
        internal ODataSerializerContext(ResourceContext resource, IEdmProperty edmProperty, ODataQueryContext queryContext, SelectItem currentSelectItem)
        {
            if (resource == null)
            {
                throw Error.ArgumentNull("resource");
            }

            // Clone the resource's context. Use a helper function so it can
            // handle platform-specific differences in ODataSerializerContext.
            ODataSerializerContext context = resource.SerializerContext;
            this.Request = context.Request;

            Model = context.Model;
            Path = context.Path;
            RootElementName = context.RootElementName;
            SkipExpensiveAvailabilityChecks = context.SkipExpensiveAvailabilityChecks;
            MetadataLevel = context.MetadataLevel;
            Items = context.Items;
            ExpandReference = context.ExpandReference;
            TimeZone = context.TimeZone;

            QueryContext = queryContext;

            ExpandedResource = resource; // parent resource

            CurrentSelectItem = currentSelectItem;

            var expandedNavigationSelectItem = currentSelectItem as ExpandedNavigationSelectItem;
            if (expandedNavigationSelectItem != null)
            {
                SelectExpandClause = expandedNavigationSelectItem.SelectAndExpand;
                NavigationSource = expandedNavigationSelectItem.NavigationSource;

                SetComputedProperties(expandedNavigationSelectItem.ComputeOption);
            }
            else
            {
                var pathSelectItem = currentSelectItem as PathSelectItem;
                if (pathSelectItem != null)
                {
                    SelectExpandClause = pathSelectItem.SelectAndExpand;
                    NavigationSource = resource.NavigationSource; // Use it's parent navigation source.

                    SetComputedProperties(pathSelectItem.ComputeOption);
                }

                var referencedNavigation = currentSelectItem as ExpandedReferenceSelectItem;
                if (referencedNavigation != null)
                {
                    ExpandReference = true;
                    NavigationSource = referencedNavigation.NavigationSource;

                    SetComputedProperties(referencedNavigation.ComputeOption);
                }
            }

            EdmProperty = edmProperty; // should be nested property

            if (currentSelectItem == null || (NavigationSource as IEdmUnknownEntitySet) != null)
            {
                IEdmNavigationProperty navigationProperty = edmProperty as IEdmNavigationProperty;
                if (navigationProperty != null && context.NavigationSource != null)
                {
                    NavigationSource = context.NavigationSource.FindNavigationTarget(NavigationProperty);
                }
                else
                {
                    NavigationSource = resource.NavigationSource;
                }
            }
        }

        /// <summary>
        /// Gets or sets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the metadata level of the response.
        /// </summary>
        public ODataMetadataLevel MetadataLevel { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        public HttpRequest Request { get;set; }

        /// <summary>
        /// Gets or sets the root element name which is used when writing primitive and enum types
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether it's $ref expanded.
        /// </summary>
        public bool ExpandReference { get; set; }

        /// <summary>
        /// Gets or sets the complex property being nested or navigation property being expanded.
        /// </summary>
        public IEdmProperty EdmProperty { get; set; }

        /// <summary>
        /// Get or sets whether expensive links should be calculated.
        /// </summary>
        public bool SkipExpensiveAvailabilityChecks { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TimeZoneInfo"/>.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataQueryOptions"/>.
        /// </summary>
        public ODataQueryOptions QueryOptions { get; internal set; }

        /// <summary>
        /// Gets the computed properties in serializer context.
        /// It contains all computed properties at current serializer context.
        /// </summary>
        public ISet<string> ComputedProperties { get; } = new HashSet<string>();

        private IUntypedResourceMapper _valueMapper;
        internal IUntypedResourceMapper UntypedMapper
        {
            get
            {
                if (_valueMapper == null)
                {
                    _valueMapper = Request?.GetRouteServices()?.GetService<IUntypedResourceMapper>();
                    _valueMapper = _valueMapper ?? DefaultUntypedResourceMapper.Instance;
                }

                return _valueMapper;
            }
        }

        /// <summary>
        /// ODataQueryContext object, retrieved from query options for top-level context and passed down to nested serializer context as is.
        /// </summary>
        internal ODataQueryContext QueryContext
        {
            get
            {
                if (QueryOptions != null)
                {
                    return QueryOptions.Context;
                }

                return _queryContext;
            }
            private set { _queryContext = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectItem"/>.
        /// </summary>
        internal SelectItem CurrentSelectItem { get; set; }

        /// <summary>
        /// Gets a property bag associated with this context to store any generic data.
        /// </summary>
        public IDictionary<object, object> Items
        {
            get
            {
                _items = _items ?? new Dictionary<object, object>();
                return _items;
            }
            private set
            {
                _items = value;
            }
        }

        /// <summary>
        /// Gets or sets the resource that is being expanded.
        /// </summary>
        public ResourceContext ExpandedResource { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SelectExpandClause"/>.
        /// </summary>
        public SelectExpandClause SelectExpandClause
        {
            get
            {
                // private backing field to be removed once public setter from ODataFeature is removed.
                if (_isSelectExpandClauseSet)
                {
                    return _selectExpandClause;
                }

                if (QueryOptions != null)
                {
                    if (QueryOptions.SelectExpand != null)
                    {
                        return QueryOptions.SelectExpand.ProcessedSelectExpandClause;
                    }

                    return null;
                }

                ExpandedNavigationSelectItem expandedItem = CurrentSelectItem as ExpandedNavigationSelectItem;
                if (expandedItem != null)
                {
                    return expandedItem.SelectAndExpand;
                }

                return null;
            }
            set
            {
                _isSelectExpandClauseSet = true;
                _selectExpandClause = value;
            }
        }

        internal bool IsDeltaOfT
        {
            get
            {
                if (_isDeltaOfT == null)
                {
                    _isDeltaOfT = Type != null && Type.IsGenericType && (Type.GetGenericTypeDefinition() == typeof(Delta<>) ||
                        Type.GetGenericTypeDefinition() == typeof(DeltaSet<>) || Type.GetGenericTypeDefinition() == typeof(DeltaDeletedResource<>));
                }

                return _isDeltaOfT.Value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ExpandedNavigationSelectItem"/>.
        /// </summary>
        internal ExpandedReferenceSelectItem CurrentExpandedSelectItem
        {
            get
            {
                return CurrentSelectItem as ExpandedReferenceSelectItem;
            }
        }

        /// <summary>
        /// Gets or sets the navigation property being expanded.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty
        {
            get
            {
                return EdmProperty as IEdmNavigationProperty;
            }
        }

        internal IEdmTypeReference GetEdmType(object instance, Type type, bool isUntyped = false)
        {
            IEdmTypeReference edmType = null;

            IEdmObject edmObject = instance as IEdmObject;
            if (edmObject != null)
            {
                edmType = edmObject.GetEdmType();
                if (edmType == null && !isUntyped)
                {
                    throw Error.InvalidOperation(SRResources.EdmTypeCannotBeNull, edmObject.GetType().FullName,
                        typeof(IEdmObject).Name);
                }
            }
            else
            {
                if (typeof(IDeltaSet).IsAssignableFrom(type))
                {
                    return Model.GetEdmTypeReference(type);
                }

                if (Model == null && !isUntyped)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
                }

                if (Model != null)
                {
                    edmType = Model.GetEdmTypeReference(type);

                    if (edmType == null)
                    {
                        if (instance is ITypedDelta delta)
                        {
                            edmType = Model.GetEdmTypeReference(delta.ExpectedClrType);
                        }
                        else
                        {
                            edmType = Model.GetEdmTypeReference(instance.GetType());
                        }

                        if (edmType == null && !isUntyped)
                        {
                            throw Error.InvalidOperation(SRResources.ClrTypeNotInModel, type);
                        }
                    }
                    else if (instance != null)
                    {
                        IEdmTypeReference actualType = Model.GetEdmTypeReference(instance.GetType());
                        if (actualType != null && actualType != edmType)
                        {
                            edmType = actualType;
                        }
                    }
                }
            }

            if (edmType == null && isUntyped)
            {
                // we can't find the Edm type and it's in untyped. Let's return it as Untyped resource type (or collection)
                return TypeHelper.GetUntypedEdmType(type ?? instance.GetType());
            }

            return edmType;
        }

        internal void SetComputedProperties(ComputeClause computeClause)
        {
            if (computeClause == null || !computeClause.ComputedItems.Any())
            {
                return;
            }

            foreach (var item in computeClause.ComputedItems)
            {
                ComputedProperties.Add(item.Alias);
            }
        }
    }
}
