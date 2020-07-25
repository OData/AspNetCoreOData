// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        public bool KeyAsSegment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ODataPathTemplate Clone()
        {
            IList<ODataSegmentTemplate> newSegmes = new List<ODataSegmentTemplate>(Segments);
            return new ODataPathTemplate(newSegmes)
            {
                KeyAsSegment = this.KeyAsSegment
            };
        }

        /// <summary>
        /// Gets the all templates supported in this path.
        /// </summary>
        /// <returns>The supported templates.</returns>
        public IList<string> GetTemplates()
        {
            bool canKeyAsSegment = CanKeyAsSegment();

            // TODO: shall we support the optional parameter??
            bool canUnqualifiedCall = Segments.Any(s => s.Kind == ODataSegmentKind.Function || s.Kind == ODataSegmentKind.Action);

            IList<string> templates = new List<string>();
            templates.Add(GetTemplate(false, unqualifiedCall: false));

            if (canKeyAsSegment)
            {
                templates.Add(GetTemplate(true, unqualifiedCall: false));
            }

            if (canUnqualifiedCall)
            {
                templates.Add(GetTemplate(false, unqualifiedCall: true));
                if (canKeyAsSegment)
                {
                    templates.Add(GetTemplate(true, unqualifiedCall: true));
                }
            }

            return templates;
        }

        private bool CanKeyAsSegment()
        {
            foreach (var segment in Segments)
            {
                if (segment.Kind != ODataSegmentKind.Key)
                {
                    continue;
                }

                // if existing a key segment with 1 key, we can support the whole path using key as segment
                KeySegmentTemplate keySegment = (KeySegmentTemplate)segment;
                if (keySegment.Count == 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyAsSegment"></param>
        /// <param name="unqualifiedCall"></param>
        /// <returns></returns>
        private string GetTemplate(bool keyAsSegment, bool unqualifiedCall)
        {
            int index = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var segment in Segments)
            {
                if (segment.Kind == ODataSegmentKind.Key)
                {
                    KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                    if (keyAsSegment && keySg.Count == 1)
                    {
                        sb.Append("/");
                        sb.Append(segment.Literal);
                    }
                    else
                    {
                        sb.Append("(");
                        sb.Append(segment.Literal);
                        sb.Append(")");
                    }
                }
                else
                {
                    if (index != 0)
                    {
                        sb.Append("/");
                    }
                    index++;

                    if (segment.Kind == ODataSegmentKind.Function)
                    {
                        FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;
                        if (unqualifiedCall)
                        {
                            sb.Append(function.UnqualifiedIdentifier);
                        }
                        else
                        {
                            sb.Append(function.Literal);
                        }
                    }
                    else if (segment.Kind == ODataSegmentKind.Action)
                    {
                        ActionSegmentTemplate action = (ActionSegmentTemplate)segment;
                        if (unqualifiedCall)
                        {
                            sb.Append(action.Action.Name);
                        }
                        else
                        {
                            sb.Append(action.Action.FullName());
                        }
                    }
                    else
                    {
                        sb.Append(segment.Literal);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ODataPath Translate(ODataSegmentTemplateTranslateContext context)
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
                    if (KeyAsSegment)
                    {
                        sb.Append("/");
                        sb.Append(segment.Literal);
                    }
                    else
                    {
                        sb.Append("(");
                        sb.Append(segment.Literal);
                        sb.Append(")");
                    }
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
