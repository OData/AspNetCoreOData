//-----------------------------------------------------------------------------
// <copyright file="TestGroupByWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.TestCommon.Query.Container;

namespace Microsoft.AspNetCore.OData.TestCommon.Query.Wrapper
{
    internal class TestGroupByWrapper : DynamicTypeWrapper
    {
        private Dictionary<string, object> values;
        protected static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();

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
                if (this.values == null)
                {
                    if (this.GroupByContainer != null)
                    {
                        this.values = this.GroupByContainer.ToDictionary(DefaultPropertyMapper);
                    }
                    else
                    {
                        this.values = new Dictionary<string, object>();
                    }

                    if (this.Container != null)
                    {
                        values.MergeWithReplace(this.Container.ToDictionary(DefaultPropertyMapper));
                    }
                }

                return this.values;
            }
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
