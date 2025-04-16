//-----------------------------------------------------------------------------
// <copyright file="IAggregationPropertyContainerOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace Microsoft.AspNetCore.OData.Query.Container;

/// <summary>
/// Represent properties used in groupby and aggregate clauses of $apply query to make them accessible in further clauses/transformations.
/// </summary>
/// <typeparam name="TWrapper">The type of the group-by wrapper associated with this container.</typeparam>
/// <typeparam name="TContainer">The concrete type of the aggregation property container, enabling self-referencing.</typeparam>
/// <remarks>
/// When we have
/// <code>$apply=groupby((Prop1,Prop2,Prop3))&amp;$orderby=Prop1,Prop2</code>
/// where <see cref="AggregationPropertyContainer"/> implements <see cref="IAggregationPropertyContainer{TWrapper, TContainer}"/>,
/// we will have following expression in .GroupBy:
/// <code>
/// $it => new AggregationPropertyContainer() {
///     Name = "Prop1",
///     Value = $it.Prop1, /* string */
///     Next = new AggregationPropertyContainer() {
///         Name = "Prop2",
///         Value = $it.Prop2, /* int */
///         Next = new LastInChain() {
///             Name = "Prop3",
///             Value = $it.Prop3 /* int */
///         }
///     }
/// }
/// </code>
/// When in $orderby,
/// Prop1 could be referenced as $it => (string)$it.Value,
/// Prop2 could be referenced as $it => (int)$it.Next.Value,
/// Prop3 could be referenced as $it => (int)$it.Next.Next.Value.
/// Generic type for Value is used to avoid type casts for primitive types that are not supported in Entity Framework.
/// Also, we have 4 use cases and this interface declares all required properties to support no cast usage.
/// 1). Primitive property with Next
/// 2). Primitive property without Next
/// 3). Nested property with Next
/// 4). Nested property without Next.
/// However, Entity Framework doesn't allow to set different properties for the same type in two places in a lambda expression.
/// Using new type with just new name to workaround that issue.
/// </remarks>
public interface IAggregationPropertyContainer<TWrapper, TContainer>
    where TWrapper : IGroupByWrapper<TContainer, TWrapper>
    where TContainer : IAggregationPropertyContainer<TWrapper, TContainer>
{
    /// <summary>Gets or sets the name of the property.</summary>
    string Name { get; set; }

    /// <summary>Gets or sets the value of the property.</summary>
    object Value { get; set; }

    /// <summary>Gets or sets the nested value of the property.</summary>
    TWrapper NestedValue { get; set; }

    /// <summary>Gets or sets the next property container.</summary>
    IAggregationPropertyContainer<TWrapper, TContainer> Next { get; set; }

    /// <summary>
    /// Adds the properties in this container to the given dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary to which the properties in this container should be added.</param>
    /// <param name="propertyMapper">The property mapper to use for mapping
    /// between the names of properties in this container and the names that
    /// should be used when adding the properties to the given dictionary.</param>
    /// <param name="includeAutoSelected">A value indicating whether auto-selected properties should be included.</param>
    void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper, bool includeAutoSelected);
}
