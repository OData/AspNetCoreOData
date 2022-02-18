//-----------------------------------------------------------------------------
// <copyright file="EntitySetTemplateSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataDynamicModel.Extensions
{
    public class EntitySetTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/{entityset}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("entityset", out object classname))
            {
                return false;
            }

            string entitySetName = classname as string;

            // if you want to support case-insensitive
            var edmEntitySet = context.Model.EntityContainer.EntitySets()
                .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

            //var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);
            if (edmEntitySet != null)
            {
                EntitySetSegment segment = new EntitySetSegment(edmEntitySet);
                context.Segments.Add(segment);
                return true;
            }

            return false;
        }
    }
}
