//-----------------------------------------------------------------------------
// <copyright file="TestGroupByWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Container;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper
{
    /// <summary>
    /// Test wrapper for GroupBy and aggregation transformations.
    /// </summary>
    /// <remarks>Overriding Equals and GetHashCode is especially important where input source is an in-memory collection of objects.</remarks>
    internal class TestGroupByWrapper : DynamicTypeWrapper, IGroupByWrapper<TestAggregationPropertyContainer, TestGroupByWrapper>
    {
        private Dictionary<string, object> values;
        protected static readonly IPropertyMapper testPropertyMapper = new TestPropertyMapper();

        /// <summary>
        /// Gets or sets the property container that contains the grouping properties
        /// </summary>
        public TestAggregationPropertyContainer GroupByContainer { get; set; }

        /// <summary>
        /// Gets or sets the property container that contains the aggregation properties
        /// </summary>
        public TestAggregationPropertyContainer Container { get; set; }

        public override Dictionary<string, object> Values
        {
            get
            {
                EnsureValues();

                return this.values;
            }
        }

        private void EnsureValues()
        {
            if (this.values == null)
            {
                if (this.GroupByContainer != null)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();

                    this.GroupByContainer.ToDictionaryCore(dictionary, testPropertyMapper, true);
                    this.values = dictionary;
                }
                else
                {
                    this.values = new Dictionary<string, object>();
                }

                if (this.Container != null)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();

                    this.Container.ToDictionaryCore(dictionary, testPropertyMapper, true);
                    this.values.MergeWithReplace(dictionary);
                }
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var compareWith = obj as TestGroupByWrapper;
            if (compareWith == null)
            {
                return false;
            }
            var dictionary1 = this.Values;
            var dictionary2 = compareWith.Values;
            return dictionary1.Count == dictionary2.Count && !dictionary1.Except(dictionary2).Any();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            EnsureValues();
            long hash = 1870403278L; //Arbitrary number from Anonymous Type GetHashCode implementation
            foreach (var v in this.Values.Values)
            {
                hash = (hash * -1521134295L) + (v == null ? 0 : v.GetHashCode());
            }

            return (int)hash;
        }
    }

    internal class TestGroupByWrapperConverter : JsonConverter<TestGroupByWrapper>
    {
        public override TestGroupByWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TestGroupByWrapper wrapper, JsonSerializerOptions options)
        {
            if (wrapper != null)
            {
                JsonSerializer.Serialize(writer, wrapper.Values, options);
            }
        }
    }
}
