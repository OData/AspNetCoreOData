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
    /// Represents a template that could match an <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public class ActionSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">The type containes the key.</param>
        public ActionSegmentTemplate(IEdmAction action)
            : this(action, unqualifiedFunctionCall: false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">The type containes the key.</param>
        /// <param name="unqualifiedFunctionCall">Unqualified function call boolean value.</param>
        public ActionSegmentTemplate(IEdmAction action, bool unqualifiedFunctionCall)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));

            if (unqualifiedFunctionCall)
            {
                Template = action.Name;
            }
            else
            {
                Template = action.FullName();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEdmAction Action { get; }

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
