// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a $ref segment.
    /// </summary>
    public class NavigatioRefTemplateSegmentTemplate : ODataSegmentTemplate
    {
        private readonly string ParameterName = "navigationProperty";

        /// <summary>
        /// Initializes a new instance of the <see cref="RefSegmentTemplate" /> class.
        /// </summary>
        /// <param name="declaredType">The Edm navigation property.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        public NavigatioRefTemplateSegmentTemplate(IEdmStructuredType declaredType, IEdmNavigationSource navigationSource)
            : this(declaredType, navigationSource, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefSegmentTemplate" /> class.
        /// </summary>
        /// <param name="declaredType">The Edm navigation property.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        /// <param name="relatedKey">The related key mapping string.</param>
        public NavigatioRefTemplateSegmentTemplate(IEdmStructuredType declaredType, IEdmNavigationSource navigationSource, string relatedKey)
        {
            StructuredType = declaredType ?? throw Error.ArgumentNull(nameof(declaredType));
            NavigationSource = navigationSource ?? throw Error.ArgumentNull(nameof(navigationSource));
            RelatedKey = relatedKey;
        }

        /// <summary>
        /// Gets the related key mapping.
        /// </summary>
        public string RelatedKey { get; }

        /// <summary>
        /// Gets the declared type for this property template.
        /// </summary>
        public IEdmStructuredType StructuredType { get; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            options = options ?? ODataRouteOptions.Default;

            if (RelatedKey != null)
            {
                if (options.EnableKeyInParenthesis && options.EnableKeyAsSegment)
                {
                    yield return $"{{{ParameterName}}}({{{RelatedKey}}})/$ref";
                    yield return $"{{{ParameterName}}}/{{{RelatedKey}}}/$ref";
                }
                else if (options.EnableKeyInParenthesis)
                {
                    yield return $"{{{ParameterName}}}({{{RelatedKey}}})/$ref";
                }
                else if (options.EnableKeyAsSegment)
                {
                    yield return $"{{{ParameterName}}}/{{{RelatedKey}}}/$ref";
                }
                else
                {
                    throw new ODataException(SRResources.RouteOptionDisabledKeySegment);
                }
            }
            else
            {
                yield return $"/{{{ParameterName}}}/$ref";
            }
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            RouteValueDictionary routeValues = context.RouteValues;

            // the request should have the navigation property
            if (routeValues.TryGetValue(ParameterName, out object rawValue))
            {
                return false;
            }

            string navigationProperty = rawValue as string;
            if (navigationProperty == null)
            {
                return false;
            }

            // Find the navigation property
            IEdmNavigationProperty edmNavProperty = StructuredType.ResolveProperty(navigationProperty) as IEdmNavigationProperty;
            if (edmNavProperty == null)
            {
                return false;
            }

            IEdmNavigationSource targetNavigationSource = NavigationSource.FindNavigationTarget(edmNavProperty);

            // ODL implementation is complex, here i just use the NavigationPropertyLinkSegment
            context.Segments.Add(new NavigationPropertyLinkSegment(edmNavProperty, targetNavigationSource));

            if (RelatedKey != null)
            {
                // TODO:  to process the related key
                IEdmEntityType entityType = edmNavProperty.ToEntityType();

                IEdmStructuralProperty keyProperty = entityType.Key().SingleOrDefault();
                Contract.Assert(entityType.Key().Count() == 1);
                Contract.Assert(keyProperty != null);

                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { keyProperty.Name, $"{{{RelatedKey}}}" }
                };

                KeySegmentTemplate keySegment = new KeySegmentTemplate(keyValuePairs, entityType, targetNavigationSource);
                if (keySegment.TryTranslate(context))
                {
                    return true;
                }
            }

            return true;
        }
    }
}
