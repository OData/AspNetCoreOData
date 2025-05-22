//-----------------------------------------------------------------------------
// <copyright file="SelectExpandBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions;

/// <summary>
/// Exposes the ability to translate an OData $select or $expand parse tree represented by <see cref="SelectExpandClause"/> to
/// an <see cref="Expression"/>.
/// </summary>
public class SelectExpandBinder : QueryBinder, ISelectExpandBinder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectExpandBinder" /> class.
    /// Select and Expand binder depends on <see cref="IFilterBinder"/> and <see cref="IOrderByBinder"/> to process inner $filter and $orderby.
    /// </summary>
    /// <param name="filterBinder">The injected filter binder.</param>
    /// <param name="orderByBinder">The injected orderby binder.</param>
    public SelectExpandBinder(IFilterBinder filterBinder, IOrderByBinder orderByBinder)
    {
        FilterBinder = filterBinder ?? throw Error.ArgumentNull(nameof(filterBinder));
        OrderByBinder = orderByBinder ?? throw Error.ArgumentNull(nameof(orderByBinder));
    }

    /// <summary>
    /// For unit test only.
    /// </summary>
    internal SelectExpandBinder()
        : this(new FilterBinder(), new OrderByBinder())
    { }

    /// <summary>
    /// Gets the filter binder.
    /// </summary>
    public IFilterBinder FilterBinder { get; }

    /// <summary>
    /// Gets the orderby binder.
    /// </summary>
    public IOrderByBinder OrderByBinder { get; }

    /// <summary>
    /// Translate an OData $select or $expand tree represented by <see cref="SelectExpandClause"/> to an <see cref="Expression"/>.
    /// </summary>
    /// <param name="selectExpandClause">The original <see cref="SelectExpandClause"/>.</param>
    /// <param name="context">An instance of the <see cref="QueryBinderContext"/>.</param>
    /// <returns>The $select and $expand binder result.</returns>
    public virtual Expression BindSelectExpand(SelectExpandClause selectExpandClause, QueryBinderContext context)
    {
        if (selectExpandClause == null)
        {
            throw Error.ArgumentNull(nameof(selectExpandClause));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        IEdmStructuredType structuredType = context.ElementType as IEdmStructuredType;
        IEdmNavigationSource navigationSource = context.NavigationSource;
        ParameterExpression source = context.CurrentParameter;

        // expression looks like -> new Wrapper { Instance = source , Properties = "...", Container = new PropertyContainer { ... } }
        Expression projectionExpression = ProjectElement(context, source, selectExpandClause, structuredType, navigationSource);

        // expression looks like -> source => new Wrapper { Instance = source .... }
        LambdaExpression projectionLambdaExpression = Expression.Lambda(projectionExpression, source);

        return projectionLambdaExpression;
    }

    internal Expression ProjectAsWrapper(QueryBinderContext context, Expression source, SelectExpandClause selectExpandClause,
        IEdmStructuredType structuredType, IEdmNavigationSource navigationSource, OrderByClause orderByClause = null,
        ComputeClause computeClause = null,
        long? topOption = null,
        long? skipOption = null,
        int? modelBoundPageSize = null)
    {
        Type elementType;
        bool isCollection = TypeHelper.IsCollection(source.Type, out elementType);
        QueryBinderContext subContext = new QueryBinderContext(context, context.QuerySettings, elementType);
        if (computeClause != null && IsAvailableODataQueryOption(context.QuerySettings, AllowedQueryOptions.Compute))
        {
            subContext.AddComputedProperties(computeClause.ComputedItems);
        }

        if (orderByClause != null && context.EnableSkipToken)
        {
            subContext.OrderByClauses = orderByClause.ToList();
        }

        if (isCollection)
        {
            // new CollectionWrapper<ElementType> { Instance = source.Select(s => new Wrapper { ... }) };
            return ProjectCollection(subContext, source, elementType, selectExpandClause, structuredType, navigationSource, orderByClause,
                topOption,
                skipOption,
                modelBoundPageSize);
        }
        else
        {
            // new Wrapper { v1 = source.property ... }
            return ProjectElement(subContext, source, selectExpandClause, /*computeClause,*/ structuredType, navigationSource);
        }
    }

    /// <summary>
    /// Creates an <see cref="Expression"/> from an <see cref="IEdmProperty"/> name.
    /// </summary>
    /// <param name="context">The <see cref="QueryBinderContext"/>.</param>
    /// <param name="elementType">The <see cref="IEdmStructuredType"/> that contains the edmProperty.</param>
    /// <param name="edmProperty">The <see cref="IEdmProperty"/> from which we are creating an <see cref="Expression"/>.</param>
    /// <param name="source">The source <see cref="Expression"/>.</param>
    /// <returns>The created <see cref="Expression"/>.</returns>
    public virtual Expression CreatePropertyNameExpression(QueryBinderContext context, IEdmStructuredType elementType, IEdmProperty edmProperty, Expression source)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        if (elementType == null)
        {
            throw Error.ArgumentNull(nameof(elementType));
        }

        if (edmProperty == null)
        {
            throw Error.ArgumentNull(nameof(edmProperty));
        }

        if (source == null)
        {
            throw Error.ArgumentNull(nameof(source));
        }

        IEdmStructuredType declaringType = edmProperty.DeclaringType;
        IEdmModel model = context.Model;

        // derived property using cast
        if (elementType != declaringType)
        {
            Type originalType = model.GetClrType(elementType);
            Type castType = model.GetClrType(declaringType);
            if (castType == null)
            {
                throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, declaringType.FullTypeName()));
            }

            if (!castType.IsAssignableFrom(originalType))
            {
                // Expression
                //          source is navigationPropertyDeclaringType ? propertyName : null
                return Expression.Condition(
                    test: Expression.TypeIs(source, castType),
                    ifTrue: Expression.Constant(edmProperty.Name),
                    ifFalse: Expression.Constant(null, typeof(string)));
            }
        }

        // Expression
        //          "propertyName"
        return Expression.Constant(edmProperty.Name);
    }

    /// <summary>
    /// Creates an <see cref="Expression"/> from an <see cref="IEdmProperty"/> name.
    /// </summary>
    /// <param name="context">The <see cref="QueryBinderContext"/>.</param>
    /// <param name="elementType">The <see cref="IEdmStructuredType"/> that contains the edmProperty.</param>
    /// <param name="edmProperty">The <see cref="IEdmProperty"/> from which we are creating an <see cref="Expression"/>.</param>
    /// <param name="source">The source <see cref="Expression"/>.</param>
    /// <param name="filterClause">The nested $filter query represented by <see cref="FilterClause"/>.</param>
    /// <param name="computeClause">The nested $compute query represented by <see cref="ComputeClause"/>.</param>
    /// <param name="search">The nested $search query represented by <see cref="SearchClause"/>.</param>
    /// <returns>The created <see cref="Expression"/>.</returns>
    public virtual Expression CreatePropertyValueExpression(QueryBinderContext context, IEdmStructuredType elementType,
        IEdmProperty edmProperty, Expression source, FilterClause filterClause, ComputeClause computeClause = null, SearchClause search = null)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        if (elementType == null)
        {
            throw Error.ArgumentNull(nameof(elementType));
        }

        if (edmProperty == null)
        {
            throw Error.ArgumentNull(nameof(edmProperty));
        }

        if (source == null)
        {
            throw Error.ArgumentNull(nameof(source));
        }

        IEdmModel model = context.Model;
        ODataQuerySettings settings = context.QuerySettings;

        // Expression: source = source as propertyDeclaringType
        if (elementType != edmProperty.DeclaringType)
        {
            Type castType = model.GetClrType(edmProperty.DeclaringType);
            if (castType == null)
            {
                throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, edmProperty.DeclaringType.FullTypeName()));
            }

            source = Expression.TypeAs(source, castType);
        }

        // Expression:  source.Property
        string propertyName = model.GetClrPropertyName(edmProperty);
            
        PropertyInfo propertyInfo = source.Type.GetProperty(propertyName, BindingFlags.DeclaredOnly);
        if (propertyInfo == null)
        {
            /*
             History of code:
                propertyInfo = source.Type.GetProperty(propertyName);
             This code fixes an issue where 'History of code' was unable to obtain attributes with the same name as the parent class after the subclass hid the parent class member.
             Such as: 
                'History of code' cannot get the Key property of the Child.
                    public class Father
                    {
                        public string Key { get; set; }
                    }
                    public class Child : Father
                    {
                        public new Guid Key { get; set; }
                    }
             */
            propertyInfo = source.Type.GetProperties().Where(m => m.Name.Equals(propertyName, StringComparison.Ordinal)).FirstOrDefault();
        }
            
        Expression propertyValue = Expression.Property(source, propertyInfo);
        Type nullablePropertyType = TypeHelper.ToNullable(propertyValue.Type);
        Expression nullablePropertyValue = ExpressionHelpers.ToNullable(propertyValue);

        if (filterClause != null && IsAvailableODataQueryOption(context.QuerySettings, AllowedQueryOptions.Filter))
        {
            bool isCollection = edmProperty.Type.IsCollection();

            IEdmTypeReference edmElementType = (isCollection ? edmProperty.Type.AsCollection().ElementType() : edmProperty.Type);
            Type clrElementType = model.GetClrType(edmElementType);
            if (clrElementType == null)
            {
                throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, edmElementType.FullName()));
            }

            Expression filterResult = nullablePropertyValue;
            QueryBinderContext subContext = CreateSubContext(context, computeClause, clrElementType);

            if (isCollection)
            {
                Expression filterSource = nullablePropertyValue;

                // TODO: Implement proper support for $select/$expand after $apply
                // Expression filterPredicate = FilterBinder.Bind(null, filterClause, clrElementType, queryContext, querySettings);
                filterResult = FilterBinder.ApplyBind(filterSource, filterClause, subContext);

                nullablePropertyType = filterResult.Type;
            }
            else if (settings.HandleReferenceNavigationPropertyExpandFilter)
            {
                LambdaExpression filterLambdaExpression = FilterBinder.BindFilter(filterClause, subContext) as LambdaExpression;
                if (filterLambdaExpression == null)
                {
                    throw new ODataException(Error.Format(SRResources.ExpandFilterExpressionNotLambdaExpression, edmProperty.Name, nameof(LambdaExpression)));
                }

                ParameterExpression filterParameter = filterLambdaExpression.Parameters.First();
                Expression predicateExpression = new ReferenceNavigationPropertyExpandFilterVisitor(filterParameter, nullablePropertyValue).Visit(filterLambdaExpression.Body);

                // create expression similar to: 'predicateExpression == true ? nullablePropertyValue : null'
                filterResult = Expression.Condition(
                    test: predicateExpression,
                    ifTrue: nullablePropertyValue,
                    ifFalse: Expression.Constant(value: null, type: nullablePropertyType));
            }

            if (settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // create expression similar to: 'nullablePropertyValue == null ? null : filterResult'
                nullablePropertyValue = Expression.Condition(
                    test: Expression.Equal(nullablePropertyValue, Expression.Constant(value: null)),
                    ifTrue: Expression.Constant(value: null, type: nullablePropertyType),
                    ifFalse: filterResult);
            }
            else
            {
                nullablePropertyValue = filterResult;
            }
        }

        // If both $search and $filter are specified in the same request, only those items satisfying both criteria are returned
        // apply $search
        if (search != null && context.SearchBinder != null && IsAvailableODataQueryOption(context.QuerySettings, AllowedQueryOptions.Search))
        {
            bool isCollection = edmProperty.Type.IsCollection();
            if (isCollection)
            {
                // only apply $search on collection
                IEdmTypeReference edmElementType = edmProperty.Type.AsCollection().ElementType();
                Type clrElementType = model.GetClrType(edmElementType);
                if (clrElementType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, edmElementType.FullName()));
                }

                QueryBinderContext subContext = CreateSubContext(context, computeClause, clrElementType);
                Expression searchResult = context.SearchBinder.ApplyBind(nullablePropertyValue, search, subContext);
                nullablePropertyType = searchResult.Type;
                if (settings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // create expression similar to: 'nullablePropertyValue == null ? null : filterResult'
                    nullablePropertyValue = Expression.Condition(
                        test: Expression.Equal(nullablePropertyValue, Expression.Constant(value: null)),
                        ifTrue: Expression.Constant(value: null, type: nullablePropertyType),
                        ifFalse: searchResult);
                }
                else
                {
                    nullablePropertyValue = searchResult;
                }
            }
        }

        if (settings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            // create expression similar to: 'source == null ? null : propertyValue'
            propertyValue = Expression.Condition(
                test: Expression.Equal(source, Expression.Constant(value: null)),
                ifTrue: Expression.Constant(value: null, type: nullablePropertyType),
                ifFalse: nullablePropertyValue);
        }
        else
        {
            // need to cast this to nullable as EF would fail while materializing if the property is not nullable and source is null.
            propertyValue = nullablePropertyValue;
        }

        return propertyValue;
    }

    // Generates the expression
    //      source => new Wrapper { Instance = source, Container = new PropertyContainer { ..expanded properties.. } }
    internal Expression ProjectElement(QueryBinderContext context, Expression source, SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource)
    {
        Contract.Assert(source != null);

        IEdmModel model = context.Model;

        // If it's not a structural type, just return the source.
        if (structuredType == null)
        {
            return source;
        }

        Type elementType = source.Type;
        Type wrapperType = typeof(SelectExpandWrapper<>).MakeGenericType(elementType);
        List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>();

        PropertyInfo wrapperProperty;
        Expression wrapperPropertyValueExpression;
        bool isInstancePropertySet = false;
        bool isTypeNamePropertySet = false;
        bool isContainerPropertySet = false;

        // Initialize property 'Model' on the wrapper class.
        // source = new Wrapper { Model = parameterized(a-edm-model) }
        // Always parameterize as EntityFramework does not let you inject non primitive constant values (like IEdmModel).
        wrapperProperty = wrapperType.GetProperty("Model");
        wrapperPropertyValueExpression = LinqParameterContainer.Parameterize(typeof(IEdmModel), model);
        wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, wrapperPropertyValueExpression));

        bool isSelectedAll = IsSelectAll(selectExpandClause);
        if (isSelectedAll)
        {
            // Initialize property 'Instance' on the wrapper class
            wrapperProperty = wrapperType.GetProperty("Instance");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, source));

            wrapperProperty = wrapperType.GetProperty("UseInstanceForProperties");
            wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, Expression.Constant(true)));
            isInstancePropertySet = true;
        }
        else
        {
            // Initialize property 'TypeName' on the wrapper class as we don't have the instance.
            Expression typeName = CreateTypeNameExpression(source, structuredType, model);
            if (typeName != null)
            {
                isTypeNamePropertySet = true;
                wrapperProperty = wrapperType.GetProperty("InstanceType");
                wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, typeName));
            }
        }

        // Initialize the property 'Container' on the wrapper class
        // source => new Wrapper { Container =  new PropertyContainer { .... } }
        if (selectExpandClause != null)
        {
            IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude;
            IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand;
            ISet<IEdmStructuralProperty> autoSelectedProperties;
            IList<string> computedProperties;

            IList<DynamicPathSegment> dynamicPathSegments = GetSelectExpandProperties(model, structuredType, navigationSource, selectExpandClause,
                out propertiesToInclude,
                out propertiesToExpand,
                out autoSelectedProperties);

            bool isContainDynamicPropertySelection = ParseComputedDynamicProperties(context, dynamicPathSegments, isSelectedAll, out computedProperties);

            bool isSelectingOpenTypeSegments = isContainDynamicPropertySelection || IsSelectAllOnOpenType(selectExpandClause, structuredType);

            if (propertiesToExpand != null || propertiesToInclude != null || computedProperties != null || autoSelectedProperties != null || isSelectingOpenTypeSegments || context.OrderByClauses != null)
            {
                Expression propertyContainerCreation =
                    BuildPropertyContainer(context, source, structuredType, propertiesToExpand, propertiesToInclude, computedProperties, autoSelectedProperties, isSelectingOpenTypeSegments, isSelectedAll);

                if (propertyContainerCreation != null)
                {
                    wrapperProperty = wrapperType.GetProperty("Container");
                    Contract.Assert(wrapperProperty != null);

                    wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperProperty, propertyContainerCreation));
                    isContainerPropertySet = true;
                }
            }
        }

        Type wrapperGenericType = GetWrapperGenericType(isInstancePropertySet, isTypeNamePropertySet, isContainerPropertySet);
        wrapperType = wrapperGenericType.MakeGenericType(elementType);
        return Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments);
    }

    private static bool ParseComputedDynamicProperties(QueryBinderContext context, IList<DynamicPathSegment> dynamicPathSegments, bool isSelectedAll,
        out IList<string> computedProperties)
    {
        computedProperties = null;

        // If $select=*, then we should include all computed properties.
        if (isSelectedAll)
        {
            computedProperties = context.ComputedProperties.Select(c => c.Key).ToList();
            return true; // select all means to include all dynamic properties.
        }

        if (context.ComputedProperties == null || context.ComputedProperties.Count == 0)
        {
            return dynamicPathSegments.Count > 0;
        }

        bool hasDynamic = false;
        foreach (var segment in dynamicPathSegments)
        {
            if (context.ComputedProperties.ContainsKey(segment.Identifier))
            {
                if (computedProperties == null)
                {
                    computedProperties = new List<string>();
                }

                computedProperties.Add(segment.Identifier);
            }
            else
            {
                hasDynamic = true;
            }
        }

        return hasDynamic;
    }

    /// <summary>
    /// Gets the $select and $expand properties from the given <see cref="SelectExpandClause"/>
    /// </summary>
    /// <param name="model">The Edm model.</param>
    /// <param name="structuredType">The current structural type.</param>
    /// <param name="navigationSource">The current navigation source.</param>
    /// <param name="selectExpandClause">The given select and expand clause.</param>
    /// <param name="propertiesToInclude">The out properties to include at current level, could be null.</param>
    /// <param name="propertiesToExpand">The out properties to expand at current level, could be null.</param>
    /// <param name="autoSelectedProperties">The out auto selected properties to include at current level, could be null.</param>
    /// <returns>true if the select contains dynamic property selection, false if it's not.</returns>
    internal static IList<DynamicPathSegment> GetSelectExpandProperties(IEdmModel model, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource,
        SelectExpandClause selectExpandClause,
        out IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude,
        out IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand,
        out ISet<IEdmStructuralProperty> autoSelectedProperties)
    {
        Contract.Assert(selectExpandClause != null);

        // Properties to be included includes all the properties selected or in the middle of a $select and $expand path.
        // for example: "$expand=abc/xyz/nav", "abc" and "xyz" are the middle properties that should be included.
        // meanwhile, "nav" is the property that should be expanded.
        // If it's a type cast path, for example: $select=NS.TypeCast/abc, "abc" should be included also.
        propertiesToInclude = null;
        propertiesToExpand = null;
        autoSelectedProperties = null;

        IList<DynamicPathSegment> dynamicsSegments = new List<DynamicPathSegment>();

        var currentLevelPropertiesInclude = new Dictionary<IEdmStructuralProperty, SelectExpandIncludedProperty>();
        foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
        {
            // $expand=...
            ExpandedReferenceSelectItem expandedItem = selectItem as ExpandedReferenceSelectItem;
            if (expandedItem != null)
            {
                ProcessExpandedItem(expandedItem, navigationSource, currentLevelPropertiesInclude, ref propertiesToExpand);
                continue;
            }

            // $select=...
            PathSelectItem pathItem = selectItem as PathSelectItem;
            if (pathItem != null)
            {
                DynamicPathSegment dynamicSegment = ProcessSelectedItem(pathItem, navigationSource, currentLevelPropertiesInclude);
                if (dynamicSegment != null)
                {
                    dynamicsSegments.Add(dynamicSegment);
                }
                continue;
            }

            // Skip processing the "WildcardSelectItem and NamespaceQualifiedWildcardSelectItem"
            // ODL now doesn't support "$select=property/*" and "$select=property/NS.*"
        }

        if (!IsSelectAll(selectExpandClause))
        {
            // We should include the keys if it's an entity.
            IEdmEntityType entityType = structuredType as IEdmEntityType;
            if (entityType != null)
            {
                foreach (IEdmStructuralProperty keyProperty in entityType.Key())
                {
                    if (!currentLevelPropertiesInclude.Keys.Contains(keyProperty))
                    {
                        if (autoSelectedProperties == null)
                        {
                            autoSelectedProperties = new HashSet<IEdmStructuralProperty>();
                        }

                        autoSelectedProperties.Add(keyProperty);
                    }
                }
            }

            // We should add concurrency properties, if not added
            if (navigationSource != null && model != null)
            {
                IEnumerable<IEdmStructuralProperty> concurrencyProperties = model.GetConcurrencyProperties(navigationSource);
                foreach (IEdmStructuralProperty concurrencyProperty in concurrencyProperties)
                {
                    if (structuredType.Properties().Any(p => p == concurrencyProperty))
                    {
                        if (!currentLevelPropertiesInclude.Keys.Contains(concurrencyProperty))
                        {
                            if (autoSelectedProperties == null)
                            {
                                autoSelectedProperties = new HashSet<IEdmStructuralProperty>();
                            }

                            autoSelectedProperties.Add(concurrencyProperty);
                        }
                    }
                }
            }
        }

        if (currentLevelPropertiesInclude.Any())
        {
            propertiesToInclude = new Dictionary<IEdmStructuralProperty, PathSelectItem>();
            foreach (var propertiesInclude in currentLevelPropertiesInclude)
            {
                propertiesToInclude[propertiesInclude.Key] = propertiesInclude.Value == null ? null : propertiesInclude.Value.ToPathSelectItem();
            }
        }

        return dynamicsSegments;
    }

    /// <summary>
    /// Process the <see cref="ExpandedReferenceSelectItem"/>.
    /// </summary>
    /// <param name="expandedItem">The expanded item.</param>
    /// <param name="navigationSource">The navigation source.</param>
    /// <param name="currentLevelPropertiesInclude">The current level properties included.</param>
    /// <param name="propertiesToExpand">out/ref, the property expanded.</param>
    private static void ProcessExpandedItem(ExpandedReferenceSelectItem expandedItem,
        IEdmNavigationSource navigationSource,
        IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude,
        ref IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand)
    {
        Contract.Assert(expandedItem != null && expandedItem.PathToNavigationProperty != null);
        Contract.Assert(currentLevelPropertiesInclude != null);

        // Verify and process the $expand=... path.
        IList<ODataPathSegment> remainingSegments;
        ODataPathSegment firstNonTypeSegment = expandedItem.PathToNavigationProperty.GetFirstNonTypeCastSegment(out remainingSegments);

        // for $expand=NS.SubType/Nav, we don't care about the leading type segment, because with or without the type segment
        // the "nav" property value expression should be built into the property container.

        PropertySegment firstStructuralPropertySegment = firstNonTypeSegment as PropertySegment;
        if (firstStructuralPropertySegment != null)
        {
            // for example: $expand=abc/nav, the remaining segments should never be null because at least the last navigation segment is there.
            Contract.Assert(remainingSegments != null);

            SelectExpandIncludedProperty newPropertySelectItem;
            if (!currentLevelPropertiesInclude.TryGetValue(firstStructuralPropertySegment.Property, out newPropertySelectItem))
            {
                newPropertySelectItem = new SelectExpandIncludedProperty(firstStructuralPropertySegment, navigationSource);
                currentLevelPropertiesInclude[firstStructuralPropertySegment.Property] = newPropertySelectItem;
            }

            newPropertySelectItem.AddSubExpandItem(remainingSegments, expandedItem);
        }
        else
        {
            // for example: $expand=nav, if we couldn't find a structural property in the path, it means we get the last navigation segment.
            // So, the remaining segments should be null and the last segment should be "NavigationPropertySegment".
            Contract.Assert(remainingSegments == null);

            NavigationPropertySegment firstNavigationPropertySegment = firstNonTypeSegment as NavigationPropertySegment;
            Contract.Assert(firstNavigationPropertySegment != null);

            // Needn't add this navigation property into the include property.
            // Because this navigation property will be included separately.
            if (propertiesToExpand == null)
            {
                propertiesToExpand = new Dictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem>();
            }

            propertiesToExpand[firstNavigationPropertySegment.NavigationProperty] = expandedItem;
        }
    }

    /// <summary>
    /// Process the <see cref="PathSelectItem"/>.
    /// </summary>
    /// <param name="pathSelectItem">The selected item.</param>
    /// <param name="navigationSource">The navigation source.</param>
    /// <param name="currentLevelPropertiesInclude">The current level properties included.</param>
    /// <returns>true if it's dynamic property selection, false if it's not.</returns>
    private static DynamicPathSegment ProcessSelectedItem(PathSelectItem pathSelectItem,
        IEdmNavigationSource navigationSource,
        IDictionary<IEdmStructuralProperty, SelectExpandIncludedProperty> currentLevelPropertiesInclude)
    {
        Contract.Assert(pathSelectItem != null && pathSelectItem.SelectedPath != null);
        Contract.Assert(currentLevelPropertiesInclude != null);

        // Verify and process the $select path
        IList<ODataPathSegment> remainingSegments;
        ODataPathSegment firstNonTypeSegment = pathSelectItem.SelectedPath.GetFirstNonTypeCastSegment(out remainingSegments);

        // for $select=NS.SubType/Property, we don't care about the leading type segment, because with or without the type segment
        // the "Property" property value expression should be built into the property container.

        PropertySegment firstStructuralPropertySegment = firstNonTypeSegment as PropertySegment;
        if (firstStructuralPropertySegment != null)
        {
            // $select=abc/..../xyz
            SelectExpandIncludedProperty newPropertySelectItem;
            if (!currentLevelPropertiesInclude.TryGetValue(firstStructuralPropertySegment.Property, out newPropertySelectItem))
            {
                newPropertySelectItem = new SelectExpandIncludedProperty(firstStructuralPropertySegment, navigationSource);
                currentLevelPropertiesInclude[firstStructuralPropertySegment.Property] = newPropertySelectItem;
            }

            newPropertySelectItem.AddSubSelectItem(remainingSegments, pathSelectItem);
        }
        else
        {
            // If we can't find a PropertySegment, the $select path maybe selecting an operation, a navigation or dynamic property.
            // And the remaining segments should be null.
            Contract.Assert(remainingSegments == null);

            // For operation (action/function), needn't process it.
            // For navigation property, needn't process it here.

            // For dynamic property, let's test the last segment for this path select item.
            if (firstNonTypeSegment is DynamicPathSegment)
            {
                // for dynamic segment, there's no leading segment.
                return (DynamicPathSegment)firstNonTypeSegment;
            }
        }

        return null;
    }

    // To test whether the current selection is SelectAll on an open type
    private static bool IsSelectAllOnOpenType(SelectExpandClause selectExpandClause, IEdmStructuredType structuredType)
    {
        if (structuredType == null || !structuredType.IsOpen)
        {
            return false;
        }

        if (IsSelectAll(selectExpandClause))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Create an <see cref="Expression"/> for the $count in $select or $expand
    /// </summary>
    /// <param name="context">The <see cref="QueryBinderContext"/></param>
    /// <param name="source">Original <see cref="Expression"/> which we will be appending the count expression.</param>
    /// <param name="countOption">Boolean to indicate if count value is present in $expand or $select item.</param>
    /// <returns>The <see cref="Expression"/> to create.</returns>
    public virtual Expression CreateTotalCountExpression(QueryBinderContext context, Expression source, bool? countOption)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        if (source == null)
        {
            throw Error.ArgumentNull(nameof(source));
        }

        Expression countExpression = Expression.Constant(null, typeof(long?));
        if (countOption == null || !countOption.Value)
        {
            return countExpression;
        }

        Type elementType;
        if (!TypeHelper.IsCollection(source.Type, out elementType))
        {
            return countExpression;
        }

        MethodInfo countMethod;
        if (typeof(IQueryable).IsAssignableFrom(source.Type))
        {
            countMethod = ExpressionHelperMethods.QueryableCountGeneric.MakeGenericMethod(elementType);
        }
        else
        {
            countMethod = ExpressionHelperMethods.EnumerableCountGeneric.MakeGenericMethod(elementType);
        }

        // call Count() method.
        countExpression = Expression.Call(null, countMethod, new[] { source });

        if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            // source == null ? null : countExpression
            return Expression.Condition(
                   test: Expression.Equal(source, Expression.Constant(null)),
                   ifTrue: Expression.Constant(null, typeof(long?)),
                   ifFalse: ExpressionHelpers.ToNullable(countExpression));
        }
        else
        {
            return countExpression;
        }
    }

    private Expression BuildPropertyContainer(QueryBinderContext context, Expression source,
        IEdmStructuredType structuredType,
        IDictionary<IEdmNavigationProperty, ExpandedReferenceSelectItem> propertiesToExpand,
        IDictionary<IEdmStructuralProperty, PathSelectItem> propertiesToInclude,
        IList<string> computedProperties,
        ISet<IEdmStructuralProperty> autoSelectedProperties,
        bool isSelectingOpenTypeSegments,
        bool isSelectedAll)
    {
        IList<NamedPropertyExpression> includedProperties = new List<NamedPropertyExpression>();

        if (propertiesToExpand != null)
        {
            foreach (var propertyToExpand in propertiesToExpand)
            {
                // $expand=abc or $expand=abc/$ref
                BuildExpandedProperty(context, source, structuredType, propertyToExpand.Key, propertyToExpand.Value, includedProperties);
            }
        }

        if (propertiesToInclude != null)
        {
            foreach (var propertyToInclude in propertiesToInclude)
            {
                // $select=abc($select=...,$filter=...,$compute=...)....
                BuildSelectedProperty(context, source, structuredType, propertyToInclude.Key, propertyToInclude.Value, includedProperties);
            }
        }

        if (computedProperties != null)
        {
            foreach (var computedProperty in computedProperties)
            {
                // $select=computed&$compute=.... as computed
                BindComputedProperty(source, context, computedProperty, includedProperties);
            }
        }

        if (autoSelectedProperties != null)
        {
            foreach (IEdmStructuralProperty propertyToInclude in autoSelectedProperties)
            {
                Expression propertyName = CreatePropertyNameExpression(context, structuredType, propertyToInclude, source);
                Expression propertyValue = CreatePropertyValueExpression(context, structuredType, propertyToInclude, source, filterClause: null, computeClause: null, search: null);
                includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue)
                {
                    AutoSelected = true
                });
            }
        }

        if (isSelectingOpenTypeSegments)
        {
            BuildDynamicProperty(context, source, structuredType, includedProperties);
        }

        BindOrderByProperties(context, source, structuredType, includedProperties, isSelectedAll);

        // create a property container that holds all these property names and values.
        return PropertyContainer.CreatePropertyContainer(includedProperties);
    }

    /// <summary>
    /// Build the navigation property <see cref="IEdmNavigationProperty"/> into the included properties.
    /// The property name is the navigation property name.
    /// The property value is the navigation property value from the source and applied the nested query options.
    /// </summary>
    /// <param name="context">Wrapper for properties used by the <see cref="SelectExpandBinder"/>.</param>
    /// <param name="source">The source contains the navigation property.</param>
    /// <param name="structuredType">The structured type or its derived type contains the navigation property.</param>
    /// <param name="navigationProperty">The expanded navigation property.</param>
    /// <param name="expandedItem">The expanded navigation select item. It may contain the nested query options.</param>
    /// <param name="includedProperties">The container to hold the created property.</param>
    internal void BuildExpandedProperty(QueryBinderContext context, Expression source, IEdmStructuredType structuredType,
        IEdmNavigationProperty navigationProperty, ExpandedReferenceSelectItem expandedItem,
        IList<NamedPropertyExpression> includedProperties)
    {
        Contract.Assert(source != null);
        Contract.Assert(context != null);
        Contract.Assert(structuredType != null);
        Contract.Assert(navigationProperty != null);
        Contract.Assert(expandedItem != null);
        Contract.Assert(includedProperties != null);

        IEdmEntityType edmEntityType = navigationProperty.ToEntityType();
        IEdmModel model = context.Model;
        ODataQuerySettings settings = context.QuerySettings;

        ModelBoundQuerySettings querySettings = model.GetModelBoundQuerySettings(navigationProperty, edmEntityType);

        // TODO: Process $apply and $compute in the $expand here, will support later.
        // $apply=...; $compute=...

        // Expression:
        //       "navigation property name"
        Expression propertyName = CreatePropertyNameExpression(context, structuredType, navigationProperty, source);

        // Expression:
        //        source.NavigationProperty
        Expression propertyValue = CreatePropertyValueExpression(context, structuredType, navigationProperty, source, expandedItem.FilterOption, expandedItem.ComputeOption, expandedItem.SearchOption);

        // Sub select and expand could be null if the expanded navigation property is not further projected or expanded.
        SelectExpandClause subSelectExpandClause = GetOrCreateSelectExpandClause(navigationProperty, expandedItem);

        Expression nullCheck = GetNullCheckExpression(context, navigationProperty, propertyValue, subSelectExpandClause);

        Expression countExpression = CreateTotalCountExpression(context, propertyValue, expandedItem.CountOption);

        int? modelBoundPageSize = querySettings == null ? null : querySettings.PageSize;
        propertyValue = ProjectAsWrapper(context, propertyValue, subSelectExpandClause, edmEntityType, expandedItem.NavigationSource,
            expandedItem.OrderByOption, // $orderby=...
            expandedItem.ComputeOption,
            expandedItem.TopOption, // $top=...
            expandedItem.SkipOption, // $skip=...
            modelBoundPageSize);

        NamedPropertyExpression propertyExpression = new NamedPropertyExpression(propertyName, propertyValue);
        if (subSelectExpandClause != null)
        {
            if (!navigationProperty.Type.IsCollection())
            {
                propertyExpression.NullCheck = nullCheck;
            }
            else if (settings.PageSize.HasValue)
            {
                propertyExpression.PageSize = settings.PageSize.Value;
            }
            else
            {
                if (querySettings != null && querySettings.PageSize.HasValue)
                {
                    propertyExpression.PageSize = querySettings.PageSize.Value;
                }
            }

            propertyExpression.TotalCount = countExpression;
            propertyExpression.CountOption = expandedItem.CountOption;
        }

        includedProperties.Add(propertyExpression);
    }

    /// <summary>
    /// Build the structural property <see cref="IEdmStructuralProperty"/> into the included properties.
    /// The property name is the structural property name.
    /// The property value is the structural property value from the source and applied the nested query options.
    /// </summary>
    /// <param name="context">Wrapper for properties used by the <see cref="SelectExpandBinder"/>.</param>
    /// <param name="source">The source contains the structural property.</param>
    /// <param name="structuredType">The structured type or its derived type contains the structural property.</param>
    /// <param name="structuralProperty">The selected structural property.</param>
    /// <param name="pathSelectItem">The selected item. It may contain the nested query options and could be null.</param>
    /// <param name="includedProperties">The container to hold the created property.</param>
    internal void BuildSelectedProperty(QueryBinderContext context, Expression source, IEdmStructuredType structuredType,
        IEdmStructuralProperty structuralProperty, PathSelectItem pathSelectItem,
        IList<NamedPropertyExpression> includedProperties)
    {
        Contract.Assert(source != null);
        Contract.Assert(context != null);
        Contract.Assert(structuredType != null);
        Contract.Assert(structuralProperty != null);
        Contract.Assert(includedProperties != null);

        IEdmModel model = context.Model;
        ODataQuerySettings settings = context.QuerySettings;

        // // Expression:
        //       "navigation property name"
        Expression propertyName = CreatePropertyNameExpression(context, structuredType, structuralProperty, source);

        // Expression:
        //        source.NavigationProperty
        Expression propertyValue;
        if (pathSelectItem == null)
        {
            propertyValue = CreatePropertyValueExpression(context, structuredType, structuralProperty, source, filterClause: null, computeClause: null, search: null);
            includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
            return;
        }

        SelectExpandClause subSelectExpandClause = pathSelectItem.SelectAndExpand;

        // TODO: Process $compute in the $select ahead.
        // $compute=...

        propertyValue = CreatePropertyValueExpression(context, structuredType, structuralProperty, source, pathSelectItem.FilterOption, pathSelectItem.ComputeOption, pathSelectItem.SearchOption);
        Type propertyValueType = propertyValue.Type;
        if (propertyValueType == typeof(char[]) || propertyValueType == typeof(byte[]))
        {
            includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
            return;
        }

        Expression nullCheck = GetNullCheckExpression(structuralProperty, propertyValue, subSelectExpandClause);

        Expression countExpression = CreateTotalCountExpression(context, propertyValue, pathSelectItem.CountOption);

        // be noted: the property structured type could be null, because the property maybe not a complex property.
        IEdmStructuredType propertyStructuredType = structuralProperty.Type.ToStructuredType();
        ModelBoundQuerySettings querySettings = null;
        if (propertyStructuredType != null)
        {
            querySettings = model.GetModelBoundQuerySettings(structuralProperty, propertyStructuredType);
        }

        int? modelBoundPageSize = querySettings == null ? null : querySettings.PageSize;
        propertyValue = ProjectAsWrapper(context, propertyValue, subSelectExpandClause, propertyStructuredType, pathSelectItem.NavigationSource,
            pathSelectItem.OrderByOption, // $orderby=...
            pathSelectItem.ComputeOption,
            pathSelectItem.TopOption, // $top=...
            pathSelectItem.SkipOption, // $skip=...
            modelBoundPageSize);

        NamedPropertyExpression propertyExpression = new NamedPropertyExpression(propertyName, propertyValue);
        if (subSelectExpandClause != null)
        {
            if (!structuralProperty.Type.IsCollection())
            {
                propertyExpression.NullCheck = nullCheck;
            }

            propertyExpression.TotalCount = countExpression;
            propertyExpression.CountOption = pathSelectItem.CountOption;
        }

        includedProperties.Add(propertyExpression);
    }

    /// <summary>
    /// Bind the computed property.
    /// </summary>
    /// <param name="source">The source contains the compute property.</param>
    /// <param name="context">Wrapper for properties used by the <see cref="SelectExpandBinder"/>.</param>
    /// <param name="computedProperty">The compute property name.</param>
    /// <param name="includedProperties">The container to hold the created property.</param>
    public virtual void BindComputedProperty(Expression source, QueryBinderContext context, string computedProperty,
        IList<NamedPropertyExpression> includedProperties)
    {
        if (source == null)
        {
            throw Error.ArgumentNull(nameof(source));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        if (includedProperties == null)
        {
            throw Error.ArgumentNull(nameof(includedProperties));
        }

        if (!context.ComputedProperties.TryGetValue(computedProperty, out var computeExpression))
        {
            return;
        }

        Expression backSource = context.Source;
        context.Source = source;

        // Pay attention: When it's nested $compute, the reference range variable is the source here.
        Expression propertyValue = Bind(computeExpression.Expression, context);
        Expression propertyName = Expression.Constant(computedProperty);
        includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));

        context.Source = backSource;
    }

    /// <summary>
    /// Build the dynamic properties into the included properties.
    /// </summary>
    /// <param name="context">The wrapper for properties used by the <see cref="SelectExpandBinder"/>.</param>
    /// <param name="source">The source contains the dynamic property.</param>
    /// <param name="structuredType">The structured type contains the dynamic property.</param>
    /// <param name="includedProperties">The container to hold the created property.</param>
    public virtual void BuildDynamicProperty(QueryBinderContext context, Expression source, IEdmStructuredType structuredType,
        IList<NamedPropertyExpression> includedProperties)
    {
        Contract.Assert(source != null);
        Contract.Assert(context != null);
        Contract.Assert(structuredType != null);
        Contract.Assert(includedProperties != null);

        IEdmModel model = context.Model;
        ODataQuerySettings settings = context.QuerySettings;

        PropertyInfo dynamicPropertyDictionary = model.GetDynamicPropertyDictionary(structuredType);
        if (dynamicPropertyDictionary != null)
        {
            Expression propertyName = Expression.Constant(dynamicPropertyDictionary.Name);
            Expression propertyValue = Expression.Property(source, dynamicPropertyDictionary.Name);
            Expression nullablePropertyValue = ExpressionHelpers.ToNullable(propertyValue);
            if (settings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // source == null ? null : propertyValue
                propertyValue = Expression.Condition(
                    test: Expression.Equal(source, Expression.Constant(value: null)),
                    ifTrue: Expression.Constant(value: null, type: TypeHelper.ToNullable(propertyValue.Type)),
                    ifFalse: nullablePropertyValue);
            }
            else
            {
                propertyValue = nullablePropertyValue;
            }

            includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue));
        }
    }

    /// <summary>
    /// Build the orderby clause into the included properties.
    /// For example: $orderby=tolower(substring(name,1,2))
    /// </summary>
    /// <param name="context">The query binder context.</param>
    /// <param name="source">The source.</param>
    /// <param name="structuredType">The structured type.</param>
    /// <param name="includedProperties">The container to hold the created property.</param>
    /// <param name="isSelectedAll">Is select all flag.</param>
    protected virtual void BindOrderByProperties(QueryBinderContext context, Expression source, IEdmStructuredType structuredType,
        IList<NamedPropertyExpression> includedProperties, bool isSelectedAll)
    {
        if (context == null || context.OrderByClauses == null || source == null || structuredType == null || includedProperties == null)
        {
            return;
        }

        // Avoid duplicated binding.
        HashSet<string> usedPropertyNames = new HashSet<string>(includedProperties.Count);
        foreach (var p in includedProperties)
        {
            if (p.Name is ConstantExpression constExp && constExp.Type == typeof(string))
            {
                usedPropertyNames.Add(constExp.Value.ToString());
            }
        }

        Expression propertyName;
        Expression propertyValue;
        List<string> names = new List<string>(context.OrderByClauses.Count);
        string name;
        int index = 1;
        foreach (OrderByClause clause in context.OrderByClauses)
        {
            // It could do things duplicated, for example if we have:
            // $orderby=tolower(dynamic)$compute=...as dynamic&$select=dynamic
            // dynamic is bind already since it's in $select, ideally we don't need to bind 'dynamic' in $orderby again (even we cached the compute binding once)
            // Besides, for 'top-level' key orderby, since the key properties are auto-select, we don't need to bind them again.
            if (clause.IsTopLevelSingleProperty(out IEdmProperty edmProperty, out name))
            {
                // $orderby=Id or
                // $orderby=computedProp&$select=computedProp
                if ((edmProperty != null && isSelectedAll) || usedPropertyNames.Contains(name))
                {
                    continue;
                }
            }

            name = GetOrderByName(usedPropertyNames, ref index);
            names.Add(name);

            // Since we generate the Unique name, it's safe NOT adding the generated name into set.
            // Leave this code here for reference.
            //usedPropertyNames.Add(name);

            propertyName = Expression.Constant(name);
            propertyValue = Bind(clause.Expression, context);
            includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue)
            {
                AutoSelected = true
            });
        }

        if (names.Count > 0)
        {
            // We use this to keep the order and naming mapping.
            propertyName = Expression.Constant(OrderByClauseHelpers.OrderByGlobalNameKey);
            propertyValue = Expression.Constant(string.Join(",", names));
            includedProperties.Add(new NamedPropertyExpression(propertyName, propertyValue)
            {
                AutoSelected = true
            });
        }
    }

    private static string GetOrderByName(HashSet<string> usedPropertyNames, ref int start)
    {
        do
        {
            // For advanced orderby, for example: $orderby=tolower(substring(location/street,1,2))
            // we want to create a 'propertyName: propertyValue' binding.
            // Since we cannot use the raw orderby literal to naming the property name, since it could be complicated and invalid
            // We create a random name for each orderby clause.
            string name = $"{OrderByClauseHelpers.OrderByPropertyNamePrefix}{start++}";
            if (!usedPropertyNames.Contains(name))
            {
                return name;
            }
        }
        while (true);
    }

    private static SelectExpandClause GetOrCreateSelectExpandClause(IEdmNavigationProperty navigationProperty, ExpandedReferenceSelectItem expandedItem)
    {
        // for normal $expand=....
        ExpandedNavigationSelectItem expandNavigationSelectItem = expandedItem as ExpandedNavigationSelectItem;
        if (expandNavigationSelectItem != null)
        {
            return expandNavigationSelectItem.SelectAndExpand;
        }

        // for $expand=..../$ref, just includes the keys properties
        IList<SelectItem> selectItems = new List<SelectItem>();
        foreach (IEdmStructuralProperty keyProperty in navigationProperty.ToEntityType().Key())
        {
            selectItems.Add(new PathSelectItem(new ODataSelectPath(new PropertySegment(keyProperty))));
        }

        return new SelectExpandClause(selectItems, false);
    }

    private Expression AddOrderByQueryForSource(QueryBinderContext context, Expression source, OrderByClause orderbyClause, Type elementType)
    {
        if (orderbyClause != null && IsAvailableODataQueryOption(context.QuerySettings, AllowedQueryOptions.OrderBy))
        {
            // TODO: Implement proper support for $select/$expand after $apply
            QueryBinderContext binderContext = CreateSubContext(context, null, elementType);

            source = OrderByBinder.ApplyBind(source, orderbyClause, binderContext, false);
        }

        return source;
    }

    private QueryBinderContext CreateSubContext(QueryBinderContext context, ComputeClause computeClause, Type clrElementType,
        HandleNullPropagationOption option = HandleNullPropagationOption.True)
    {
        ODataQuerySettings newSettings = new ODataQuerySettings();
        newSettings.CopyFrom(context.QuerySettings);
        newSettings.HandleNullPropagation = option;
        QueryBinderContext binderContext = new QueryBinderContext(context, newSettings, clrElementType);
        if (computeClause != null)
        {
            binderContext.AddComputedProperties(computeClause.ComputedItems);
        }

        return binderContext;
    }

    private static Expression GetNullCheckExpression(IEdmStructuralProperty propertyToInclude, Expression propertyValue,
        SelectExpandClause projection)
    {
        if (projection == null || propertyToInclude.Type.IsCollection())
        {
            return null;
        }

        if (IsSelectAll(projection) && propertyToInclude.Type.IsComplex())
        {
            // for Collections (Primitive, Enum, Complex collection), that's check above.
            return Expression.Equal(propertyValue, Expression.Constant(null));
        }

        return null;
    }

    private Expression GetNullCheckExpression(QueryBinderContext context, IEdmNavigationProperty propertyToExpand, Expression propertyValue,
        SelectExpandClause projection)
    {
        if (projection == null || propertyToExpand.Type.IsCollection())
        {
            return null;
        }

        if (IsSelectAll(projection) || !propertyToExpand.ToEntityType().Key().Any())
        {
            return Expression.Equal(propertyValue, Expression.Constant(null));
        }

        Expression keysNullCheckExpression = null;
        foreach (var key in propertyToExpand.ToEntityType().Key())
        {
            var propertyValueExpression = CreatePropertyValueExpression(context, propertyToExpand.ToEntityType(), key, propertyValue, filterClause: null, search: null);
            var keyExpression = Expression.Equal(
                propertyValueExpression,
                Expression.Constant(null, propertyValueExpression.Type));

            keysNullCheckExpression = keysNullCheckExpression == null
                ? keyExpression
                : Expression.And(keysNullCheckExpression, keyExpression);
        }

        return keysNullCheckExpression;
    }

    // new CollectionWrapper<ElementType> { Instance = source.Select((ElementType element) => new Wrapper { }) }
    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
    private Expression ProjectCollection(QueryBinderContext context, Expression source, Type elementType,
        SelectExpandClause selectExpandClause, IEdmStructuredType structuredType, IEdmNavigationSource navigationSource,
        OrderByClause orderByClause,
        long? topOption,
        long? skipOption,
        int? modelBoundPageSize)
    {
        ODataQuerySettings settings = context.QuerySettings;

        // structuralType could be null, because it can be primitive collection.

        ParameterExpression element = context.CurrentParameter;

        Expression projection;
        // expression
        //      new Wrapper { }
        if (structuredType != null)
        {
            projection = ProjectElement(context, element, selectExpandClause, structuredType, navigationSource);
        }
        else
        {
            projection = element;
        }

        // expression
        //      (ElementType element) => new Wrapper { }
        LambdaExpression selector = Expression.Lambda(projection, element);

        if (orderByClause != null)
        {
            source = AddOrderByQueryForSource(context, source, orderByClause, elementType);
        }

        bool hasTopValue = topOption != null && topOption.HasValue;
        bool hasSkipvalue = skipOption != null && skipOption.HasValue;

        if (structuredType is IEdmEntityType entityType)
        {
            if (settings.PageSize.HasValue || modelBoundPageSize.HasValue || hasTopValue || hasSkipvalue)
            {
                // nested paging. Need to apply order by first, and take one more than page size as we need to know
                // whether the collection was truncated or not while generating next page links.
                IEnumerable<IEdmStructuralProperty> properties =
                    entityType.Key().Any()
                        ? entityType.Key()
                        : entityType
                            .StructuralProperties()
                            .Where(property => property.Type.IsPrimitive() && !property.Type.IsStream())
                            .OrderBy(property => property.Name);

                if (orderByClause == null)
                {
                    bool alreadyOrdered = false;
                    foreach (var prop in properties)
                    {
                        string propertyName = context.Model.GetClrPropertyName(prop);
                        source = ExpressionHelpers.OrderByPropertyExpression(source, propertyName, elementType,
                            alreadyOrdered);

                        if (!alreadyOrdered)
                        {
                            alreadyOrdered = true;
                        }
                    }
                }
            }
        }

        if (hasSkipvalue && IsAvailableODataQueryOption(context.QuerySettings, AllowedQueryOptions.Skip))
        {
            Contract.Assert(skipOption.Value <= Int32.MaxValue);
            source = ExpressionHelpers.Skip(source, (int)skipOption.Value, elementType,
                settings.EnableConstantParameterization);
        }

        if (hasTopValue && IsAvailableODataQueryOption(context.QuerySettings, AllowedQueryOptions.Top))
        {
            Contract.Assert(topOption.Value <= Int32.MaxValue);
            source = ExpressionHelpers.Take(source, (int)topOption.Value, elementType,
                settings.EnableConstantParameterization);
        }

        if (structuredType is IEdmEntityType)
        {
            if (settings.PageSize.HasValue || modelBoundPageSize.HasValue)
            {
                // don't page nested collections if EnableCorrelatedSubqueryBuffering is enabled
                if (!settings.EnableCorrelatedSubqueryBuffering)
                {
                    if (settings.PageSize.HasValue)
                    {
                        source = ExpressionHelpers.Take(source, settings.PageSize.Value + 1, elementType,
                            settings.EnableConstantParameterization);
                    }
                    else if (settings.ModelBoundPageSize.HasValue)
                    {
                        source = ExpressionHelpers.Take(source, modelBoundPageSize.Value + 1, elementType,
                            settings.EnableConstantParameterization);
                    }
                }
            }
        }

        // expression
        //      source.Select((ElementType element) => new Wrapper { })
        var selectMethod = GetSelectMethod(elementType, projection.Type);
        Expression selectedExpresion = Expression.Call(selectMethod, source, selector);

        // Append ToList() to collection as a hint to LINQ provider to buffer correlated sub-queries in memory and avoid executing N+1 queries
        if (settings.EnableCorrelatedSubqueryBuffering)
        {
            selectedExpresion = Expression.Call(ExpressionHelperMethods.QueryableToList.MakeGenericMethod(projection.Type), selectedExpresion);
        }

        if (settings.HandleNullPropagation == HandleNullPropagationOption.True)
        {
            // source == null ? null : projectedCollection
            return Expression.Condition(
                   test: Expression.Equal(source, Expression.Constant(null)),
                   ifTrue: Expression.Constant(null, selectedExpresion.Type),
                   ifFalse: selectedExpresion);
        }
        else
        {
            return selectedExpresion;
        }
    }

    // OData formatter requires the type name of the entity that is being written if the type has derived types.
    // Expression
    //      source is GrandChild ? "GrandChild" : ( source is Child ? "Child" : "Root" )
    // Notice that the order is important here. The most derived type must be the first to check.
    // If entity framework had a way to figure out the type name without selecting the whole object, we don't have to do this magic.
    /// <summary>
    /// Create <see cref="Expression"/> for Derived types.
    /// </summary>
    /// <param name="source">The original <see cref="Expression"/>.</param>
    /// <param name="elementType">The <see cref="IEdmStructuredType"/> which may contain the derived types.</param>
    /// <param name="model">The <see cref="IEdmModel"/>.</param>
    /// <returns>The <see cref="Expression"/> with derived types if any are present.</returns>
    public virtual Expression CreateTypeNameExpression(Expression source, IEdmStructuredType elementType, IEdmModel model)
    {
        if (source == null)
        {
            throw Error.ArgumentNull(nameof(source));
        }

        if (elementType == null)
        {
            throw Error.ArgumentNull(nameof(elementType));
        }

        if (model == null)
        {
            throw Error.ArgumentNull(nameof(model));
        }

        IReadOnlyList<IEdmStructuredType> derivedTypes = GetAllDerivedTypes(elementType, model);
        if (derivedTypes.Count == 0)
        {
            // no inheritance.
            return null;
        }
        else
        {
            Expression expression = Expression.Constant(elementType.FullTypeName());
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                Type clrType = model.GetClrType(derivedTypes[i]);
                if (clrType == null)
                {
                    throw new ODataException(Error.Format(SRResources.MappingDoesNotContainResourceType, derivedTypes[0].FullTypeName()));
                }

                expression = Expression.Condition(
                    test: Expression.TypeIs(source, clrType),
                    ifTrue: Expression.Constant(derivedTypes[i].FullTypeName()),
                    ifFalse: expression);
            }

            return expression;
        }
    }

    private static bool IsAvailableODataQueryOption(ODataQuerySettings querySettings, AllowedQueryOptions queryOptionFlag)
    {
        return (querySettings.IgnoredNestedQueryOptions & queryOptionFlag) == AllowedQueryOptions.None;
    }

    // returns all the derived types (direct and indirect) of baseType ordered according to their depth. The direct children
    // are the first in the list.
    private static IReadOnlyList<IEdmStructuredType> GetAllDerivedTypes(IEdmStructuredType baseType, IEdmModel model)
    {
        IEnumerable<IEdmStructuredType> allStructuredTypes = model.SchemaElements.OfType<IEdmStructuredType>();

        List<Tuple<int, IEdmStructuredType>> derivedTypes = new List<Tuple<int, IEdmStructuredType>>();
        foreach (IEdmStructuredType structuredType in allStructuredTypes)
        {
            int distance = IsDerivedTypeOf(structuredType, baseType);
            if (distance > 0)
            {
                derivedTypes.Add(Tuple.Create(distance, structuredType));
            }
        }

        return derivedTypes.OrderBy(tuple => tuple.Item1).Select(tuple => tuple.Item2).ToList();
    }

    // returns -1 if type does not derive from baseType and a positive number representing the distance
    // between them if it does.
    private static int IsDerivedTypeOf(IEdmStructuredType type, IEdmStructuredType baseType)
    {
        int distance = 0;
        while (type != null)
        {
            if (baseType == type)
            {
                return distance;
            }

            type = type.BaseType();
            distance++;
        }

        return -1;
    }

    private static MethodInfo GetSelectMethod(Type elementType, Type resultType)
    {
        return ExpressionHelperMethods.EnumerableSelectGeneric.MakeGenericMethod(elementType, resultType);
    }

    private static bool IsSelectAll(SelectExpandClause selectExpandClause)
    {
        if (selectExpandClause == null)
        {
            return true;
        }

        if (selectExpandClause.AllSelected || selectExpandClause.SelectedItems.OfType<WildcardSelectItem>().Any())
        {
            return true;
        }

        return false;
    }

    private static Type GetWrapperGenericType(bool isInstancePropertySet, bool isTypeNamePropertySet, bool isContainerPropertySet)
    {
        if (isInstancePropertySet)
        {
            // select all
            Contract.Assert(!isTypeNamePropertySet, "we don't set type name if we set instance as it can be figured from instance");

            return isContainerPropertySet ? typeof(SelectAllAndExpand<>) : typeof(SelectAll<>);
        }
        else
        {
            Contract.Assert(isContainerPropertySet, "if it is not select all, container should hold something");

            return isTypeNamePropertySet ? typeof(SelectSomeAndInheritance<>) : typeof(SelectSome<>);
        }
    }

    private class ReferenceNavigationPropertyExpandFilterVisitor : ExpressionVisitor
    {
        private Expression _source;
        private ParameterExpression _parameterExpression;

        public ReferenceNavigationPropertyExpandFilterVisitor(ParameterExpression parameterExpression, Expression source)
        {
            _source = source;
            _parameterExpression = parameterExpression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node != _parameterExpression)
            {
                throw new ODataException(Error.Format(SRResources.ReferenceNavigationPropertyExpandFilterVisitorUnexpectedParameter, node.Name));
            }

            return _source;
        }
    }
}
