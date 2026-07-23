//-----------------------------------------------------------------------------
// <copyright file="DefaultSkipTokenHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
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
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// Default implementation of SkipTokenHandler for the service.
/// </summary>
public class DefaultSkipTokenHandler : SkipTokenHandler
{
    private const char CommaDelimiter = ',';
    private static char propertyDelimiter = '-';
    internal static DefaultSkipTokenHandler Instance = new DefaultSkipTokenHandler();

    // Shared null constant node. ConstantNode is an immutable AST value object so a single
    // instance is safe to reuse across any number of filter expressions built per request.
    private static readonly ConstantNode NullConstantNode = new ConstantNode(null, "null");

    // Shared false constant node — used for "nothing comes after null in descending order".
    private static readonly ConstantNode FalseConstantNode = new ConstantNode(false, "false", EdmCoreModel.Instance.GetBoolean(false));

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
        bool isNoDollarQueryEnable = context.Request.IsNoDollarQueryEnable();
        if (queryConfigs.EnableSkipToken)
        {
            if (expandedItem != null)
            {
                // Handle Delta resource; currently not value based.
                if (DeltaHelper.IsDeltaOfT(context.ExpandedResource.GetType()))
                {
                    return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, null, null, isNoDollarQueryEnable);
                }

                if (expandedItem.OrderByOption != null)
                {
                    orderByClause = expandedItem.OrderByOption;
                }

                skipTokenGenerator = (obj) =>
                {
                    return GenerateSkipTokenValue(obj, model, orderByClause, context);
                };

                return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator, isNoDollarQueryEnable);
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

        return GetNextPageHelper.GetNextPageLink(baseUri, pageSize, instance, skipTokenGenerator, isNoDollarQueryEnable);
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

        List<OrderByClause> clauses = GetOrderByClauses(lastMember, model, clause);

        TimeZoneInfo timeZoneInfo = context?.TimeZone;
        List<KeyValuePair<string, object>> values = GetPropertyValues(lastMember, model, clauses, context);
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
                IEdmEnumType enumDefinition = typeReference.AsEnum().EnumDefinition();
                string edmTypeName = enumDefinition.FullName();

                // Prefer the EDM member name (which reflects any [EnumMember(Value="...")] alias)
                // over the CLR name returned by value.ToString(). Without this, an aliased enum
                // like [EnumMember(Value="Oro")] Gold would generate a token containing "Gold",
                // which BuildTypedConstantNode would reject because the EDM member is named "Oro".
                string memberName = value.ToString();
                ClrEnumMemberAnnotation clrEnumMemberAnnotation = model.GetClrEnumMemberAnnotation(enumDefinition);
                if (clrEnumMemberAnnotation != null && value is Enum enumVal)
                {
                    IEdmEnumMember edmMember = clrEnumMemberAnnotation.GetEdmEnumMember(enumVal);
                    if (edmMember != null)
                    {
                        memberName = edmMember.Name;
                    }
                }

                ODataEnumValue enumValue = new ODataEnumValue(memberName, edmTypeName);
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

    private static List<KeyValuePair<string, object>> GetPropertyValues(object source, IEdmModel model, List<OrderByClause> clauses,
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

        List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();
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
        OrderByQueryOption orderByOption = queryOptions?.GenerateStableOrder();
        if (orderByOption == null)
        {
            return query;
        }

        IList<OrderByClause> orderByClauses = orderByOption.OrderByClause.ToList();
        IList<string> tokenValues = PopulatePropertyValues(skipTokenRawValue);

        // Reject a skip token whose segment count does not match the stable order clause count.
        if (orderByClauses.Count != tokenValues.Count)
        {
            throw Error.InvalidOperation(SRResources.SkipTokenProcessingError);
        }

        IEdmModel model = queryOptions.Context.Model;

        // Build the skip-token filter entirely as an OData AST.
        // Token values are bound as typed ConstantNodes and combined with BinaryOperatorNodes.
        // This mirrors the approach used by SelectExpandQueryValidator which builds a FilterClause
        // from an already-parsed AST and passes it to the internal FilterQueryOption(context, filterClause)
        // constructor rather than constructing a filter string.
        SingleValueNode filterExpression = BuildSkipTokenFilterExpression(
            orderByClauses, tokenValues, model, skipTokenRawValue);

        FilterClause filterClause = new FilterClause(filterExpression, orderByClauses[0].RangeVariable);
        FilterQueryOption filter = new FilterQueryOption(queryOptions.Context, filterClause);
        filter.Compute = queryOptions.Compute;

        // NOTE: We intentionally skip filter.Validate() here.
        // This synthetic filter is server-generated from the $orderby clause properties
        // and type-validated token values. Running the developer's ODataValidationSettings
        // against it would incorrectly reject properties that appear in $orderby but whose
        // owning entity does not have $filter enabled (a common configuration), which was
        // exactly the pre-existing behaviour of the old string-based approach.

        return filter.ApplyTo(query, querySettings);
    }

