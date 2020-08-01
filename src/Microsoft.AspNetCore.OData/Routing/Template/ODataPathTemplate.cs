// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a path template that could contains a list of <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    public class ODataPathTemplate
    {
        private string _template;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segment templates for the path.</param>
        public ODataPathTemplate(params ODataSegmentTemplate[] segments)
            : this((IList<ODataSegmentTemplate>)segments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segment templates for the path.</param>
        public ODataPathTemplate(IEnumerable<ODataSegmentTemplate> segments)
            : this(segments.ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPathTemplate(IList<ODataSegmentTemplate> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            Segments = new ReadOnlyCollection<ODataSegmentTemplate>(segments);
        }

        /// <summary>
        /// Gets the path segments for the OData path.
        /// </summary>
        public ReadOnlyCollection<ODataSegmentTemplate> Segments { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ODataPath Translate(ODataTemplateTranslateContext context)
        {
            // calculate every time
            IList<ODataPathSegment> oSegments = new List<ODataPathSegment>();
            IEdmNavigationSource previousNavigationSource = null;
            foreach (var segment in Segments)
            {
                ODataPathSegment odataSegment = segment.Translate(context);
                if (odataSegment == null)
                {
                    return null;
                }

                oSegments.Add(odataSegment);
                previousNavigationSource = GetTargetNavigationSource(previousNavigationSource, odataSegment);
            }

            return new ODataPath(oSegments);
        }

        private static IEdmNavigationSource GetTargetNavigationSource(IEdmNavigationSource previous, ODataPathSegment segment)
        {
            if (segment == null)
            {
                return null;
            }

            EntitySetSegment entitySet = segment as EntitySetSegment;
            if (entitySet != null)
            {
                return entitySet.EntitySet;
            }

            SingletonSegment singleton = segment as SingletonSegment;
            if (singleton != null)
            {
                return singleton.Singleton;
            }

            TypeSegment cast = segment as TypeSegment;
            if (cast != null)
            {
                return cast.NavigationSource;
            }

            KeySegment key = segment as KeySegment;
            if (key != null)
            {
                return key.NavigationSource;
            }

            OperationSegment opertion = segment as OperationSegment;
            if (opertion != null)
            {
                return opertion.EntitySet;
            }

            OperationImportSegment import = segment as OperationImportSegment;
            if (import != null)
            {
                return import.EntitySet;
            }

            PropertySegment property = segment as PropertySegment;
            if (property != null)
            {
                return previous; // for property, return the previous, or return null????
            }

            MetadataSegment metadata = segment as MetadataSegment;
            if (metadata != null)
            {
                return null;
            }

            CountSegment count = segment as CountSegment;
            if (count != null)
            {
                return null;
            }

            OperationImportSegment operationImport = segment as OperationImportSegment;
            if (operationImport != null)
            {
                return null;
            }

            NavigationPropertySegment navigationPropertySegment = segment as NavigationPropertySegment;
            if (navigationPropertySegment != null)
            {
                return navigationPropertySegment.NavigationSource;
            }

            throw new Exception("Not supported segment in endpoint routing convention!");
        }


        /// <summary>
        /// 
        /// </summary>
        public string Template
        {
            get
            {
                if (_template == null)
                {
                    _template = CalculateTemplate();
                }

                return _template;
            }
        }

        private string CalculateTemplate()
        {
            int index = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var segment in Segments)
            {
                KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                if (keySg != null)
                {
                    sb.Append("(");
                    sb.Append(segment.Literal);
                    sb.Append(")");
                }
                else
                {
                    if (index != 0)
                    {
                        sb.Append("/");
                    }
                    sb.Append(segment.Literal);
                    index++;
                }
            }

            return sb.ToString();
        }
    }
}
