// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match an <see cref="IEdmActionImport"/>.
    /// </summary>
    public class ActionImportSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="actionImport">.</param>
        public ActionImportSegmentTemplate(IEdmActionImport actionImport)
        {
            ActionImport = actionImport ?? throw new ArgumentNullException(nameof(actionImport));

            if (actionImport.Action.ReturnType != null)
            {
                IsSingle = actionImport.Action.ReturnType.TypeKind() != EdmTypeKind.Collection;

                EdmType = actionImport.Action.ReturnType.Definition;
            }
        }

        /// <inheritdoc />
        public override string Literal => ActionImport.Name;

        /// <inheritdoc />
        public override IEdmType EdmType { get; }

        /// <summary>
        /// Gets the wrapped action import.
        /// </summary>
        public IEdmActionImport ActionImport { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.ActionImport;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new OperationImportSegment(ActionImport, null);
        }
    }
}
