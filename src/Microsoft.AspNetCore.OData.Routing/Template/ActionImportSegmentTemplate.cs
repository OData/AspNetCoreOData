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
    /// Represents a template that could match an <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public class ActionImportSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionImport">.</param>
        public ActionImportSegmentTemplate(IEdmActionImport actionImport)
        {
            ActionImport = actionImport ?? throw new ArgumentNullException(nameof(actionImport));
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Template => ActionImport.Name;

        /// <summary>
        /// 
        /// </summary>
        public IEdmActionImport ActionImport { get; }

        /// <inheritdoc />
        public override ODataPathSegment GenerateODataSegment(IEdmModel model,
            IEdmNavigationSource previous, RouteValueDictionary routeValue, QueryString queryString)
        {
            return new OperationImportSegment(ActionImport, null);
        }
    }
}
