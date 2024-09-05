//-----------------------------------------------------------------------------
// <copyright file="MetadataPropertiesDataModel.Plant1.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Core;

// NOTE: Pipeline class defined in a different namespace to repro a reported scenario
namespace Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Plant1;

public class Pipeline : PipelineBase
{
    public int? Length { get; set; }
}
