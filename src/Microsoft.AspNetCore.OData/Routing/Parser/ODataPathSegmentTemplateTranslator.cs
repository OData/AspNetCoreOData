// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
    internal class ODataPathSegmentToTemplateHandler : PathSegmentHandler
    {
        private IEdmModel _model;

        private IList<ODataSegmentTemplate> _segmentTemplates;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentTemplateTranslator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataPathSegmentToTemplateHandler(IEdmModel model)
        {
            _model = model;
            _segmentTemplates = new List<ODataSegmentTemplate>();
        }

        public IList<ODataSegmentTemplate> Templates => _segmentTemplates;

        public override void Handle(MetadataSegment segment)
        {
            _segmentTemplates.Add(MetadataSegmentTemplate.Instance);
        }

        public override void Handle(ValueSegment segment)
        {
            _segmentTemplates.Add(new ValueSegmentTemplate(segment));
        }

        public override void Handle(NavigationPropertyLinkSegment segment)
        {
            _segmentTemplates.Add(new NavigationSegmentTemplate(segment.NavigationProperty, segment.NavigationSource));
            _segmentTemplates.Add(new RefSegmentTemplate(segment.NavigationProperty, segment.NavigationSource));
        }

        public override void Handle(CountSegment segment)
        {
            _segmentTemplates.Add(CountSegmentTemplate.Instance);
        }

        public override void Handle(DynamicPathSegment segment)
        {
            _segmentTemplates.Add(new DynamicSegmentTemplate(segment));
        }
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

        public override void Handle(OperationImportSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            if (segment.OperationImports.First().IsActionImport())
            {
                _segmentTemplates.Add(new ActionImportSegmentTemplate(segment));
            }
            else
            {
                _segmentTemplates.Add(new FunctionImportSegmentTemplate(segment));
            }
        }

        public override void Handle(PropertySegment segment)
        {
            _segmentTemplates.Add(new PropertySegmentTemplate(segment));
        }


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
            RefSegmentTemplate preRef = previous as RefSegmentTemplate;
            if (preRef != null)
            {
                int count = _segmentTemplates.Count;
                _segmentTemplates.Insert(count - 1, keyTemplate);
            }
            else
            {
                _segmentTemplates.Add(keyTemplate);
            }
        }

        public override void Handle(SingletonSegment segment)
        {
            _segmentTemplates.Add(new SingletonSegmentTemplate(segment));
        }
        public override void Handle(EntitySetSegment segment)
        {
            _segmentTemplates.Add(new EntitySetSegmentTemplate(segment));

        }
        public override void Handle(NavigationPropertySegment segment)
        {
            _segmentTemplates.Add(new NavigationSegmentTemplate(segment));
        }

        public override void Handle(TypeSegment segment)
        {
            _segmentTemplates.Add(new CastSegmentTemplate(segment));
        }

        public override void Handle(PathTemplateSegment segment)
        {
            _segmentTemplates.Add(new PathTemplateSegmentTemplate(segment));
        }
    }

    /// <summary>
    /// Translator an OData path to a path segment templates.
    /// </summary>
    public class ODataPathSegmentTemplateTranslator : PathSegmentTranslator<ODataSegmentTemplate>
    {
        private IEdmModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathSegmentTemplateTranslator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public ODataPathSegmentTemplateTranslator(IEdmModel model)
        {
            _model = model;
        }

        /// <summary>
        /// Translate a TypeSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template</returns>
        public override ODataSegmentTemplate Translate(TypeSegment segment)
        {
            return new CastSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a NavigationPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(NavigationPropertySegment segment)
        {
            return new NavigationSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate an EntitySetSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(EntitySetSegment segment)
        {
            return new EntitySetSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate an SingletonSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(SingletonSegment segment)
        {
            return new SingletonSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a KeySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(KeySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

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
        }

        /// <summary>
        /// Translate a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(PropertySegment segment)
        {
            return new PropertySegmentTemplate(segment);
        }

        /// <summary>
        /// Handle a PropertySegment
        /// </summary>
        /// <param name="segment">the segment to handle</param>
        public override ODataSegmentTemplate Translate(PathTemplateSegment segment)
        {
            return new PathTemplateSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a OperationImportSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(OperationImportSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            if (segment.OperationImports.First().IsActionImport())
            {
                return new ActionImportSegmentTemplate(segment);
            }

            return new FunctionImportSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a OperationSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(OperationSegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            IEdmOperation operation = segment.Operations.First();
            if (operation.IsAction())
            {
                return new ActionSegmentTemplate(segment);
            }

            return new FunctionSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate an OpenPropertySegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(DynamicPathSegment segment)
        {
            return new DynamicSegmentTemplate(segment);
        }

        /// <summary>
        /// Visit a NavigationPropertyLinkSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(NavigationPropertyLinkSegment segment)
        {
            return new NavigationLinkSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a CountSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(CountSegment segment)
        {
            return CountSegmentTemplate.Instance;
        }

        /// <summary>
        /// Translate a ValueSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(ValueSegment segment)
        {
            return new ValueSegmentTemplate(segment);
        }

        /// <summary>
        /// Translate a BatchSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(BatchSegment segment)
        {
            throw new ODataException(Error.Format(SRResources.TargetKindNotImplemented, "ODataPathSegment", "BatchSegment"));
        }

        /// <summary>
        /// Translate a MetadataSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(MetadataSegment segment)
        {
            return MetadataSegmentTemplate.Instance;
        }

        /// <summary>
        /// Translate a BatchReferenceSegment
        /// </summary>
        /// <param name="segment">the segment to Translate</param>
        /// <returns>Translated the path segment template.</returns>
        public override ODataSegmentTemplate Translate(BatchReferenceSegment segment)
        {
            throw new ODataException(Error.Format(SRResources.TargetKindNotImplemented, "ODataPathSegment", "BatchReferenceSegment"));
        }
    }
}
