//-----------------------------------------------------------------------------
// <copyright file="PropertySegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template;

/// <summary>
/// Represents a template that could match an <see cref="IEdmStructuralProperty"/>.
/// </summary>
public class PropertySegmentTemplate : ODataSegmentTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySegmentTemplate" /> class.
    /// </summary>
    /// <param name="property">The wrapped Edm property.</param>
    public PropertySegmentTemplate(IEdmStructuralProperty property)
        : this(new PropertySegment(property))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySegmentTemplate" /> class.
    /// </summary>
    /// <param name="segment">The property segment.</param>
    public PropertySegmentTemplate(PropertySegment segment)
    {
        Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
    }

    /// <summary>
    /// Gets the wrapped Edm property.
    /// </summary>
    public IEdmStructuralProperty Property => Segment.Property;

    /// <summary>
    /// Gets the property segment.
    /// </summary>
    public PropertySegment Segment { get; }

    /// <inheritdoc />
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return $"/{Property.Name}";
    }

    /// <inheritdoc />
    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        context.Segments.Add(Segment);
        return true;
    }
}
