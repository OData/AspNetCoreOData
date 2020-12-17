// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    [JsonConverter(typeof(AggregationTypeWrapperConverter))]
    internal class AggregationWrapper : GroupByWrapper
    {
    }
}