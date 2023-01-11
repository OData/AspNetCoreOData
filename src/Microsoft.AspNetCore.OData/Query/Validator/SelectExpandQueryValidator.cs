//-----------------------------------------------------------------------------
// <copyright file="SelectExpandQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SelectExpandQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SelectExpandQueryValidator : ISelectExpandQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SelectExpandQueryOption" />.
        /// </summary>
        /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings)
        {
            if (selectExpandQueryOption == null)
            {
                throw Error.ArgumentNull(nameof(selectExpandQueryOption));
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            SelectExpandValidatorContext validatorContext = new SelectExpandValidatorContext
            {
                SelectExpand = selectExpandQueryOption,
                Context = selectExpandQueryOption.Context,
                ValidationSettings = validationSettings,
                Property = selectExpandQueryOption.Context.TargetProperty,
                StructuredType = selectExpandQueryOption.Context.TargetStructuredType,
                CurrentDepth = 0
            };

            ValidateSelectExpand(selectExpandQueryOption.SelectExpandClause, validatorContext);

            if (validationSettings.MaxExpansionDepth > 0)
            {
                if (selectExpandQueryOption.LevelsMaxLiteralExpansionDepth < 0)
                {
                    selectExpandQueryOption.LevelsMaxLiteralExpansionDepth = validationSettings.MaxExpansionDepth;
                }
                else if (selectExpandQueryOption.LevelsMaxLiteralExpansionDepth > validationSettings.MaxExpansionDepth)
                {
                    throw new ODataException(Error.Format(
                        SRResources.InvalidExpansionDepthValue,
                        "LevelsMaxLiteralExpansionDepth",
                        "MaxExpansionDepth"));
                }

                ValidateDepth(selectExpandQueryOption.SelectExpandClause, validationSettings.MaxExpansionDepth);
            }
        }

        /// <summary>
        /// Validates all select and expand items in $select and $expand.
        /// For example, ~/Customers?$expand=Nav($expand=subNav;$select=Prop;$top=2)&$select=Addresses($select=City;$top=1)
        /// </summary>
        /// <param name="selectExpandClause">The $select and $expand clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateSelectExpand(SelectExpandClause selectExpandClause, SelectExpandValidatorContext validatorContext)
        {
            if (selectExpandClause == null)
            {
                return;
            }

            foreach (SelectItem selectItem in selectExpandClause.SelectedItems)
            {
                if (selectItem is ExpandedNavigationSelectItem expandedSelectItem)
                {
                    // $expand=Nav
                    ValidateExpandedNavigationSelectItem(expandedSelectItem, validatorContext);
                }
                else if (selectItem is ExpandedCountSelectItem expandedCountSelectItem)
                {
                    // $expand=Nav/$count
                    ValidateExpandedCountSelectItem(expandedCountSelectItem, validatorContext);
                }
                else if (selectItem is PathSelectItem pathSelectItem)
                {
                    // $select=Prop
                    ValidatePathSelectItem(pathSelectItem, validatorContext);
                }
                else if (selectItem is WildcardSelectItem wildCardSelectItem)
                {
                    // $select=*
                    ValidateWildcardSelectItem(wildCardSelectItem, validatorContext);
                }
                else if (selectItem is NamespaceQualifiedWildcardSelectItem namespaceQualifiedWildcardSelectItem)
                {
                    // $select=NS.*
                    ValidateNamespaceQualifiedWildcardSelectItem(namespaceQualifiedWildcardSelectItem, validatorContext);
                }
                else if (selectItem is ExpandedReferenceSelectItem referSelectItem) // Let ExpandedReferenceSelectItem be the last one.
                {
                    // $expand=Nav/$ref
                    ValidateExpandedReferenceSelectItem(referSelectItem, validatorContext);
                }
            }
        }

        /// <summary>
        /// Validates one $expand. For example, ~/Customers?$expand=Nav($expand=subNav;$select=Prop;$top=2)
        /// </summary>
        /// <param name="expandItem">One $expand clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        /// <exception cref="ODataException">The thrown exception.</exception>
        protected virtual void ValidateExpandedNavigationSelectItem(ExpandedNavigationSelectItem expandItem, SelectExpandValidatorContext validatorContext)
        {
            if (expandItem == null)
            {
                return;
            }

            if (validatorContext.RemainingDepth != null && validatorContext.RemainingDepth <= 0)
            {
                throw new ODataException(
                    Error.Format(SRResources.MaxExpandDepthExceeded, validatorContext.CurrentDepth, "MaxExpansionDepth"));
            }

            ODataValidationSettings validationSettings = validatorContext.ValidationSettings;
            int currentDepth = validatorContext.CurrentDepth;

            // if validationSettings.MaxExpansionDepth <= 0, It means to disable the maximum expansion depth check.
            // if currentDepth (starting 0) is bigger than max depth, throw exception.
            if (validationSettings.MaxExpansionDepth > 0 && currentDepth > validationSettings.MaxExpansionDepth)
            {
                throw new ODataException(Error.Format(SRResources.MaxExpandDepthExceeded, validationSettings.MaxExpansionDepth, "MaxExpansionDepth"));
            }

            // $expand=a/b/NS.C/NavProp
            NavigationPropertySegment navigationSegment = (NavigationPropertySegment)expandItem.PathToNavigationProperty.LastSegment;
            IEdmNavigationProperty property = navigationSegment.NavigationProperty;

            IEdmModel edmModel = validatorContext.Context.Model;
            // Check the "old" NotExpandable configuration on the property. Could be remove later.
            if (EdmHelpers.IsNotExpandable(property, edmModel))
            {
                throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand, property.Name));
            }

            int? remainDepth = validatorContext.RemainingDepth;
            // Check the "new" model query configuation on the type and property.
            // The following logic looks weird (I copied from existing codes with small changes).
            // TODO: We should combine them (old/new) together and figure out a whole solution for the query configuration on the model.
            bool isExpandable;
            ExpandConfiguration expandConfiguration;
            isExpandable = EdmHelpers.IsExpandable(property.Name, validatorContext.Property, validatorContext.StructuredType, edmModel, out expandConfiguration);
            if (isExpandable)
            {
                int maxDepth = expandConfiguration.MaxDepth;
                if (maxDepth > 0 && (validatorContext.RemainingDepth == null || maxDepth < validatorContext.RemainingDepth))
                {
                    remainDepth = maxDepth;
                }

                //if (expandConfiguration.MaxDepth > 0 && currentDepth >= expandConfiguration.MaxDepth)
                //{
                //    throw new ODataException(Error.Format(SRResources.MaxExpandDepthExceeded, validationSettings.MaxExpansionDepth, "MaxExpansionDepth"));
                //}
            }
            else if (!isExpandable)
            {
                if (!validatorContext.Context.DefaultQuerySettings.EnableExpand ||
                    (expandConfiguration != null && expandConfiguration.ExpandType == SelectExpandType.Disabled))
                {
                    throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand, property.Name));
                }
            }

            // Move to next level within $expand. First, let's update the applied property and related structured type.
            SelectExpandValidatorContext subValidatorContext = validatorContext.Clone();
            subValidatorContext.Property = property;
            subValidatorContext.StructuredType = property.ToEntityType();
            subValidatorContext.CurrentDepth = validatorContext.CurrentDepth + 1;
            subValidatorContext.RemainingDepth = remainDepth != null ? remainDepth - 1 : null;

            // Validate the nested $select and $expand within $expand
            ValidateSelectExpand(expandItem.SelectAndExpand, subValidatorContext);

            // Validate the nested $filter within $expand
            ValidateFilter(expandItem.FilterOption, subValidatorContext);

            // Validate the nested $orderby within $expand
            ValidateOrderby(expandItem.OrderByOption, subValidatorContext);

            // Validate the nested $top within $expand
            ValidateTop(expandItem.TopOption, subValidatorContext);

            // Validate the nested $skip within $expand
            ValidateSkip(expandItem.SkipOption, subValidatorContext);

            // Validate the nested $count within $expand
            ValidateCount(expandItem.CountOption, subValidatorContext);

            // Validate the nested $search within $expand
            ValidateSearch(expandItem.SearchOption, subValidatorContext);

            // Validate the nested $levels within $expand
            ValidateLevels(expandItem.LevelsOption, subValidatorContext);

            // Validate the nested $compute within $expand
            ValidateCompute(expandItem.ComputeOption, subValidatorContext);

            // Validate the nested $apply within $expand
            ValidateApply(expandItem.ApplyOption, subValidatorContext);
        }

        /// <summary>
        /// Validates one expand count. For example, ~/Customers?$expand=Nav/$count
        /// </summary>
        /// <param name="expandCountItem">The expand count item.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateExpandedCountSelectItem(ExpandedCountSelectItem expandCountItem, SelectExpandValidatorContext validatorContext)
        {
            // So far, No default validation logic here.
        }

        /// <summary>
        /// Validates one expand reference. For example, ~/Customers?$expand=Nav/$ref
        /// </summary>
        /// <param name="expandReferItem">The expand reference item.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateExpandedReferenceSelectItem(ExpandedReferenceSelectItem expandReferItem, SelectExpandValidatorContext validatorContext)
        {
            // So far, No default validation logic here.
        }

        /// <summary>
        /// Validates $select. For example, ~/Customers?$select=Prop($select=SubProp;$top=2)
        /// </summary>
        /// <param name="pathSelectItem"></param>
        /// <param name="validatorContext">The validator context.</param>
        /// <exception cref="ODataException">The thrown exception.</exception>
        protected virtual void ValidatePathSelectItem(PathSelectItem pathSelectItem, SelectExpandValidatorContext validatorContext)
        {
            if (pathSelectItem == null)
            {
                return;
            }

            IEdmModel edmModel = validatorContext.Context.Model;
            bool enableSelect = validatorContext.Context.DefaultQuerySettings.EnableSelect;
            ODataPathSegment segment = pathSelectItem.SelectedPath.LastSegment;

            IEdmProperty property = validatorContext.Property;
            IEdmStructuredType structuredType = validatorContext.StructuredType;

            if (segment is NavigationPropertySegment navigationPropertySegment)
            {
                IEdmNavigationProperty navProperty = navigationPropertySegment.NavigationProperty;
                if (EdmHelpers.IsNotNavigable(navProperty, edmModel))
                {
                    throw new ODataException(Error.Format(SRResources.NotNavigablePropertyUsedInNavigation, navProperty.Name));
                }

                property = navProperty;
                structuredType = navProperty.ToEntityType();
            }
            else if (segment is PropertySegment propertySegment)
            {
                if (EdmHelpers.IsNotSelectable(propertySegment.Property, property, structuredType, edmModel, enableSelect))
                {
                    throw new ODataException(Error.Format(SRResources.NotSelectablePropertyUsedInSelect, propertySegment.Property.Name));
                }

                property = propertySegment.Property;
                structuredType = GetStructuredType(propertySegment.Property.Type);
            }
            else
            {
                return;
            }

            // Move to next level within $select. Let's update the applied property and related structured type.
            SelectExpandValidatorContext subValidatorContext = validatorContext.Clone();
            subValidatorContext.Property = property;
            subValidatorContext.StructuredType = structuredType;
            subValidatorContext.CurrentDepth = validatorContext.CurrentDepth + 1;

            // Validate the nested $select within $select
            ValidateSelectExpand(pathSelectItem.SelectAndExpand, subValidatorContext);

            // Validate the nested $filter within $select
            ValidateFilter(pathSelectItem.FilterOption, subValidatorContext);

            // Validate the nested $orderby within $select
            ValidateOrderby(pathSelectItem.OrderByOption, subValidatorContext);

            // Validate the nested $top within $select
            ValidateTop(pathSelectItem.TopOption, subValidatorContext);

            // Validate the nested $skip within $select
            ValidateSkip(pathSelectItem.SkipOption, subValidatorContext);

            // Validate the nested $count within $select
            ValidateCount(pathSelectItem.CountOption, subValidatorContext);

            // Validate the nested $search within $select
            ValidateSearch(pathSelectItem.SearchOption, subValidatorContext);

            // Validate the nested $compute within $select
            ValidateCompute(pathSelectItem.ComputeOption, subValidatorContext);
        }

        /// <summary>
        /// Validates $select wildcard. For example, ~/Customers?$select=*
        /// </summary>
        /// <param name="wildCardSelectItem">The wildcard select item.</param>
        /// <param name="validatorContext">The validator context.</param>
        /// <exception cref="ODataException">The thrown exception.</exception>
        protected virtual void ValidateWildcardSelectItem(WildcardSelectItem wildCardSelectItem, SelectExpandValidatorContext validatorContext)
        {
            if (wildCardSelectItem == null)
            {
                return;
            }

            IEdmStructuredType structuredType = validatorContext.StructuredType;
            IEdmModel edmModel = validatorContext.Context.Model;
            IEdmProperty pathProperty = validatorContext.Property;

            foreach (var property in structuredType.StructuralProperties())
            {
                if (EdmHelpers.IsNotSelectable(property, pathProperty, structuredType, edmModel,
                    validatorContext.Context.DefaultQuerySettings.EnableSelect))
                {
                    throw new ODataException(Error.Format(SRResources.NotSelectablePropertyUsedInSelect, property.Name));
                }
            }
        }

        /// <summary>
        /// Validates $select namespace wildcard. For example, ~/Customers?$select=NS.*
        /// </summary>
        /// <param name="namespaceQualifiedWildcardSelectItem">The namespace wildcard select item.</param>
        /// <param name="validatorContext">The validator context.</param>
        /// <exception cref="ODataException">The thrown exception.</exception>
        protected virtual void ValidateNamespaceQualifiedWildcardSelectItem(
            NamespaceQualifiedWildcardSelectItem namespaceQualifiedWildcardSelectItem,
            SelectExpandValidatorContext validatorContext)
        {
            // So far, No default validation logic here.
        }

        /// <summary>
        /// Validates $filter within $select or $expand
        /// </summary>
        /// <param name="filterClause">The nested $filter clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateFilter(FilterClause filterClause, SelectExpandValidatorContext validatorContext)
        {
            if (filterClause == null)
            {
                return;
            }

            // It seems the query validator interface should take the AST as input, but now we have the XXXQueryOption.
            // Here's the workaround, we should change it later.
            IFilterQueryValidator filterValidator = validatorContext.Context.GetFilterQueryValidator();

            ODataQueryContext queryContext = new ODataQueryContext
            {
                Request = validatorContext.Context.Request,
                RequestContainer = validatorContext.Context.RequestContainer,
                Model = validatorContext.Context.Model,
                TargetProperty = validatorContext.Property,
                TargetStructuredType = validatorContext.StructuredType
            };

            FilterQueryOption filterQueryOption = new FilterQueryOption(queryContext, filterClause);

            filterValidator.Validate(filterQueryOption, validatorContext.ValidationSettings);
        }

        /// <summary>
        /// Validates $orderby within $select or $expand
        /// </summary>
        /// <param name="orderByClause">The nested $orderby clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateOrderby(OrderByClause orderByClause, SelectExpandValidatorContext validatorContext)
        {
            if (orderByClause != null)
            {
                // TODO: OrderByModelLimitationsValidator is used already. but we should use IOrderbyQueryValidator to validate.
                // Should change it later.
                OrderByModelLimitationsValidator orderByQueryValidator =
                   new OrderByModelLimitationsValidator(validatorContext.Context, validatorContext.Context.DefaultQuerySettings.EnableOrderBy);

                orderByQueryValidator.TryValidate(validatorContext.Property, validatorContext.StructuredType, orderByClause, false);
            }
        }

        /// <summary>
        /// Validates $top within $select or $expand
        /// </summary>
        /// <param name="topOption">The nested $top clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateTop(long? topOption, SelectExpandValidatorContext validatorContext)
        {
            if (topOption != null)
            {
                Contract.Assert(topOption.Value <= Int32.MaxValue);

                IEdmModel edmModel = validatorContext.Context.Model;
                IEdmProperty property = validatorContext.Property;
                IEdmStructuredType structuredType = validatorContext.StructuredType;
                DefaultQuerySettings settings = validatorContext.Context.DefaultQuerySettings;

                int maxTop;
                if (EdmHelpers.IsTopLimitExceeded(property, structuredType, edmModel, (int)topOption.Value, settings, out maxTop))
                {
                    throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop, AllowedQueryOptions.Top, topOption.Value));
                }
            }
        }

        /// <summary>
        /// Validates $skip within $select or $expand
        /// </summary>
        /// <param name="skipOption">The nested $skip clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateSkip(long? skipOption, SelectExpandValidatorContext validatorContext)
        {
            // Nothing here.
        }

        /// <summary>
        /// Validates $count within $select or $expand
        /// </summary>
        /// <param name="countOption">The nested $count clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateCount(bool? countOption, SelectExpandValidatorContext validatorContext)
        {
            if (countOption != null && countOption.Value)
            {
                IEdmModel edmModel = validatorContext.Context.Model;
                IEdmProperty property = validatorContext.Property;
                IEdmStructuredType structuredType = validatorContext.StructuredType;
                DefaultQuerySettings settings = validatorContext.Context.DefaultQuerySettings;

                if (EdmHelpers.IsNotCountable(property, structuredType, edmModel, settings.EnableCount))
                {
                    throw new ODataException(Error.Format(SRResources.NotCountablePropertyUsedForCount, property.Name));
                }
            }
        }

        /// <summary>
        /// Validates $levels within $expand
        /// </summary>
        /// <param name="countOption">The nested $levels clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateLevels(LevelsClause levelsClause, SelectExpandValidatorContext validatorContext)
        {
            if (levelsClause == null)
            {
                return;
            }

            IEdmModel edmModel = validatorContext.Context.Model;
            int depth = validatorContext.ValidationSettings.MaxExpansionDepth;
            IEdmProperty property = validatorContext.Property;
            //Contract.Assert(property != null); // $levels only on navigation property ?
            int currentDepth = validatorContext.CurrentDepth;

            ExpandConfiguration expandConfiguration;
            bool isExpandable = EdmHelpers.IsExpandable(property.Name, property, validatorContext.StructuredType, edmModel, out expandConfiguration);
            if (isExpandable)
            {
                int maxDepth = expandConfiguration.MaxDepth;
                if (maxDepth > 0 && maxDepth < depth)
                {
                    depth = maxDepth;
                }

                if ((depth == 0 && levelsClause.IsMaxLevel) || (depth < levelsClause.Level))
                {
                    throw new ODataException(Error.Format(SRResources.MaxExpandDepthExceeded, currentDepth + depth, "MaxExpansionDepth"));
                }
            }
            else
            {
                if (!validatorContext.Context.DefaultQuerySettings.EnableExpand ||
                    (expandConfiguration != null && expandConfiguration.ExpandType == SelectExpandType.Disabled))
                {
                    throw new ODataException(Error.Format(SRResources.NotExpandablePropertyUsedInExpand, property.Name));
                }
            }
        }

        /// <summary>
        /// Validates $search within $select or $expand
        /// </summary>
        /// <param name="searchClause">The nested $search clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateSearch(SearchClause searchClause, SelectExpandValidatorContext validatorContext)
        {
            // Add logics here to verify nested $search. So far, No default validation logic here.
        }

        /// <summary>
        /// Validates $compute within $expand
        /// </summary>
        /// <param name="computeClause">The nested $compute clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateCompute(ComputeClause computeClause, SelectExpandValidatorContext validatorContext)
        {
            // Add logics here to verify nested $compute. So far, No default validation logic here.
        }

        /// <summary>
        /// Validates $apply within $expand
        /// </summary>
        /// <param name="applyClause">The nested $apply clause.</param>
        /// <param name="validatorContext">The validator context.</param>
        protected virtual void ValidateApply(ApplyClause applyClause, SelectExpandValidatorContext validatorContext)
        {
            // Add logics here to verify nested $apply. So far, No default validation logic here.
        }

        private static void ValidateDepth(SelectExpandClause selectExpand, int maxDepth)
        {
            // do a DFS to see if there is any node that is too deep.
            Stack<Tuple<int, SelectExpandClause>> nodesToVisit = new Stack<Tuple<int, SelectExpandClause>>();
            nodesToVisit.Push(Tuple.Create(0, selectExpand));
            while (nodesToVisit.Count > 0)
            {
                Tuple<int, SelectExpandClause> tuple = nodesToVisit.Pop();
                int currentDepth = tuple.Item1;
                SelectExpandClause currentNode = tuple.Item2;

                ExpandedNavigationSelectItem[] expandItems = currentNode.SelectedItems.OfType<ExpandedNavigationSelectItem>().ToArray();

                if (expandItems.Length > 0 &&
                    ((currentDepth == maxDepth &&
                    expandItems.Any(expandItem =>
                        expandItem.LevelsOption == null ||
                        expandItem.LevelsOption.IsMaxLevel ||
                        expandItem.LevelsOption.Level != 0)) ||
                    expandItems.Any(expandItem =>
                        expandItem.LevelsOption != null &&
                        !expandItem.LevelsOption.IsMaxLevel &&
                        (expandItem.LevelsOption.Level > Int32.MaxValue ||
                        expandItem.LevelsOption.Level + currentDepth > maxDepth))))
                {
                    throw new ODataException(
                        Error.Format(SRResources.MaxExpandDepthExceeded, maxDepth, "MaxExpansionDepth"));
                }

                foreach (ExpandedNavigationSelectItem expandItem in expandItems)
                {
                    int depth = currentDepth + 1;

                    if (expandItem.LevelsOption != null && !expandItem.LevelsOption.IsMaxLevel)
                    {
                        // Add the value of $levels for next depth.
                        depth = depth + (int)expandItem.LevelsOption.Level - 1;
                    }

                    nodesToVisit.Push(Tuple.Create(depth, expandItem.SelectAndExpand));
                }
            }
        }

        private static IEdmStructuredType GetStructuredType(IEdmTypeReference typeRef)
        {
            if (typeRef == null)
            {
                return null;
            }

            EdmTypeKind kind = typeRef.TypeKind();
            if (kind == EdmTypeKind.Collection)
            {
                return GetStructuredType(typeRef.AsCollection().ElementType());
            }

            if (kind == EdmTypeKind.Entity)
            {
                return ((IEdmEntityTypeReference)typeRef).StructuredDefinition();
            }

            if (kind == EdmTypeKind.Complex)
            {
                return ((IEdmComplexTypeReference)typeRef).StructuredDefinition();
            }

            return null;
        }
    }
}
