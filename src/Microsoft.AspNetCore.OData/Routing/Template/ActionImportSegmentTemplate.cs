﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
            : this(BuildSegment(actionImport, navigationSource as IEdmEntitySetBase))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionImportSegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The operation import segment.</param>
        internal ActionImportSegmentTemplate(OperationImportSegment segment)
        {
            Segment = segment ?? throw Error.ArgumentNull(nameof(segment));

            IEdmOperationImport operationImport = segment.OperationImports.FirstOrDefault();
            if (!operationImport.IsActionImport())
            {
                throw new ODataException(Error.Format(SRResources.SegmentShouldBeKind, "ActionImport", "ActionImportSegmentTemplate"));
            }

            ActionImport = (IEdmActionImport)operationImport;

            if (ActionImport.Action.ReturnType != null)
            {
                IsSingle = ActionImport.Action.ReturnType.TypeKind() != EdmTypeKind.Collection;

                EdmType = ActionImport.Action.ReturnType.Definition;
            }

            NavigationSource = segment.EntitySet;
        }

        /// <inheritdoc />
        public override string Literal => ActionImport.Name;

        /// <inheritdoc />
        public override IEdmType EdmType { get; }

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the wrapped action import.
        /// </summary>
        public IEdmActionImport ActionImport { get; }

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.ActionImport;

        /// <inheritdoc />
        public override bool IsSingle { get; }

        /// <summary>
        /// Gets the action import segment.
        /// </summary>
        public OperationImportSegment Segment { get; }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            context?.Segments.Add(Segment);
            return true;
        }

        private static OperationImportSegment BuildSegment(IEdmActionImport actionImport, IEdmNavigationSource navigationSource)
        {
            if (actionImport == null)
            {
                throw Error.ArgumentNull(nameof(actionImport));
            }

            return new OperationImportSegment(actionImport, navigationSource as IEdmEntitySetBase);
        }
    }
}
