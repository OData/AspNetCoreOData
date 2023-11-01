//-----------------------------------------------------------------------------
// <copyright file="ODataPathSegmentToTemplateHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// Handle an OData path to a path segment templates.
    /// </summary>
    internal class ODataPathSegmentToTemplateHandler : PathSegmentHandler
    {
        private IEdmModel _model;
        private IList<ODataSegmentTemplate> _segmentTemplates;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentToTemplateHandler" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataPathSegmentToTemplateHandler(IEdmModel model)
        {
            _model = model;
            _segmentTemplates = new List<ODataSegmentTemplate>();
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        public IList<ODataSegmentTemplate> Templates => _segmentTemplates;

        /// <summary>
        /// Translate a <see cref="MetadataSegment"/>
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(MetadataSegment segment)
        {
            _segmentTemplates.Add(MetadataSegmentTemplate.Instance);
        }

        /// <summary>
        /// Translate a <see cref="ValueSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(ValueSegment segment)
        {
            _segmentTemplates.Add(new ValueSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="NavigationPropertyLinkSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(NavigationPropertyLinkSegment segment)
        {
           _segmentTemplates.Add(new NavigationLinkSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="CountSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(CountSegment segment)
        {
            _segmentTemplates.Add(CountSegmentTemplate.Instance);
        }

        /// <summary>
        /// Translate a <see cref="DynamicPathSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(DynamicPathSegment segment)
        {
            _segmentTemplates.Add(new DynamicSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="OperationSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(OperationSegment segment)
        {
            IEdmOperation operation = segment.Operations.First();
            if (operation.IsAction())
            {
                _segmentTemplates.Add(new ActionSegmentTemplate(segment));
            }
            else
            {
                _segmentTemplates.Add(new FunctionSegmentTemplate(segment));
            }
        }

        /// <summary>
        /// Translate a <see cref="OperationImportSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(OperationImportSegment segment)
        {
            if (segment.OperationImports.First().IsActionImport())
            {
                _segmentTemplates.Add(new ActionImportSegmentTemplate(segment));
            }
            else
            {
                _segmentTemplates.Add(new FunctionImportSegmentTemplate(segment));
            }
        }

        /// <summary>
        /// Translate a <see cref="PropertySegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(PropertySegment segment)
        {
            _segmentTemplates.Add(new PropertySegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="KeySegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(KeySegment segment)
        {
            Func<KeySegmentTemplate> BuildKeyTemplate = () =>
            {
                try
                {
                    return new KeySegmentTemplate(segment);
                }
                catch
                {
                    if (_model != null)
                    {
                        var alternateKeys = _model.ResolveAlternateKeyProperties(segment);
                        if (alternateKeys != null)
                        {
                            return new KeySegmentTemplate(segment, alternateKeys);
                        }
                    }

                    throw;
                }
            };

            KeySegmentTemplate keyTemplate = BuildKeyTemplate();

            ODataSegmentTemplate previous = _segmentTemplates.LastOrDefault();
            NavigationLinkSegmentTemplate preRef = previous as NavigationLinkSegmentTemplate;
            if (preRef != null)
            {
                preRef.Key = keyTemplate;
            }
            else
            {
                _segmentTemplates.Add(keyTemplate);
            }
        }

        /// <summary>
        /// Translate a <see cref="SingletonSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(SingletonSegment segment)
        {
            _segmentTemplates.Add(new SingletonSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="EntitySetSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(EntitySetSegment segment)
        {
            _segmentTemplates.Add(new EntitySetSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="NavigationPropertySegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(NavigationPropertySegment segment)
        {
            _segmentTemplates.Add(new NavigationSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="TypeSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(TypeSegment segment)
        {
            _segmentTemplates.Add(new CastSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="PathTemplateSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        public override void Handle(PathTemplateSegment segment)
        {
            _segmentTemplates.Add(new PathTemplateSegmentTemplate(segment));
        }

        /// <summary>
        /// Translate a <see cref="BatchSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override void Handle(BatchSegment segment)
        {
            throw new ODataException(Error.Format(SRResources.TargetKindNotImplemented, "ODataPathSegment", "BatchSegment"));
        }

        /// <summary>
        /// Translate a <see cref="BatchReferenceSegment"/>.
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override void Handle(BatchReferenceSegment segment)
        {
            throw new ODataException(Error.Format(SRResources.TargetKindNotImplemented, "ODataPathSegment", "BatchReferenceSegment"));
        }
    }
}
