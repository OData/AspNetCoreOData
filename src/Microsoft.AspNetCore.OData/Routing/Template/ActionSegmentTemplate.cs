// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmAction"/>.
    /// </summary>
    public class ActionSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="action">The Edm action.</param>
        /// <param name="navigationSource">The Edm navigation source.</param>
        public ActionSegmentTemplate(IEdmAction action, IEdmNavigationSource navigationSource)
        {
            Action = action ?? throw Error.ArgumentNull(nameof(action));
            NavigationSource = navigationSource;
            Segment = new OperationSegment(Action, NavigationSource as IEdmEntitySetBase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The operation segment, it should be a function segment and the parameters are template.</param>
        public ActionSegmentTemplate(OperationSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            IEdmOperation operation = segment.Operations.FirstOrDefault();
            if (!operation.IsAction())
            {
                throw new ODataException(Error.Format(SRResources.SegmentShouldBeKind, "Action", "ActionSegmentTemplate"));
            }

            Action = (IEdmAction)operation;
            NavigationSource = segment.EntitySet;
            Segment = segment;
        }

        /// <summary>
        /// Gets the wrapped Edm action.
        /// </summary>
        public IEdmAction Action { get; }

        /// <inheritdoc />
        public IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the action segment.
        /// </summary>
        public OperationSegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            options = options ?? ODataRouteOptions.Default;

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                yield return $"/{Action.FullName()}";
                yield return $"/{Action.Name}";
            }
            else if (options.EnableQualifiedOperationCall)
            {
                yield return $"/{Action.FullName()}";
            }
            else if (options.EnableUnqualifiedOperationCall)
            {
                yield return $"/{Action.Name}";
            }
            else
            {
                throw new ODataException(Error.Format(SRResources.RouteOptionDisabledOperationSegment, "action"));
            }
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
