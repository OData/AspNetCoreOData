//-----------------------------------------------------------------------------
// <copyright file="TestFlatteningWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper;

internal class TestFlatteningWrapper<T> : TestGroupByWrapper, IGroupByWrapper<TestAggregationPropertyContainer, TestGroupByWrapper>, IFlatteningWrapper<T>
{
    public T Source { get; set; }
}

internal class TestFlatteningWrapperConverter<T> : JsonConverter<TestFlatteningWrapper<T>>
{
    public override TestFlatteningWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(TestFlatteningWrapper<>).Name));
    }

    public override void Write(Utf8JsonWriter writer, TestFlatteningWrapper<T> value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            JsonSerializer.Serialize(writer, value.Values, options);
        }
    }
}
