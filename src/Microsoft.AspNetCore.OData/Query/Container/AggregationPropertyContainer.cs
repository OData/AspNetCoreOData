//-----------------------------------------------------------------------------
// <copyright file="AggregationPropertyContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace Microsoft.AspNetCore.OData.Query.Container;

/// <summary>
/// Represent properties used in groupby and aggregate clauses to make them accessible in further clauses/transformations
/// </summary>
/// <remarks>
/// When we have $apply=groupby((Prop1,Prop2, Prop3))&amp;$orderby=Prop1, Prop2
/// We will have following expression in .GroupBy
/// $it => new AggregationPropertyContainer() {
///     Name = "Prop1", 
///     Value = $it.Prop1, // string
///     Next = new AggregationPropertyContainer() {
///         Name = "Prop2",
///         Value = $it.Prop2, // int
///         Next = new LastInChain() {
///             Name = "Prop3",
///             Value = $it.Prop3
///         }
///     }
/// }
/// when in $orderby (see AggregationBinder CollectProperties method)
/// Prop1 could be referenced as $it => (string)$it.Value
/// Prop2 could be referenced as $it => (int)$it.Next.Value
/// Prop3 could be referenced as $it => (int)$it.Next.Next.Value
/// Generic type for Value is used to avoid type casts for primitive types that are not supported in EF
/// 
/// Also we have 4 use cases and base type have all required properties to support no cast usage. 
/// 1. Primitive property with Next
/// 2. Primitive property without Next
/// 3. Nested property with Next
/// 4. Nested property without Next
/// However, EF doesn't allow to set different properties for the same type in two places in an lambda-expression => using new type with just new name to workaround that issue
/// </remarks>
internal class AggregationPropertyContainer : NamedProperty<object>, IAggregationPropertyContainer<GroupByWrapper, AggregationPropertyContainer>
{
    public GroupByWrapper NestedValue
    {
        get
        {
            return (GroupByWrapper)this.Value;
        }
        set
        {
            Value = value;
        }
    }

    public IAggregationPropertyContainer<GroupByWrapper, AggregationPropertyContainer> Next { get; set; }

    public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
        bool includeAutoSelected)
    {
        base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
        if (Next != null)
        {
            Next.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
        }
    }

    public override object GetValue()
    {
        // Value is object and when Value is populated form the DB by EF or other ORM, it will not auto converted to null as in case of real type
        if (Value == DBNull.Value)
        {
            return null;
        }

        return base.GetValue();
    }

    private class LastInChain : AggregationPropertyContainer
    {
    }

    private class NestedPropertyLastInChain : AggregationPropertyContainer
    {
    }

    private class NestedProperty : AggregationPropertyContainer
    {
    }

    public static Expression CreateNextNamedPropertyContainer(IList<NamedPropertyExpression> properties)
    {
        Expression container = null;

        // build the linked list of properties.
        for (int i = 0; i < properties.Count; i++)
        {
            container = CreateNextNamedPropertyCreationExpression(properties[i], container);
        }

        return container;
    }

    private static Expression CreateNextNamedPropertyCreationExpression(NamedPropertyExpression property, Expression next)
    {
        Contract.Assert(property != null);
        Contract.Assert(property.Value != null);

        Type namedPropertyType = null;
        if (next != null)
        {
            if (property.Value.Type == typeof(GroupByWrapper))
            {
                namedPropertyType = typeof(NestedProperty);
            }
            else
            {
                namedPropertyType = typeof(AggregationPropertyContainer);
            }
        }
        else
        {
            if (property.Value.Type == typeof(GroupByWrapper))
            {
                namedPropertyType = typeof(NestedPropertyLastInChain);
            }
            else
            {
                namedPropertyType = typeof(LastInChain);
            }
        }

        List<MemberBinding> memberBindings = new List<MemberBinding>();

        memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty(QueryConstants.AggregationPropertyContainerNameProperty), property.Name));

        if (property.Value.Type == typeof(GroupByWrapper))
        {
            memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty(QueryConstants.AggregationPropertyContainerNestedValueProperty), property.Value));
        }
        else
        {
            memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty(QueryConstants.AggregationPropertyContainerValueProperty), property.Value));
        }

        if (next != null)
        {
            memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty(QueryConstants.AggregationPropertyContainerNextProperty), next));
        }

        if (property.NullCheck != null)
        {
            memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("IsNull"), property.NullCheck));
        }

        return Expression.MemberInit(Expression.New(namedPropertyType), memberBindings);
    }
}
