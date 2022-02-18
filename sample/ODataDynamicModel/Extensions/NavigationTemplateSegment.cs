//-----------------------------------------------------------------------------
// <copyright file="NavigationTemplateSegment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataDynamicModel.Extensions
{
    public class NavigationTemplateSegment : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/{navigation}";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("navigation", out object navigationNameObj))
            {
                return false;
            }

            string navigationName = navigationNameObj as string;
            KeySegment keySegment = context.Segments.Last() as KeySegment;
            IEdmEntityType entityType = keySegment.EdmType as IEdmEntityType;

            IEdmNavigationProperty navigationProperty = entityType.NavigationProperties().FirstOrDefault(n => n.Name == navigationName);
            if (navigationProperty != null)
            {
                var navigationSource = keySegment.NavigationSource;
                IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty);

                NavigationPropertySegment seg = new NavigationPropertySegment(navigationProperty, navigationSource);
                context.Segments.Add(seg);
                return true;
            }

            return false;
        }
    }
}
