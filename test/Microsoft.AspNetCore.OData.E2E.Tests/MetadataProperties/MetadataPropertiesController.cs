//-----------------------------------------------------------------------------
// <copyright file="MetadataPropertiesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Core;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties
{
    [Route("NonContainedNavPropInContainedNavSource")]
    [Route("ContainedNavPropInContainedNavSource")]
    public class SitesController : ODataController
    {
        [EnableQuery]
        [HttpGet("Sites")]
        public ActionResult<IEnumerable<Site>> Get()
        {
            return MetadataPropertiesDataSource.Sites;
        }

        [EnableQuery]
        [HttpGet("Sites({key})")]
        public ActionResult<Site> Get(int key)
        {
            var site = MetadataPropertiesDataSource.Sites.SingleOrDefault(d => d.Id == key);

            if (site == null)
            {
                return NotFound();
            }

            return site;
        }

        [EnableQuery]
        [HttpGet("Sites({key})/Plants")]
        public ActionResult<IEnumerable<Plant>> GetPlants(int key)
        {
            var site = MetadataPropertiesDataSource.Sites.SingleOrDefault(d => d.Id == key);

            if (site == null || site.Plants == null)
            {
                return Enumerable.Empty<Plant>().ToList();
            }

            return site.Plants.ToList();
        }

        [EnableQuery]
        [HttpGet("Sites({siteKey})/Plants({plantKey})")]
        public ActionResult<Plant> GetPlant(int siteKey, int plantKey)
        {
            var plant = MetadataPropertiesDataSource.Sites.SingleOrDefault(d => d.Id == siteKey)?.Plants?.SingleOrDefault(d => d.Id == plantKey);

            if (plant == null)
            {
                return NotFound();
            }

            return plant;
        }

        [EnableQuery]
        [HttpGet("Sites({siteKey})/Plants({plantKey})/Pipelines")]
        public ActionResult<IEnumerable<PipelineBase>> GetPipelines(int siteKey, int plantKey)
        {
            var plant = MetadataPropertiesDataSource.Sites.SingleOrDefault(d => d.Id == siteKey)?.Plants?.SingleOrDefault(d => d.Id == plantKey);

            if (plant == null || plant.Pipelines == null)
            {
                return Enumerable.Empty<PipelineBase>().ToList();
            }

            return plant.Pipelines.ToList();
        }

        [EnableQuery]
        [HttpGet("Sites({siteKey})/Plants({plantKey})/Pipelines({pipelineKey})")]
        public ActionResult<PipelineBase> GetPlantPipeline(int siteKey, int plantKey, int pipelineKey)
        {
            var pipeline = MetadataPropertiesDataSource.Sites.SingleOrDefault(d => d.Id == siteKey)?.Plants?.SingleOrDefault(d => d.Id == plantKey)?.Pipelines?.SingleOrDefault(d => d.Id == pipelineKey);

            if (pipeline == null)
            {
                return NotFound();
            }

            return pipeline;
        }
    }
}
