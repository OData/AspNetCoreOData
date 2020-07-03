// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.Routing;
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
        public ActionSegmentTemplate(IEdmAction action)
            : this(action, unqualifiedFunctionCall: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionSegmentTemplate" /> class.
        /// </summary>
        /// <param name="action">The Edm action.</param>
        /// <param name="unqualifiedFunctionCall">Unqualified function/action call boolean value.</param>
        public ActionSegmentTemplate(IEdmAction action, bool unqualifiedFunctionCall)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));

            if (action.ReturnType != null)
            {
                IsSingle = action.ReturnType.TypeKind() != EdmTypeKind.Collection;
            }
        }

        /// <inheritdoc />
        public override string Literal => Action.FullName();

        /// <summary>
        /// Gets the wrapped Edm action.
        /// </summary>
        public IEdmAction Action { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Action;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            IEdmNavigationSource targetset = null;
            if (Action.ReturnType != null)
            {
                targetset = Action.GetTargetEntitySet(previous, model);
            }

            return new OperationSegment(Action, targetset as IEdmEntitySetBase);
        }
    }
}
