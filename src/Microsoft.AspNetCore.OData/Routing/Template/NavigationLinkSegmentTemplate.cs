//-----------------------------------------------------------------------------
// <copyright file="NavigationLinkSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template;

/// <summary>
/// Represents a template that can match a <see cref="NavigationPropertyLinkSegment"/> and a potential key.
/// </summary>
public class NavigationLinkSegmentTemplate : ODataSegmentTemplate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationLinkSegmentTemplate" /> class.
    /// </summary>
    /// <param name="navigationProperty">The Edm navigation property.</param>
    /// <param name="navigationSource">The Edm navigation source. This could be null.</param>
    public NavigationLinkSegmentTemplate(IEdmNavigationProperty navigationProperty, IEdmNavigationSource navigationSource)
        : this(new NavigationPropertyLinkSegment(navigationProperty, navigationSource))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationLinkSegmentTemplate" /> class.
    /// </summary>
    /// <param name="segment">The navigation link segment.</param>
    public NavigationLinkSegmentTemplate(NavigationPropertyLinkSegment segment)
    {
        Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
    }

    /// <summary>
    /// Gets/sets the related key template.
    /// </summary>
    public KeySegmentTemplate Key { get; set; }

    /// <summary>
    /// Gets the Edm navigation property.
    /// </summary>
    public IEdmNavigationProperty NavigationProperty => Segment.NavigationProperty;

    /// <summary>
    /// Gets the navigation source.
    /// </summary>
    public IEdmNavigationSource NavigationSource => Segment.NavigationSource;

    /// <summary>
    /// Gets the navigation property link segment.
    /// </summary>
    public NavigationPropertyLinkSegment Segment { get; }

    /// <inheritdoc />
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        if (Key != null)
        {
            IEnumerable<string> keyTemplates = Key.GetTemplates(options);
            foreach (var keyTemplate in keyTemplates)
            {
                yield return $"/{NavigationProperty.Name}{keyTemplate}/$ref";
            }
        }
        else
        {
            yield return $"/{NavigationProperty.Name}/$ref";
        }
    }

    /// <inheritdoc />
    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        // ODL path has the "NavigationPropertyLinkSegment" first.
        context.Segments.Add(Segment);

        // Then, append the key if apply.
        if (Key != null)
        {
            return Key.TryTranslate(context);
        }

        return true;
    }
}
