// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataDynamicModel.Extensions
{
    public class NavigationTemplateSegment : ODataSegmentTemplate
    {
        public override string Literal => "{navigation}";

        public override ODataSegmentKind Kind => ODataSegmentKind.Navigation;

        public override IEdmType EdmType => null;

        public override bool IsSingle => false;

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
