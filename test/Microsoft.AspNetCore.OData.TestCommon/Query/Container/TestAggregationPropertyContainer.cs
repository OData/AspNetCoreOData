//-----------------------------------------------------------------------------
// <copyright file="TestAggregationPropertyContainer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.TestCommon.Query.Wrapper;

namespace Microsoft.AspNetCore.OData.TestCommon.Query.Container
{
    internal class TestAggregationPropertyContainer : NamedProperty<object>
    {
        public TestGroupByWrapper NestedValue
        {
            get { return (TestGroupByWrapper)this.Value; }
            set { Value = value; }
        }

        public TestAggregationPropertyContainer Next { get; set; }

        public override void ToDictionaryCore(
            Dictionary<string, object> dictionary,
            IPropertyMapper propertyMapper,
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
            return (Value == DBNull.Value) ? null : base.GetValue();
        }

        public static Expression CreateNextNamedPropertyContainer(IList<NamedPropertyExpression> namedProperties)
        {
            Expression container = null;

            // Build the linked list of propeties
            foreach (var property in namedProperties)
            {
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
}