    /// <summary>
    /// Builds the skip-token filter expression as an OData AST.
    /// <para>
    /// For a 2-column stable order (Prop1 asc, Prop2 asc) with token values (v1, v2), the
    /// expression is: <c>(Prop1 gt v1) OR ((Prop1 eq v1) AND (Prop2 gt v2))</c>
    /// </para>
    /// <para>
    /// For descending columns the comparison flips:
    /// <c>((Prop1 lt v1) OR (Prop1 eq null)) OR ((Prop1 eq v1) AND ((Prop2 lt v2) OR (Prop2 eq null)))</c>
    /// </para>
    /// </summary>
    private static SingleValueNode BuildSkipTokenFilterExpression(
        IList<OrderByClause> orderByClauses,
        IList<string> tokenValues,
        IEdmModel model,
        string skipTokenRawValue)
    {
        SingleValueNode whereNode = null;
        SingleValueNode previousEqualityNode = null;
        bool isFirst = true;

        for (int i = 0; i < orderByClauses.Count; i++)
        {
            OrderByClause orderBy = orderByClauses[i];
            string rawValue = tokenValues[i];
            bool isNullValue = rawValue.Length == 4 && string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase);

            // Reuse the property-access node from the OrderByClause. All clauses share the
            // same RangeVariable (set as FilterClause.RangeVariable above), so the LINQ binder
            // will correctly map property accesses to the lambda parameter.
            SingleValueNode propNode = orderBy.Expression;

            // Build a typed constant node for the token value.
            SingleValueNode constantNode = isNullValue
                ? NullConstantNode
                : BuildTypedConstantNode(rawValue, orderBy.Expression.TypeReference, model, skipTokenRawValue);

            // Build the "comes after" comparison for this sort column:
            //   asc,  non-null → prop gt value
            //   asc,  null     → prop ne null   (null sorts first; anything non-null is "after")
            //   desc, non-null → (prop lt value) OR (prop eq null)  (null sorts last in desc)
            //   desc, null     → false           (nothing comes after null in desc)
            SingleValueNode compareNode;
            if (orderBy.Direction == OrderByDirection.Descending)
            {
                if (isNullValue)
                {
                    compareNode = FalseConstantNode;
                }
                else
                {
                    SingleValueNode ltNode = new BinaryOperatorNode(BinaryOperatorKind.LessThan, propNode, constantNode);
                    SingleValueNode eqNullNode = new BinaryOperatorNode(BinaryOperatorKind.Equal, propNode, NullConstantNode);
                    compareNode = new BinaryOperatorNode(BinaryOperatorKind.Or, ltNode, eqNullNode);
                }
            }
            else
            {
                compareNode = isNullValue
                    ? (SingleValueNode)new BinaryOperatorNode(BinaryOperatorKind.NotEqual, propNode, NullConstantNode)
                    : new BinaryOperatorNode(BinaryOperatorKind.GreaterThan, propNode, constantNode);
            }

            // Equality node used in the multi-column AND chain: (prop eq value)
            SingleValueNode equalityNode = new BinaryOperatorNode(BinaryOperatorKind.Equal, propNode, constantNode);

            if (isFirst)
            {
                whereNode = compareNode;
                previousEqualityNode = equalityNode;
                isFirst = false;
            }
            else
            {
                // condition  = (previousEquality AND compare)
                // where      = (where OR condition)
                // prevEquality = (previousEquality AND (prop eq value))
                SingleValueNode conditionNode = new BinaryOperatorNode(BinaryOperatorKind.And, previousEqualityNode, compareNode);
                whereNode = new BinaryOperatorNode(BinaryOperatorKind.Or, whereNode, conditionNode);
                previousEqualityNode = new BinaryOperatorNode(BinaryOperatorKind.And, previousEqualityNode, equalityNode);
            }
        }

