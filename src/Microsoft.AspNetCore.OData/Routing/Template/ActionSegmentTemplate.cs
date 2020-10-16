// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Linq;

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
        /// <param name="navigationSource">Unqualified function/action call boolean value.</param>
        public ActionSegmentTemplate(IEdmAction action, IEdmNavigationSource navigationSource)
        {
            Action = action ?? throw Error.ArgumentNull(nameof(action));
            NavigationSource = navigationSource;

            if (action.ReturnType != null)
            {
                IsSingle = action.ReturnType.TypeKind() != EdmTypeKind.Collection;
                EdmType = action.ReturnType.Definition;
            }

            Segment = new OperationSegment(Action, NavigationSource as IEdmEntitySetBase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="operationSegment">The operation segment, it should be a function segment and the parameters are template.</param>
        public ActionSegmentTemplate(OperationSegment operationSegment)
        {
            if (operationSegment == null)
            {
                throw Error.ArgumentNull(nameof(operationSegment));
            }

            IEdmOperation operation = operationSegment.Operations.FirstOrDefault();
            if (!operation.IsAction())
            {
                throw new ODataException(Error.Format(SRResources.SegmentShouldBeKind, "Action", "ActionSegmentTemplate"));
            }

            Action = (IEdmAction)operation;
            if (Action.ReturnType != null)
            {
                IsSingle = Action.ReturnType.TypeKind() != EdmTypeKind.Collection;
                EdmType = Action.ReturnType.Definition;
            }

            Segment = operationSegment;
        }

        /// <inheritdoc />
        public override string Literal => Action.FullName();

        /// <inheritdoc />
        public override IEdmType EdmType { get; }

        /// <summary>
        /// Gets the wrapped Edm action.
        /// </summary>
        public IEdmAction Action { get; }

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Action;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <summary>
        /// Gets the action segment.
        /// </summary>
        public OperationSegment Segment { get; }

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            return Segment;
        }
    }
}
