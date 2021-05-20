// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmSingleton"/>.
    /// </summary>
    public class SingletonSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonSegmentTemplate" /> class.
        /// </summary>
        /// <param name="singleton">The Edm singleton.</param>
        public SingletonSegmentTemplate(IEdmSingleton singleton)
            : this(new SingletonSegment(singleton))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The singleton segment.</param>
        public SingletonSegmentTemplate(SingletonSegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));
        }

        /// <summary>
        /// Gets the wrapped Edm singleton.
        /// </summary>
        public IEdmSingleton Singleton => Segment.Singleton;

        /// <summary>
        /// Gets the wrapped singleton segment.
        /// </summary>
        public SingletonSegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{Segment.Singleton.Name}";
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
}
