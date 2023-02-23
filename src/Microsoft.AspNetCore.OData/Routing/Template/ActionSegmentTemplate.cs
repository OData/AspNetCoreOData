//-----------------------------------------------------------------------------
// <copyright file="ActionSegmentTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
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

            // Only accept the bound action
            if (!action.IsBound)
            {
                throw new ODataException(Error.Format(SRResources.OperationIsNotBound, action.Name, "action"));
            }

            NavigationSource = navigationSource;
            Segment = new OperationSegment(Action, NavigationSource as IEdmEntitySetBase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The operation segment.</param>
        public ActionSegmentTemplate(OperationSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            IEdmOperation operation = segment.Operations.First();
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

        /// <summary>
        /// Gets the wrapped Edm navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the action segment.
        /// </summary>
        public OperationSegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            options = options ?? ODataRouteOptions.Default;
            Contract.Assert(options.EnableQualifiedOperationCall || options.EnableUnqualifiedOperationCall);

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                yield return $"/{Action.FullName()}";
                yield return $"/{Action.Name}";
            }
            else if (options.EnableQualifiedOperationCall)
            {
                yield return $"/{Action.FullName()}";
            }
            else
            {
                yield return $"/{Action.Name}";
            }
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (NavigationSource != null)
            {
                context.Segments.Add(Segment);
                return true;
            }

            IEdmNavigationSource navigationSource = SegmentTemplateHelpers.GetNavigationSourceFromEdmOperation(context.Model, Action);
            OperationSegment actionSegment = new OperationSegment(Action, navigationSource as IEdmEntitySetBase);
            context.Segments.Add(actionSegment);
            return true;
        }
    }
}
