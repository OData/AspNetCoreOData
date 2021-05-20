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
    /// Represents a template that could match a $ref on a generic navigation segment.
    /// </summary>
    public class NavigationLinkTemplateSegmentTemplate : ODataSegmentTemplate
    {
        private readonly string ParameterName = "navigationProperty";

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationLinkTemplateSegmentTemplate" /> class.
        /// </summary>
        /// <param name="declaringType">The declaring type.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        public NavigationLinkTemplateSegmentTemplate(IEdmStructuredType declaringType, IEdmNavigationSource navigationSource)
        {
            DeclaringType = declaringType ?? throw Error.ArgumentNull(nameof(declaringType));
            NavigationSource = navigationSource ?? throw Error.ArgumentNull(nameof(navigationSource));
        }

        /// <summary>
        /// Gets the related key mapping.
        /// </summary>
        public string RelatedKey { get; set; }

        /// <summary>
        /// Gets the declaring type for this property template.
        /// </summary>
        public IEdmStructuredType DeclaringType { get; }

        /// <summary>
        /// Gets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            if (RelatedKey != null)
            {
                options = options ?? ODataRouteOptions.Default;

                if (options.EnableKeyInParenthesis && options.EnableKeyAsSegment)
                {
                    yield return $"/{{{ParameterName}}}({{{RelatedKey}}})/$ref";
                    yield return $"/{{{ParameterName}}}/{{{RelatedKey}}}/$ref";
                }
                else if (options.EnableKeyInParenthesis)
                {
                    yield return $"/{{{ParameterName}}}({{{RelatedKey}}})/$ref";
                }
                else if (options.EnableKeyAsSegment)
                {
                    yield return $"/{{{ParameterName}}}/{{{RelatedKey}}}/$ref";
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
            if (!routeValues.TryGetValue(ParameterName, out object rawValue))
            {
                return false;
            }

            string navigationProperty = rawValue as string;
            if (navigationProperty == null)
            {
                return false;
            }

            // Find the navigation property
            IEdmNavigationProperty edmNavProperty = DeclaringType.ResolveProperty(navigationProperty) as IEdmNavigationProperty;
            if (edmNavProperty == null)
            {
                return false;
            }

            IEdmNavigationSource targetNavigationSource = NavigationSource.FindNavigationTarget(edmNavProperty);

            // ODL implementation is complex, here i just use the NavigationPropertyLinkSegment
            context.Segments.Add(new NavigationPropertyLinkSegment(edmNavProperty, targetNavigationSource));

            if (RelatedKey != null)
            {
                IEdmEntityType entityType = edmNavProperty.ToEntityType();

                // only handle the single key
                IEdmStructuralProperty keyProperty = entityType.Key().SingleOrDefault();
                Contract.Assert(entityType.Key().Count() == 1);
                Contract.Assert(keyProperty != null);

                IDictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { keyProperty.Name, $"{{{RelatedKey}}}" }
                };

                KeySegmentTemplate keySegment = new KeySegmentTemplate(keyValuePairs, entityType, targetNavigationSource);
                return keySegment.TryTranslate(context);
            }

            return true;
        }
    }
}