        return whereNode;
    }

    /// <summary>
    /// Parses a raw skip-token string value into a typed <see cref="ConstantNode"/> for use in an AST.
    /// </summary>
    /// <remarks>
    /// Three cases are handled:
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Enum</b> — <c>ConvertFromUriLiteral</c> fails for CLR-formatted enum literals
    ///     (<c>'+'</c>-separated nested type names). The member name is extracted directly from
    ///     <c>"TypeName'MemberName'"</c> and validated against the EDM enum's declared members.
    ///     An <see cref="ODataException"/> is thrown if the closing quote is missing, the member
    ///     name is not a declared member, or the member name is not a valid integer underlying value.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Null <see cref="IEdmTypeReference"/> (open-type dynamic property)</b> — the value is
    ///     parsed without a type reference.
    ///   </description></item>
    ///   <item><description>
    ///     <b>All other primitives</b> — <c>ConvertFromUriLiteral</c> is called with the declared EDM type.
    ///     Any trailing characters beyond the valid literal are rejected by the parser.
    ///   </description></item>
    /// </list>
    /// </remarks>
    private static ConstantNode BuildTypedConstantNode(
        string rawValue,
        IEdmTypeReference typeRef,
        IEdmModel model,
        string skipTokenRawValue)
    {
        if (typeRef == null)
        {
            // Open-entity-type dynamic property: no declared type. Attempt a generic parse.
            try
            {
                object value = ODataUriUtils.ConvertFromUriLiteral(rawValue, ODataVersion.V401);
                return new ConstantNode(value, rawValue);
            }
            catch (ODataException)
            {
                throw new ODataException(Error.Format(SRResources.SkipTokenParseError, skipTokenRawValue));
            }
        }

        if (typeRef.IsEnum())
        {
            IEdmEnumType enumDefinition = typeRef.AsEnum().EnumDefinition();
            string edmTypeName = enumDefinition.FullName();
            int openingQuoteIdx = rawValue.IndexOf('\'');
            ReadOnlySpan<char> memberSpan;
            string memberName = null;

            if (openingQuoteIdx >= 0)
            {
                int closingQuoteIdx = rawValue.IndexOf('\'', openingQuoteIdx + 1);
                if (closingQuoteIdx < 0)
                {
                    throw new ODataException(Error.Format(SRResources.SkipTokenParseError, skipTokenRawValue));
                }

                // Use a span for validation to avoid allocating the member-name string on the
                // rejection path. The string is only materialized after validation passes.
                memberSpan = rawValue.AsSpan(openingQuoteIdx + 1, closingQuoteIdx - openingQuoteIdx - 1);
            }
            else
            {
                // No quotes: the whole raw value is the member name. Reuse the existing
                // string reference — no allocation needed.
                memberName = rawValue;
                memberSpan = rawValue.AsSpan();
            }

            bool memberFound = long.TryParse(memberSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
            if (!memberFound)
            {
                foreach (IEdmEnumMember m in enumDefinition.Members)
                {
                    if (memberSpan.Equals(m.Name.AsSpan(), StringComparison.Ordinal))
                    {
                        memberFound = true;
                        break;
                    }
                }
            }

            if (!memberFound)
            {
                throw new ODataException(Error.Format(SRResources.SkipTokenParseError, skipTokenRawValue));
            }

            // Materialise the string only after validation confirms it is a known member.
            memberName ??= memberSpan.ToString();
            return new ConstantNode(new ODataEnumValue(memberName, edmTypeName), rawValue, typeRef);
        }

        try
        {
            object parsedValue = ODataUriUtils.ConvertFromUriLiteral(rawValue, ODataVersion.V401, model, typeRef);
            return new ConstantNode(parsedValue, rawValue, typeRef);
        }
        catch (ODataException)
        {
            throw new ODataException(Error.Format(SRResources.SkipTokenParseError, skipTokenRawValue));
        }
    }

    internal static string[] PopulatePropertyValues(string value)
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
                // with the property name, only contains the value
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
    internal static List<OrderByClause> GetOrderByClauses(object lastMember, IEdmModel model, OrderByClause clause)
    {
        IEdmType edmType = GetTypeFromObject(lastMember, model);
        IEdmEntityType entity = edmType as IEdmEntityType;
        if (entity == null)
        {
            return null;
        }

        List<OrderByClause> orderByClauses = new List<OrderByClause>();
        HashSet<IEdmProperty> properties = new HashSet<IEdmProperty>();
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
