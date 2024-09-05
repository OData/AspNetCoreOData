//-----------------------------------------------------------------------------
// <copyright file="EntitySetSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template;

/// <summary>
/// Represents a template that could match an <see cref="IEdmEntitySet"/>.
/// </summary>
public class EntitySetSegmentTemplate : ODataSegmentTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetSegmentTemplate" /> class.
    /// </summary>
    /// <param name="entitySet">The Edm entity set.</param>
    public EntitySetSegmentTemplate(IEdmEntitySet entitySet)
        : this(new EntitySetSegment(entitySet))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySetSegmentTemplate" /> class.
    /// </summary>
    /// <param name="segment">The entity set segment.</param>
    public EntitySetSegmentTemplate(EntitySetSegment segment)
    {
        Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
    }

    /// <summary>
    /// Gets the wrapped Edm entityset.
    /// </summary>
    public IEdmEntitySet EntitySet => Segment.EntitySet;

    /// <summary>
    /// Gets the entity set segment.
    /// </summary>
    public EntitySetSegment Segment { get; }

    /// <inheritdoc />
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return $"/{Segment.EntitySet.Name}";
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
