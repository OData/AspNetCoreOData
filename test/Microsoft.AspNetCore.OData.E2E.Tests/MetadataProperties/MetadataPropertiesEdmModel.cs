//-----------------------------------------------------------------------------
// <copyright file="MetadataPropertiesEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Core;
using Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties.Plant1;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MetadataProperties
{
    public class MetadataPropertiesEdmModel
    {
        /// <summary>
        /// Returns model where Site and Plant navigation properties are non-contained and navigation source is contained.
        /// </summary>
        /// <returns>Returns Edm model.</returns>
        public static IEdmModel GetEdmModelWithNonContainedNavPropInContainedNavSource()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<EntityBase>();
            modelBuilder.EntityType<Site>();
            modelBuilder.EntityType<Plant>();
            modelBuilder.EntityType<PipelineBase>();
            modelBuilder.EntityType<Pipeline>();
            modelBuilder.EntitySet<Site>("Sites");

            var model = modelBuilder.GetEdmModel();

            var sitesEntitySet = (EdmEntitySet)model.FindDeclaredEntitySet("Default.Container.Sites");
            var plantsNavigationProperty = sitesEntitySet.EntityType().DeclaredNavigationProperties().Single(d => d.Name.Equals("Plants"));
            var plantsContainedEntitySet = sitesEntitySet.FindNavigationTarget(plantsNavigationProperty);
            var siteNavigationProperty = plantsContainedEntitySet.EntityType().DeclaredNavigationProperties().Single(d => d.Name.Equals("Site"));
            var pipelinesNavigationProperty = plantsContainedEntitySet.EntityType().DeclaredNavigationProperties().Single(d => d.Name.Equals("Pipelines"));
            var pipelineContainedEntitySet = plantsContainedEntitySet.FindNavigationTarget(pipelinesNavigationProperty, new EdmPathExpression("Plants", "Pipelines"));
            var plantNavigationProperty = pipelineContainedEntitySet.EntityType().DeclaredNavigationProperties().Single(d => d.Name.Equals("Plant"));

            sitesEntitySet.AddNavigationTarget(siteNavigationProperty, sitesEntitySet, new EdmPathExpression("Plants", "Site"));
            sitesEntitySet.AddNavigationTarget(plantNavigationProperty, plantsContainedEntitySet, new EdmPathExpression("Plants", "Pipelines", "Plant"));

            return model;
        }

        /// <summary>
        /// Returns model where Site and Plant navigation properties are contained and navigation source is contained.
        /// </summary>
        /// <returns>Returns Edm model.</returns>
        public static IEdmModel GetEdmModelWithContainedNavPropInContainedNavSource()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<EntityBase>();
            modelBuilder.EntityType<Site>();
            // Make Site and Plant navigation properties contained
            modelBuilder.EntityType<Plant>().ContainsRequired(d => d.Site).Contained();
            modelBuilder.EntityType<PipelineBase>().ContainsRequired(d => d.Plant).Contained();
            modelBuilder.EntityType<Pipeline>();
            modelBuilder.EntitySet<Site>("Sites");

            var model = modelBuilder.GetEdmModel();

            return model;
        }
    }
}
