// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ODataDynamicModel.Extensions
{
    public class StaticNameSegment : ODataSegmentTemplate
    {
        public override string Literal => "Name";

        public override ODataSegmentKind Kind => ODataSegmentKind.Property;

        public override IEdmType EdmType => null;

        public override bool IsSingle => false;

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            KeySegment keySegment = context.Segments.Last() as KeySegment;
            IEdmEntityType entityType = keySegment.EdmType as IEdmEntityType;
            IEdmProperty edmProperty = entityType.Properties().FirstOrDefault(p => p.Name == "Name");
            if (edmProperty != null)
            {
                PropertySegment seg = new PropertySegment(edmProperty as IEdmStructuralProperty);
                context.Segments.Add(seg);
                return true;
            }

            return false;
        }
    }
}
