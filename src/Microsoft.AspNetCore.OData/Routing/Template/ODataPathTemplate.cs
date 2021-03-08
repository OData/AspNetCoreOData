// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a path template that could contains a list of <see cref="ODataSegmentTemplate"/>.
    /// </summary>
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "ODataPathTemplate is ok.")]
    public class ODataPathTemplate : List<ODataSegmentTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segment templates for the path.</param>
        public ODataPathTemplate(params ODataSegmentTemplate[] segments)
            : base(segments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segment templates for the path.</param>
        public ODataPathTemplate(IEnumerable<ODataSegmentTemplate> segments)
            : base(segments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathTemplate" /> class.
        /// </summary>
        /// <param name="segments">The path segments for the path.</param>
        public ODataPathTemplate(IList<ODataSegmentTemplate> segments)
            : base(segments)
        {
        }

        /// <summary>
        /// Generates all templates for the given <see cref="ODataPathTemplate"/>.
        /// All templates mean:
        /// 1) for key segment, we have key in parenthesis and key as segment.
        /// 2) for bound function/action segment, we have qualified function call and unqualified function call.
        /// All of such might be based on route options.
        /// </summary>
        /// <param name="options">The route options.</param>
        /// <returns>All path template.</returns>
        public virtual IEnumerable<string> GetTemplates(ODataRouteOptions options = null)
        {
            options = options ?? ODataRouteOptions.Default;

            IList<StringBuilder> templates = new List<StringBuilder>
            {
                new StringBuilder()
            };

            for (int index = 0; index < Count; index++)
            {
                ODataSegmentTemplate segment = this[index];

                if (segment.Kind == ODataSegmentKind.Key)
                {
                    // for key segment, if it's single key, let's add key as segment template also
                    // otherwise, we only add the key in parenthesis template.
                    KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                    templates = AppendKeyTemplate(templates, keySg, options);
                    continue;
                }

                if (index != 0)
                {
                    templates = CombinateTemplate(templates, "/");
                }

                // create =>  ~.../navigation/{key}/$ref
                if (segment.Kind == ODataSegmentKind.NavigationLink)
                {
                    NavigationLinkSegmentTemplate navigationLinkSegment = (NavigationLinkSegmentTemplate)segment;
                    if (index == Count - 1)
                    {
                        // we don't have the other segment
                        templates = CombinateTemplates(templates, $"{navigationLinkSegment.Segment.NavigationProperty.Name}/$ref");
                    }
                    else
                    {
                        ODataSegmentTemplate nextSegment = this[index + 1];
                        if (nextSegment.Kind == ODataSegmentKind.Key)
                        {
                            // append "navigation property"
                            templates = CombinateTemplates(templates, navigationLinkSegment.Segment.NavigationProperty.Name);

                            // append "key"
                            KeySegmentTemplate keySg = nextSegment as KeySegmentTemplate;
                            templates = AppendKeyTemplate(templates, keySg, options);

                            // append $ref
                            templates = CombinateTemplates(templates, "/$ref");
                            index++; // skip the key segment after $ref.
                        }
                        else
                        {
                            templates = CombinateTemplates(templates, $"{navigationLinkSegment.Segment.NavigationProperty.Name}/$ref");
                        }
                    }

                    continue;
                }

                if (segment.Kind == ODataSegmentKind.Action)
                {
                    ActionSegmentTemplate action = (ActionSegmentTemplate)segment;
                    templates = AppendActionTemplate(templates, action, options);
                }
                else if (segment.Kind == ODataSegmentKind.Function)
                {
                    FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;
                    templates = AppendFunctionTemplate(templates, function, options);
                }
                else
                {
                    templates = CombinateTemplate(templates, segment.Literal);
                }
            }

            return templates.Select(t => t.ToString());
        }

        private static IList<StringBuilder> AppendKeyTemplate(IList<StringBuilder> templates, KeySegmentTemplate segment, ODataRouteOptions options)
        {
            Contract.Assert(segment != null);
            Contract.Assert(options != null);

            if (options.EnableKeyInParenthesis && options.EnableKeyAsSegment)
            {
                return CombinateTemplates(templates, $"({segment.Literal})", $"/{segment.Literal}");
            }
            else if (options.EnableKeyInParenthesis)
            {
                return CombinateTemplate(templates, $"({segment.Literal})");
            }
            else if (options.EnableKeyAsSegment)
            {
                return CombinateTemplate(templates, $"/{segment.Literal}");
            }
            else
            {
                throw new ODataException(SRResources.RouteOptionDisabledKeySegment);
            }
        }

        private static IList<StringBuilder> AppendActionTemplate(IList<StringBuilder> templates, ActionSegmentTemplate segment, ODataRouteOptions options)
        {
            Contract.Assert(segment != null);
            Contract.Assert(options != null);

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplates(templates, segment.Action.FullName(), segment.Action.Name);
            }
            else if (options.EnableQualifiedOperationCall)
            {
                return CombinateTemplate(templates, segment.Action.FullName());
            }
            else if (options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplate(templates, segment.Action.Name);
            }
            else
            {
                throw new ODataException(Error.Format(SRResources.RouteOptionDisabledOperationSegment, "action"));
            }
        }

        private static IList<StringBuilder> AppendFunctionTemplate(IList<StringBuilder> templates, FunctionSegmentTemplate segment, ODataRouteOptions options)
        {
            Contract.Assert(segment != null);
            Contract.Assert(options != null);

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplates(templates, segment.Literal, segment.UnqualifiedIdentifier);
            }
            else if (options.EnableQualifiedOperationCall)
            {
                return CombinateTemplate(templates, segment.Literal);
            }
            else if (options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplate(templates, segment.UnqualifiedIdentifier);
            }
            else
            {
                throw new ODataException(Error.Format(SRResources.RouteOptionDisabledOperationSegment, "function"));
            }
        }

        /// <summary>
        /// Combinates the next template to the existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplate">The nexte template.</param>
        /// <returns>The templates.</returns>
        private static IList<StringBuilder> CombinateTemplate(IList<StringBuilder> templates, string nextTemplate)
        {
            Contract.Assert(templates != null);

            foreach (StringBuilder sb in templates)
            {
                sb.Append(nextTemplate);
            }

            return templates;
        }

        /// <summary>
        /// Combinates the next templates with existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplates">The next templates.</param>
        /// <returns>The new templates.</returns>
        private static IList<StringBuilder> CombinateTemplates(IList<StringBuilder> templates, params string[] nextTemplates)
        {
            Contract.Assert(templates != null);

            IList<StringBuilder> newList = new List<StringBuilder>(templates.Count * nextTemplates.Length);

            foreach (StringBuilder sb in templates)
            {
                string oldTemplate = sb.ToString();
                foreach (string newTemplate in nextTemplates)
                {
                    newList.Add(new StringBuilder(oldTemplate).Append(newTemplate));
                }
            }

            return newList;
        }
    }
}
