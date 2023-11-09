//-----------------------------------------------------------------------------
// <copyright file="MetadataPropertiesDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Core;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Plant1;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties
{
    internal static class MetadataPropertiesDataSource
    {
        private readonly static List<Site> sites;
        private readonly static List<Plant> plants;
        private readonly static List<PipelineBase> pipelines;

        static MetadataPropertiesDataSource()
        {
            pipelines = new List<PipelineBase>(Enumerable.Range(1, 8).Select(idx => new Pipeline
            {
                Id = idx,
                Name = $"Pipeline {idx}",
                Length = idx * 100
            }));

            plants = new List<Plant>(Enumerable.Range(1, 4).Select(idx => new Plant
            {
                Id = idx,
                Name = $"Plant {idx}",
                Pipelines = pipelines.Skip((idx - 1) * 2).Take(2)
            }));

            sites = new List<Site>(Enumerable.Range(1, 2).Select(idx => new Site
            {
                Id = idx,
                Name = $"Site {idx}",
                Plants = plants.Skip((idx - 1) * 2).Take(2)
            }));

            for (var i = 0; i < plants.Count; i++)
            {
                plants[i].Site = sites[i / 2];
            }

            for (var i = 0; i < pipelines.Count; i++)
            {
                pipelines[i].Plant = plants[i / 2];
            }
        }

        public static List<Site> Sites => sites;
    }
}
