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
    /// Represents a template that could match an <see cref="IEdmActionImport"/>.
    /// </summary>
    public class ActionImportSegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="actionImport">The wrapper action import.</param>
        /// <param name="navigationSource">The target navigation source. it could be null.</param>
        public ActionImportSegmentTemplate(IEdmActionImport actionImport, IEdmNavigationSource navigationSource)
        {
            ActionImport = actionImport ?? throw Error.ArgumentNull(nameof(actionImport));
            Segment = new OperationImportSegment(actionImport, navigationSource as IEdmEntitySetBase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The operation import segment.</param>
        public ActionImportSegmentTemplate(OperationImportSegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));

            IEdmOperationImport operationImport = segment.OperationImports.First();
            if (!operationImport.IsActionImport())
            {
                throw new ODataException(Error.Format(SRResources.SegmentShouldBeKind, "ActionImport", "ActionImportSegmentTemplate"));
            }

            ActionImport = (IEdmActionImport)operationImport;
        }

        /// <summary>
        /// Gets the wrapped action import.
        /// </summary>
        public IEdmActionImport ActionImport { get; }

        /// <summary>
        /// Gets the action import segment.
        /// </summary>
        public OperationImportSegment Segment { get; }

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return $"/{ActionImport.Name}";
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
