//-----------------------------------------------------------------------------
// <copyright file="TestAggregationPropertyContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper;
using Microsoft.AspNetCore.OData.Query.Container;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Container;

internal class TestAggregationPropertyContainer : IAggregationPropertyContainer<TestGroupByWrapper, TestAggregationPropertyContainer>
{
    public string Name { get; set; }

    public object Value { get; set; }

    public TestGroupByWrapper NestedValue
    {
        get { return (TestGroupByWrapper)this.Value; }
        set { Value = value; }
    }

    public IAggregationPropertyContainer<TestGroupByWrapper, TestAggregationPropertyContainer> Next { get; set; }

    public void ToDictionaryCore(
        Dictionary<string, object> dictionary,
        IPropertyMapper propertyMapper,
        bool includeAutoSelected)
    {
        Contract.Assert(dictionary != null);

        if (Name != null && includeAutoSelected)
        {
            string mappedName = propertyMapper.MapProperty(Name);
            if (mappedName != null)
            {
                if (String.IsNullOrEmpty(mappedName))
                {
                    throw Error.InvalidOperation(SRResources.InvalidPropertyMapping, Name);
                }

                dictionary.Add(mappedName, Value);
            }
        }

        if (Next != null)
        {
            Next.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
        }
    }

    public static Expression CreateNextNamedPropertyContainer(IList<NamedPropertyExpression> namedProperties)
    {
        Expression container = null;

        // Build the linked list of properties
        for (int i = 0; i < namedProperties.Count; i++)
        {
            var property = namedProperties[i];
            Type namedPropertyType = null;
            if (container != null)
            {
                namedPropertyType = (property.Value.Type == typeof(TestGroupByWrapper)) ? typeof(NestedProperty) : typeof(TestAggregationPropertyContainer);
            }
            else
            {
                namedPropertyType = (property.Value.Type == typeof(TestGroupByWrapper)) ? typeof(NestedPropertyLastInChain) : typeof(LastInChain);
            }

            var bindings = new List<MemberBinding>
                {
                    Expression.Bind(namedPropertyType.GetProperty("Name"), property.Name)
                };

            if (property.Value.Type == typeof(TestGroupByWrapper))
            {
                bindings.Add(Expression.Bind(namedPropertyType.GetProperty("NestedValue"), property.Value));
            }
            else
            {
                bindings.Add(Expression.Bind(namedPropertyType.GetProperty("Value"), property.Value));
            }

            if (container != null)
            {
                bindings.Add(Expression.Bind(namedPropertyType.GetProperty("Next"), container));
            }

            if (property.NullCheck != null)
            {
                bindings.Add(Expression.Bind(namedPropertyType.GetProperty("IsNull"), property.NullCheck));
            }

            container = Expression.MemberInit(Expression.New(namedPropertyType), bindings);
        }

        return container;
    }

    private class NestedProperty : TestAggregationPropertyContainer { }

    private class LastInChain : TestAggregationPropertyContainer { }

    private class NestedPropertyLastInChain : TestAggregationPropertyContainer { }
}
