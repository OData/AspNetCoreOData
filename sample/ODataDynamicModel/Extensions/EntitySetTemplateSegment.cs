// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataDynamicModel.Extensions
{
    public class EntitySetTemplateSegment : ODataSegmentTemplate
    {
        public override string Literal => "{entityset}";

        public override ODataSegmentKind Kind => ODataSegmentKind.EntitySet;

        public override IEdmType EdmType => null;

        public override bool IsSingle => true;

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("entityset", out object classname))
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
                EntitySetSegment segment = new EntitySetSegment(edmEntitySet);
                context.Segments.Add(segment);
                return true;
            }

            return false;
        }
    }
}
