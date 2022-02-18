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

namespace ODataCustomizedSample.Extensions
{
    public class EntitySetTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/{classname}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("classname", out object classname))
            {
                return false;
            }

            string entitySetName = classname as string;

            // if you want to support case-insenstivie
            var edmEntitySet = context.Model.EntityContainer.EntitySets()
                .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

            //var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);
            if (edmEntitySet != null)
            {
                context.Segments.Add(new EntitySetSegment(edmEntitySet));
                return true;
            }

            return false;
        }
    }
}
