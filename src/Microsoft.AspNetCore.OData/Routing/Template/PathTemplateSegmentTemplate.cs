// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    ///  Represents a template that can match a <see cref="PathTemplateSegment"/>.
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

            string value;
            SegmentName = segment.TranslatePathTemplateSegment(out value);

            PropertyName = value;
            TreatPropertyNameAsParameterName = false;

            if (IsRouteParameter(PropertyName))
            {
                PropertyName = PropertyName.Substring(1, PropertyName.Length - 2);
                TreatPropertyNameAsParameterName = true;

                if (string.IsNullOrEmpty(PropertyName))
                {
                    Error.Format(SRResources.EmptyParameterAlias, PropertyName, segment.LiteralText);
                }
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
        /// The parameter name of the dynamic property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets the segment name
        /// </summary>
        public string SegmentName { get; private set; }

        /// <summary>
        /// Indicates whether the template should match the name, or treat it as a parameter.
        /// </summary>
        private bool TreatPropertyNameAsParameterName { get; set; }

        /// <summary>
        /// The parameter name of the dynamic property.
        /// </summary>
        public PathTemplateSegment Segment { get; }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            // TODO: maybe save the property name.
            // or create the PropertySegment using the information in the context.
            context?.Segments.Add(Segment);
            return true;
        }

        private static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
