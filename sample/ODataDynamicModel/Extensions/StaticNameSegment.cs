//-----------------------------------------------------------------------------
// <copyright file="StaticNameSegment.cs" company=".NET Foundation">
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

namespace ODataDynamicModel.Extensions;

public class StaticNameSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "/Name";
    }

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
