// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System;
using System.Linq;

namespace ODataCustomizedSample.Extensions
{
    public class EntitySetTemplateSegment : ODataSegmentTemplate
    {
        public override string Literal => "{classname}";

        public override ODataSegmentKind Kind => ODataSegmentKind.EntitySet;

        public override IEdmType EdmType => null;

        public override bool IsSingle => false;

        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("classname", out object classname))
            {
                return null;
            }

            string entitySetName = classname as string;

            // if you want to support case-insenstivie
            var edmEntitySet = context.Model.EntityContainer.EntitySets()
                .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

            //var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);
            if (edmEntitySet != null)
            {
                return new EntitySetSegment(edmEntitySet);
            }

            return null;
        }
    }
}
