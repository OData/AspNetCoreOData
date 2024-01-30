//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Default implementation of SkipTokenHandler for the service.
    /// </summary>
    public class DefaultSkipTokenHandler : SkipTokenHandler
    {
        private const char CommaDelimiter = ',';
        private static char propertyDelimiter = '-';
        internal static DefaultSkipTokenHandler Instance = new DefaultSkipTokenHandler();

        /// <summary>
        /// Returns the URI for NextPageLink
        /// </summary>
        /// <param name="baseUri">BaseUri for nextlink. It should be request URI for top level resource and navigation link for nested resource.</param>
        /// <param name="pageSize">Maximum number of records in the set of partial results for a resource.</param>
        /// <param name="instance">Instance based on which SkipToken value will be generated.</param>
        /// <param name="context">Serializer context</param>
        /// <returns>Returns the URI for NextPageLink. If a null object is passed for the instance, resorts to the default paging mechanism of using $skip and $top.</returns>
        public override Uri GenerateNextPageLink(Uri baseUri, int pageSize, Object instance, ODataSerializerContext context)
        {
            if (context == null || pageSize <= 0)
            {
                return null;
            }

            Func<object, string> skipTokenGenerator = null;
            OrderByClause orderByClause = null;
            ExpandedReferenceSelectItem expandedItem = context.CurrentSelectItem as ExpandedReferenceSelectItem;
            IEdmModel model = context.Model;

            DefaultQueryConfigurations queryConfigs = context.QueryContext.DefaultQueryConfigurations;
            if (queryConfigs.EnableSkipToken)
            {
                if (expandedItem != null)
                {
                    // Handle Delta resource; currently not value based.
                    if (DeltaHelper.IsDeltaOfT(context.ExpandedResource.GetType()))
                    {
                        return GetNextPageHelper.GetNextPageLink(baseUri, pageSize);
                    }

                    if (expandedItem.OrderByOption != null)
                    {
                        orderByClause = expandedItem.OrderByOption;
                    }

                    skipTokenGenerator = (obj) =>
                    {
                        return GenerateSkipTokenValue(obj, model, orderByClause, context);
                    };

                    return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator);
                }

                if (context.QueryOptions != null && context.QueryOptions.OrderBy != null)
                {
                    orderByClause = context.QueryOptions.OrderBy.OrderByClause;
                }

                skipTokenGenerator = (obj) =>
                {
                    return GenerateSkipTokenValue(obj, model, orderByClause, context);
                };
            }

            return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator);
        }

        /// <summary>
        /// Generates a string to be used as the skip token value within the next link.
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="clause">The orderby clause to generate the skiptoken value.</param>
        /// <param name="context">The serializer context.</param>
        /// <returns>Value for the skiptoken to be used in the next link.</returns>
        internal static string GenerateSkipTokenValue(object lastMember, IEdmModel model, OrderByClause clause, ODataSerializerContext context = null)
        {
            if (lastMember == null)
            {
                return string.Empty;
            }

            IList<OrderByClause> clauses = GetOrderByClauses(lastMember, model, clause);

            TimeZoneInfo timeZoneInfo = context?.TimeZone;
            IList<KeyValuePair<string, object>> values = GetPropertyValues(lastMember, model, clauses, context);
            if (values == null || values.Count == 0 || values.Count != clauses.Count)
            {
                return null;
            }

            StringBuilder skipTokenBuilder = new StringBuilder();
            int index = 0;
            foreach (OrderByClause orderBy in clauses)
            {
                object value = values[index].Value;
                string name = values[index].Key;
                IEdmTypeReference typeReference = orderBy.Expression.TypeReference;

                string uriLiteral;
                if (value == null)
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401);
                }
                else if (typeReference != null && typeReference.IsEnum())
                {
                    ODataEnumValue enumValue = new ODataEnumValue(value.ToString(), value.GetType().FullName);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V401, model);
                }
                else if (typeReference != null && typeReference.IsDateTimeOffset() && value is DateTime)
                {
                    var dateTime = (DateTime)value;
                    var dateTimeOffsetValue = TimeZoneInfoHelper.ConvertToDateTimeOffset(dateTime, timeZoneInfo);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(dateTimeOffsetValue, ODataVersion.V401, model);
                }
                else
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401, model);
                }

                var encodedUriLiteral = WebUtility.UrlEncode(uriLiteral);

                // By design: We should only include the value?
                // For back-compatibility, let's keep the key-value pair for top-level property orderby
                // For other advance orderby clause, only include the value.
                if (!string.IsNullOrEmpty(name))
                {
                    skipTokenBuilder.Append(name).Append(propertyDelimiter);
                }
                skipTokenBuilder.Append(encodedUriLiteral);

                index++;
                if (index != clauses.Count)
                {
                    skipTokenBuilder.Append(',');
                }
            }

            return skipTokenBuilder.ToString();
        }

        private static IList<KeyValuePair<string, object>> GetPropertyValues(object source, IEdmModel model, IList<OrderByClause> clauses,
            ODataSerializerContext context)
        {
            if (source == null || clauses == null || clauses.Count == 0)
            {
                return null;
            }

            // When page size enabled, we will generate the next link no matter what the following scenarios:
            // 1) Without $orderby, $select, $expand, for example: ~/customers
            // 2) Without $select =..., for example: ~/customers?$orderby=....
            // 3) Without $orderby=..., for example: ~/customers?$select=....
            // 4) With $orderby, $select..., for example: ~/customers?$select=....&$orderby=.....

            // If the request contains $select, the SelectExpandBinder at least contains the 'key(s)', since they are auto-selected
            // If the request doesn't contain $select, the below codes can return all properties
            IEdmStructuredObject structuredObj = source as IEdmStructuredObject;
            if (structuredObj == null)
            {
                Type sourceType = source.GetType();
                QueryBinderContext binderContext = new QueryBinderContext(model, new ODataQuerySettings(), sourceType);
                binderContext.AddComputedProperties(context?.QueryOptions?.Compute?.ComputeClause?.ComputedItems);
                binderContext.OrderByClauses = clauses;
                ISelectExpandBinder selectExpandBinder = context != null && context.QueryContext != null ?
                    context.QueryContext.GetSelectExpandBinder() :
                    new SelectExpandBinder(new FilterBinder(), new OrderByBinder());

                SelectExpandClause selectAll = new SelectExpandClause(null, true);
                structuredObj = selectExpandBinder.ApplyBind(source, selectAll, binderContext) as IEdmStructuredObject;
            }

            if (structuredObj == null)
            {
                return null;
            }

            structuredObj.TryGetPropertyValue(OrderByClauseHelpers.OrderByGlobalNameKey, out object orderByNameObject);

            IList<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();
            string[] orderByNames = orderByNameObject == null ? null : (orderByNameObject as string).Split(",");
            int index = 0;
            object value;
            foreach (OrderByClause clause in clauses)
            {
                if (clause.IsTopLevelSingleProperty(out _, out string name))
                {
                    // Let's first retrieve the value using property name, for example: "Id"
                    if (structuredObj.TryGetPropertyValue(name, out value))
                    {
                        values.Add(new KeyValuePair<string, object>(name, value));
                    }
                    else
                    {
                        value = GetValue(structuredObj, orderByNames, index++, out string _);
                        values.Add(new KeyValuePair<string, object>(name, value));
                    }
                }
                else
                {
                    value = GetValue(structuredObj, orderByNames, index++, out string _);
                    values.Add(new KeyValuePair<string, object>(string.Empty, value));
                }
            }

            return values;
        }

        private static object GetValue(IEdmStructuredObject source, string[] orderByNames, int index, out string name)
        {
            name = string.Empty;
            if (orderByNames != null && index < orderByNames.Length)
            {
                if (source.TryGetPropertyValue(orderByNames[index], out object value))
                {
                    name = orderByNames[index];
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="skipTokenQueryOption">The skiptoken query option which needs to be applied to this query option.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="queryOptions">Information about the other query options.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to, could be null.</returns>
        public override IQueryable<T> ApplyTo<T>(IQueryable<T> query, SkipTokenQueryOption skipTokenQueryOption,
            ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            return ApplyToImplementation(query, skipTokenQueryOption, querySettings, queryOptions) as IQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>, not null.</param>
        /// <param name="skipTokenQueryOption">The skiptoken query option which needs to be applied to this query option, not null.</param>
        /// <param name="querySettings">The query settings to use while applying this query option, not null.</param>
        /// <param name="queryOptions">Information about the other query options, could be null.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skiptoken query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query, SkipTokenQueryOption skipTokenQueryOption,
            ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            return ApplyToImplementation(query, skipTokenQueryOption, querySettings, queryOptions);
        }

        private static IQueryable ApplyToImplementation(IQueryable query, SkipTokenQueryOption skipTokenQueryOption,
            ODataQuerySettings querySettings, ODataQueryOptions queryOptions)
        {
            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (skipTokenQueryOption == null)
            {
                throw Error.ArgumentNull(nameof(skipTokenQueryOption));
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull(nameof(querySettings));
            }

            ODataQueryContext context = skipTokenQueryOption.Context;
            if (context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            return ApplyToCore(query, querySettings, queryOptions, skipTokenQueryOption.RawValue);
        }

        private static IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings,
             ODataQueryOptions queryOptions, string skipTokenRawValue)
        {
            OrderByQueryOption orderByOption = queryOptions?.OrderBy;
            if (orderByOption == null)
            {
                return query;
            }

            // It's better to visit the nodes of OrderByClause reclusively to get the orderby raw value.
            // Since we don't have such methods, let's simply split the request raw value.
            IList<string> orderBys = orderByOption.RawValue.Split(',');
            IList<OrderByClause> orderByClauses = orderByOption.OrderByClause.ToList();
            IList<string> tokenValueParis = PopulatePropertyValuePairs(skipTokenRawValue);

            if (orderBys.Count != orderByClauses.Count && orderByClauses.Count != tokenValueParis.Count)
            {
                throw Error.InvalidOperation(SRResources.SkipTokenProcessingError);
            }

            string where = string.Empty;
            string lastEquality = null;
            bool isFirst = true;
            Contract.Assert(orderBys.Count == orderByClauses.Count);
            for (int i = 0; i < orderBys.Count; ++i)
            {
                OrderByClause orderBy = orderByClauses[i];
                string orderByRaw = orderBys[i];
                string orderByValue = tokenValueParis[i];
                bool isNullValue = string.Equals(orderByValue, "null", StringComparison.OrdinalIgnoreCase);

                string compare;
                if (orderBy.Direction == OrderByDirection.Descending)
                {
                    // In descending ordering, the 'null' value goes later.
                    orderByRaw = orderByRaw.RemoveDesc();

                    if (isNullValue)
                    {
                        compare = "false"; // Dummy compare, always false
                    }
                    else
                    {
                        // (prop < value) OR (prop == null)
                        compare = $"(({orderByRaw} lt {orderByValue}) or ({orderByRaw} eq null))";
                    }
                }
                else
                {
                    if (isNullValue)
                    {
                        // We are aiming for the following expression
                        // when value is null in the ascending order scenario:
                        // (Prop1 != null) OR ((Prop1 == null) AND (Prop2 > Value2)) ...
                        compare = $"({orderByRaw} ne null)";
                    }
                    else
                    {
                        // We are aiming for the following expression
                        // when value is NOT null in the ascending order scenario:
                        // (Prop1 > Value1) OR ((Prop1 == Value1) AND (Prop2 > Value2)) ...
                        compare = $"({orderByRaw} gt {orderByValue})";
                    }
                }

                if (isFirst)
                {
                    lastEquality = $"({orderByRaw} eq {orderByValue})";
                    where = compare;
                    isFirst = false;
                }
                else
                {
                    string condition = $"({lastEquality} and {compare})";
                    where = $"({where} or {condition})";
                    lastEquality = $"({lastEquality} and ({orderByRaw} eq {orderByValue}))";
                }
            }

            FilterQueryOption filter = new FilterQueryOption(where, queryOptions.Context);
            filter.Compute = queryOptions.Compute;

            return filter.ApplyTo(query, querySettings);
        }

        internal static string[] PopulatePropertyValuePairs(string value)
        {
            IList<string> keyValuesPairs = ParseValue(value, CommaDelimiter);
            string[] items = new string[keyValuesPairs.Count];
            int index = 0;
            foreach (string pair in keyValuesPairs)
            {
                // Since the original design is to use '-' to split the "propertyName" and "PropertyValue".
                // Now, the keyValuePairs should only contain the property value and the property value itself could contain '-'.
                // So far, the possible problem is for 'DateTimeOffset', 'Date', 'Guid' or a negative value (-42).
                // Actually, I think it's better to use '=' to split the name and value.
                // For back-compatibility, let's keep use '-'
                string trimmedPair = pair.Trim();
                if (trimmedPair.StartsWith('-'))
                {
                    items[index] = trimmedPair;
                    ++index;
                    continue;
                }

                if (DateTimeOffset.TryParse(trimmedPair, out _) ||
                    Date.TryParse(trimmedPair, out _) ||
                    Guid.TryParse(trimmedPair, out _))
                {
                    items[index] = trimmedPair;
                    ++index;
                    continue;
                }

                // It should work for: abc--42
                IList<string> pieces = trimmedPair.Split(new char[] { propertyDelimiter }, 2);
                if (pieces.Count == 1)
                {
                    // without the property name, only contains the value
                    items[index] = pieces[0];
                }
                else if (pieces.Count == 2 && !string.IsNullOrWhiteSpace(pieces[0]))
                {
                    // without the property name, only contains the value
                    items[index] = pieces[1];
                }
                else
                {
                    throw new ODataException(Error.Format(SRResources.SkipTokenParseError, value));
                }

                ++index;
            }

            return items;
        }

        /// <summary>
        /// Get all orderby clause and append the keys if apply
        /// </summary>
        /// <param name="lastMember">The last object.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="clause">The original orderby clause.</param>
        /// <returns>The orderby clauses, since we don't need the 'ThenBy' for each clause, we didn't need to change it.</returns>
        internal static IList<OrderByClause> GetOrderByClauses(object lastMember, IEdmModel model, OrderByClause clause)
        {
            IEdmType edmType = GetTypeFromObject(lastMember, model);
            IEdmEntityType entity = edmType as IEdmEntityType;
            if (entity == null)
            {
                return null;
            }

            IList<OrderByClause> orderByClauses = new List<OrderByClause>();
            ISet<IEdmProperty> properties = new HashSet<IEdmProperty>();
            while (clause != null)
            {
                // Be noted, the 'ThenBy' doesn't reset to null. It doesn't matter since we don't use it.
                orderByClauses.Add(clause);

                if (clause.IsTopLevelSingleProperty(out IEdmProperty property, out _))
                {
                    properties.Add(property);
                }

                clause = clause.ThenBy;
            }

            ResourceRangeVariable rangeVar = new ResourceRangeVariable("$it", new EdmEntityTypeReference(entity, true), navigationSource: null);
            ResourceRangeVariableReferenceNode rangeNode = new ResourceRangeVariableReferenceNode("$it", rangeVar);
            IEnumerable<IEdmProperty> key = entity.Key();
            foreach (IEdmProperty subKey in key)
            {
                if (!properties.Contains(subKey))
                {
                    SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(rangeNode, subKey);
                    OrderByClause keyOrderBy = new OrderByClause(null, node, OrderByDirection.Ascending, rangeVar);
                    orderByClauses.Add(keyOrderBy);
                }
            }

            return orderByClauses;
        }

        /// <summary>
        /// Gets the EdmType from the Instance which may be a select expand wrapper.
        /// </summary>
        /// <param name="value">Instance for which the edmType needs to be computed.</param>
        /// <param name="model">IEdmModel</param>
        /// <returns>The EdmType of the underlying instance.</returns>
        internal static IEdmType GetTypeFromObject(object value, IEdmModel model)
        {
            SelectExpandWrapper selectExpand = value as SelectExpandWrapper;
            if (selectExpand != null)
            {
                IEdmTypeReference typeReference = selectExpand.GetEdmType();
                return typeReference.Definition;
            }

            Type clrType = value.GetType();
            return model.GetEdmTypeReference(clrType)?.Definition;
        }

        private static IList<string> ParseValue(string value, char delim)
        {
            IList<string> results = new List<string>();
            StringBuilder escapedStringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\'' || value[i] == '"')
                {
                    escapedStringBuilder.Append(value[i]);
                    char openingQuoteChar = value[i];
                    i++;
                    while (i < value.Length && value[i] != openingQuoteChar)
                    {
                        escapedStringBuilder.Append(value[i++]);
                    }

                    if (i != value.Length)
                    {
                        escapedStringBuilder.Append(value[i]);
                    }
                }
                else if (value[i] == delim)
                {
                    results.Add(escapedStringBuilder.ToString());
                    escapedStringBuilder.Clear();
                }
                else
                {
                    escapedStringBuilder.Append(value[i]);
                }
            }

            string lastPair = escapedStringBuilder.ToString();
            if (!String.IsNullOrWhiteSpace(lastPair))
            {
                results.Add(lastPair);
            }

            return results;
        }
    }
}
