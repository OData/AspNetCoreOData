//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query.Container;
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
            if (context == null)
            {
                return null;
            }

            if (pageSize <= 0)
            {
                return null;
            }

            Func<object, string> skipTokenGenerator = null;
            IList<OrderByNode> orderByNodes = null;
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
                        orderByNodes = OrderByNode.CreateCollection(expandedItem.OrderByOption);
                    }

                    skipTokenGenerator = (obj) =>
                    {
                        return GenerateSkipTokenValue(obj, model, orderByNodes, context.TimeZone);
                    };

                    return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator);
                }

                if (context.QueryOptions != null && context.QueryOptions.OrderBy != null)
                {
                    orderByNodes = context.QueryOptions.OrderBy.OrderByNodes;
                }

                skipTokenGenerator = (obj) =>
                {
                    return GenerateSkipTokenValue(obj, model, orderByNodes, context.TimeZone);
                };
            }

            return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator);
        }

        /// <summary>
        /// Generates a string to be used as the skip token value within the next link.
        /// </summary>
        /// <param name="lastMember"> Object based on which SkipToken value will be generated.</param>
        /// <param name="model">The edm model.</param>
        /// <param name="orderByNodes">List of orderByNodes used to generate the skiptoken value.</param>
        /// <param name="timeZoneInfo">The timezone info.</param>
        /// <returns>Value for the skiptoken to be used in the next link.</returns>
        internal static string GenerateSkipTokenValue(Object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes, TimeZoneInfo timeZoneInfo = null)
        {
            if (lastMember == null)
            {
                return string.Empty;
            }

            IEnumerable<IEdmProperty> propertiesForSkipToken = GetPropertiesForSkipToken(lastMember, model, orderByNodes);
            StringBuilder skipTokenBuilder = new StringBuilder(String.Empty);
            if (propertiesForSkipToken == null)
            {
                return skipTokenBuilder.ToString();
            }

            int count = 0;
            string uriLiteral;
            object value;
            int lastIndex = propertiesForSkipToken.Count() - 1;
            IEdmStructuredObject obj = lastMember as IEdmStructuredObject;

            foreach (IEdmProperty edmProperty in propertiesForSkipToken)
            {
                bool islast = count == lastIndex;
                string clrPropertyName = model.GetClrPropertyName(edmProperty);
                if (obj != null)
                {
                    obj.TryGetPropertyValue(clrPropertyName, out value);
                }
                else
                {
                    value = lastMember.GetType().GetProperty(clrPropertyName).GetValue(lastMember);
                }

                if (value == null)
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401);
                }
                else if (edmProperty.Type.IsEnum())
                {
                    ODataEnumValue enumValue = new ODataEnumValue(value.ToString(), value.GetType().FullName);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V401, model);
                }
                else if (edmProperty.Type.IsDateTimeOffset() && value is DateTime)
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

                skipTokenBuilder
                    .Append(edmProperty.Name)
                    .Append(propertyDelimiter)
                    .Append(encodedUriLiteral)
                    .Append(islast ? String.Empty : CommaDelimiter.ToString(CultureInfo.CurrentCulture));
                count++;
            }

            return skipTokenBuilder.ToString();
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

            IList<OrderByNode> orderByNodes = null;

            if (queryOptions != null)
            {
                OrderByQueryOption orderBy = queryOptions.GenerateStableOrder();
                if (orderBy != null)
                {
                    orderByNodes = orderBy.OrderByNodes;
                }
            }

            return ApplyToCore(query, querySettings, orderByNodes, skipTokenQueryOption.Context, skipTokenQueryOption.RawValue);
        }

        /// <summary>
        /// Core logic for applying the query option to the IQueryable. 
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">Query setting used for validating the query option.</param>
        /// <param name="orderByNodes">OrderBy information required to correctly apply the query option for default implementation.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="skipTokenRawValue">The raw string value of the skiptoken query parameter.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable.")]
        private static IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings, IList<OrderByNode> orderByNodes, ODataQueryContext context, string skipTokenRawValue)
        {
            Contract.Assert(query != null);
            Contract.Assert(context.ElementClrType != null);

            IDictionary<string, OrderByDirection> directionMap;
            if (orderByNodes != null)
            {
                directionMap =
                    orderByNodes.OfType<OrderByPropertyNode>().ToDictionary(node => node.Property.Name, node => node.Direction);
            }
            else
            {
                directionMap = new Dictionary<string, OrderByDirection>();
            }

            IDictionary<string, (object PropertyValue, Type PropertyType)> propertyValuePairs = PopulatePropertyValuePairs(skipTokenRawValue, context);

            if (propertyValuePairs.Count == 0)
            {
                throw Error.InvalidOperation(SRResources.SkipTokenProcessingError);
            }

            bool parameterizeConstant = querySettings.EnableConstantParameterization;
            ParameterExpression param = Expression.Parameter(context.ElementClrType);
            Expression where = null;
            /* We will create a where lambda of the following form -
             * Where (Prop1>Value1)
             * OR (Prop1=Value1 AND Prop2>Value2)
             * OR (Prop1=Value1 AND Prop2=Value2 AND Prop3>Value3)
             * and so on...
             * Adding the first true to simplify implementation.
             */
            Expression lastEquality = null;
            bool firstProperty = true;

            foreach (KeyValuePair<string, (object PropertyValue, Type PropertyType)> item in propertyValuePairs)
            {
                string key = item.Key;
                MemberExpression property = Expression.Property(param, key);

                object value = item.Value.PropertyValue;

                Type propertyType = item.Value.PropertyType ?? value.GetType();
                bool propertyIsNullable = property.Type.IsNullable();

                Expression compare = null;
                if (value is ODataEnumValue enumValue)
                {
                    value = enumValue.Value;
                    propertyType = value.GetType();
                }
                else if (value is ODataNullValue)
                {
                    value = null;
                    propertyType = property.Type;
                }

                Expression constant = parameterizeConstant ? LinqParameterContainer.Parameterize(propertyType, value) : Expression.Constant(value);

                if (directionMap.ContainsKey(key) && directionMap[key] == OrderByDirection.Descending)
                {
                    // Prop < Value
                    compare = ExpressionBinderHelper.CreateBinaryExpression(
                        binaryOperator: BinaryOperatorKind.LessThan,
                        left: property,
                        right: constant,
                        liftToNull: !propertyIsNullable,
                        querySettings: querySettings);

                    if (propertyIsNullable && value != null)
                    {
                        // Prop == null

                        // We only do this when value is NOT null since
                        // ((Prop1 < null) OR (Prop1 == null)) OR ((Prop1 == null) AND (Prop2 > Value2))
                        // doesn't make logical sense
                        Expression condition = ExpressionBinderHelper.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.Equal,
                            left: property,
                            right: parameterizeConstant ? LinqParameterContainer.Parameterize(property.Type, null) : Expression.Constant(null),
                            liftToNull: false,
                            querySettings: querySettings);

                        // (Prop < Value) OR (Prop == null)
                        compare = Expression.OrElse(compare, condition);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        // Prop != null

                        // We are aiming for the following expression
                        // when value is null in the ascending order scenario:
                        // (Prop1 != null) OR ((Prop1 == null) AND (Prop2 > Value2)) ...
                        compare = ExpressionBinderHelper.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.NotEqual,
                            left: property,
                            right: constant,
                            liftToNull: false,
                            querySettings: querySettings);
                    }
                    else
                    {
                        // Prop > Value

                        // We are aiming for the following expression
                        // when value is NOT null in the ascending order scenario:
                        // (Prop1 > Value1) OR ((Prop1 == Value1) AND (Prop2 > Value2)) ...
                        compare = ExpressionBinderHelper.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.GreaterThan,
                            left: property,
                            right: constant,
                            liftToNull: !propertyIsNullable,
                            querySettings: querySettings);
                    }
                }

                if (firstProperty)
                {
                    lastEquality = ExpressionBinderHelper.CreateBinaryExpression(
                        binaryOperator: BinaryOperatorKind.Equal,
                        left: property,
                        right: constant,
                        liftToNull: !propertyIsNullable,
                        querySettings: querySettings);
                    where = compare;
                    firstProperty = false;
                }
                else
                {
                    Expression condition = Expression.AndAlso(lastEquality, compare);
                    where = Expression.OrElse(where, condition);
                    lastEquality = Expression.AndAlso(
                        lastEquality,
                        ExpressionBinderHelper.CreateBinaryExpression(
                            binaryOperator: BinaryOperatorKind.Equal,
                            left: property,
                            right: constant,
                            liftToNull: !propertyIsNullable,
                            querySettings: querySettings));
                }
            }

            Expression whereLambda = Expression.Lambda(where, param);
            return ExpressionHelpers.Where(query, whereLambda, query.ElementType);
        }

        /// <summary>
        /// Generates a dictionary with property name and property values specified in the skiptoken value.
        /// </summary>
        /// <param name="value">The skiptoken string value.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <returns>Dictionary with property name and property value in the skiptoken value.</returns>
        internal static IDictionary<string, (object PropertyValue, Type PropertyType)> PopulatePropertyValuePairs(string value, ODataQueryContext context)
        {
            Contract.Assert(context != null);

            IDictionary<string, (object PropertyValue, Type PropertyType)> propertyValuePairs = new Dictionary<string, (object PropertyValue, Type PropertyType)>();
            IList<string> keyValuesPairs = ParseValue(value, CommaDelimiter);

            IEdmStructuredType type = context.ElementType as IEdmStructuredType;
            Debug.Assert(type != null);

            foreach (string pair in keyValuesPairs)
            {
                string[] pieces = pair.Split(new char[] { propertyDelimiter }, 2);
                if (pieces.Length > 1 && !String.IsNullOrWhiteSpace(pieces[0]))
                {
                    object propValue = null;

                    IEdmTypeReference propertyType = null;
                    IEdmProperty property = type.FindProperty(pieces[0]);
                    Type propertyClrType = null;
                    if (property != null)
                    {
                        propertyType = property.Type;
                        propertyClrType = context.Model.GetClrType(propertyType);
                    }

                    propValue = ODataUriUtils.ConvertFromUriLiteral(pieces[1], ODataVersion.V401, context.Model, propertyType);
                    propertyValuePairs.Add(pieces[0], (propValue, propertyClrType));
                }
                else
                {
                    throw new ODataException(Error.Format(SRResources.SkipTokenParseError, value));
                }
            }

            return propertyValuePairs;
        }

        /// <summary>
        /// Returns the list of properties that should be used for generating the skiptoken value. 
        /// </summary>
        /// <param name="lastMember">The last record that will be returned in the response.</param>
        /// <param name="model">IEdmModel</param>
        /// <param name="orderByNodes">OrderBy nodes in the original request.</param>
        /// <returns>List of properties that should be used for generating the skiptoken value.</returns>
        internal static IEnumerable<IEdmProperty> GetPropertiesForSkipToken(object lastMember, IEdmModel model, IList<OrderByNode> orderByNodes)
        {
            IEdmType edmType = GetTypeFromObject(lastMember, model);
            IEdmEntityType entity = edmType as IEdmEntityType;
            if (entity == null)
            {
                return null;
            }

            IEnumerable<IEdmProperty> key = entity.Key();
            if (orderByNodes != null)
            {
                if (orderByNodes.OfType<OrderByOpenPropertyNode>().Any())
                {
                    //SkipToken will not support ordering on dynamic properties
                    return null;
                }

                IList<IEdmProperty> orderByProps = orderByNodes.OfType<OrderByPropertyNode>().Select(p => p.Property).ToList();
                foreach (IEdmProperty subKey in key)
                {
                    if (!orderByProps.Contains(subKey))
                    {
                        orderByProps.Add(subKey);
                    }
                }

                return orderByProps.AsEnumerable();
            }

            return key;
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
