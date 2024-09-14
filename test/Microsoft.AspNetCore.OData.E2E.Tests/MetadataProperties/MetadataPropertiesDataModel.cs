//-----------------------------------------------------------------------------
// <copyright file="MetadataPropertiesDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Core;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.OData.ModelBuilder;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Attributes { get; set; }
}

public class Site : EntityBase
{
    [Contained]
    public IEnumerable<Plant> Plants { get; set; }
}

public class Plant : EntityBase
{
    public Site Site { get; set; }
    [Contained]
    public IEnumerable<PipelineBase> Pipelines { get; set; }
}

public abstract class PipelineBase : EntityBase
{
    public Plant Plant { get; set; }
}
