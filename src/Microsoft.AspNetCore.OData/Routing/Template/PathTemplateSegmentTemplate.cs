﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    ///  Represents a template that can match a <see cref="PathTemplateSegment"/>.
    ///  From OData Lib:
    ///  If template parseing enabled, any literal wrappered with "{" and "}" is considered as PathTemplateSegment.
    ///  So, here's the design (so far, we can add more later):
    ///  {property} ==> declared property
    ///  {dynamicproperty} => dynamic property
    ///  TODO: we can change to use route constraint, for example:
    ///  {name:odataproperty}
    ///  {name:odatadynamic}
    ///  {name:odatacast}
    ///  {name:odataentityset} ...
    /// </summary>
    public class PathTemplateSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathTemplateSegmentTemplate"/> class.
        /// </summary>
        /// <param name="segment">The path template segment to be parsed as a template.</param>
        public PathTemplateSegmentTemplate(PathTemplateSegment segment)
        {
            Segment = segment ?? throw new ArgumentNullException(nameof(segment));

            if (!IsRouteParameter(segment.LiteralText))
            {
                throw new ODataException(
                    Error.Format(SRResources.InvalidAttributeRoutingTemplateSegment, segment.LiteralText));
            }

            // ParameterName = property or dynamicproperty
            ParameterName = segment.LiteralText.Substring(1, segment.LiteralText.Length - 2);

            if (string.IsNullOrEmpty(ParameterName))
            {
                Error.Format(SRResources.EmptyParameterAlias, ParameterName, segment.LiteralText);
            }
        }

        /// <inheritdoc />
        public override string Literal => Segment.LiteralText;

        /// <inheritdoc />
        public override IEdmType EdmType => null;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.PathTemplate;

        /// <inheritdoc />
        public override bool IsSingle => false;

        /// <summary>
        /// Gets the segment name
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The parameter name of the dynamic property.
        /// </summary>
        public PathTemplateSegment Segment { get; }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // {property}
            if (ParameterName == "property")
            {
                ODataPathSegment previous = context.Segments.LastOrDefault();
                ODataPathSegment property = CreatePropertySegment(previous, context);
                if (property != null)
                {
                    context.Segments.Add(property);
                    return true;
                }
            }
            else if (ParameterName == "dynamicproperty")
            {
                // {dynamicproperty}
                ODataPathSegment previous = context.Segments.LastOrDefault();
                DynamicPathSegment dynamicSeg = CreateDynamicSegment(previous, context);
                if (dynamicSeg != null)
                {
                    context.Segments.Add(dynamicSeg);
                    return true;
                }
            }

            return false;
        }

        private static ODataPathSegment CreatePropertySegment(ODataPathSegment previous, ODataTemplateTranslateContext context)
        {
            if (previous == null)
            {
                return null;
            }

            IEdmStructuredType previewEdmType = previous.EdmType as IEdmStructuredType;
            if (previewEdmType == null)
            {
                return null;
            }

            if (!context.RouteValues.TryGetValue("property", out object value))
            {
                return null;
            }

            string propertyName = value as string;
            IEdmStructuralProperty edmProperty = previewEdmType.StructuralProperties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (edmProperty != null)
            {
                return new PropertySegment(edmProperty);
            }

            IEdmNavigationProperty navProperty = previewEdmType.NavigationProperties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (navProperty != null)
            {
                // TODO: shall we calculate the navigation source for navigation segment?
               return new NavigationPropertySegment(navProperty, null);
            }

            return null;
        }

        private static DynamicPathSegment CreateDynamicSegment(ODataPathSegment previous, ODataTemplateTranslateContext context)
        {
            if (previous == null)
            {
                return null;
            }

            IEdmStructuredType previewEdmType = previous.EdmType as IEdmStructuredType;
            if (previewEdmType == null || !previewEdmType.IsOpen)
            {
                return null;
            }

            if (!context.RouteValues.TryGetValue("dynamicproperty", out object value))
            {
                return null;
            }

            string propertyName = value as string;
            IEdmProperty edmProperty = previewEdmType.Properties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (edmProperty != null)
            {
                return null;
            }

            return new DynamicPathSegment(propertyName);
        }

        private static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
