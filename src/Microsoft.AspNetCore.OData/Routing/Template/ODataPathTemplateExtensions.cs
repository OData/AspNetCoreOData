// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// The extension methods for <see cref="ODataPathTemplate"/>
    /// </summary>
    public static class ODataPathTemplateExtensions
    {
        /// <summary>
        /// Generates all templates for the given <see cref="ODataPathTemplate"/>.
        /// All templates mean:
        /// 1) for key segment, we have key in parenthesis & key as segment.
        /// 2) for bound function segment, we have qualified function call & unqualified function call.
        /// </summary>
        /// <param name="path">The given path template.</param>
        /// <returns>All path templates.</returns>
        public static IEnumerable<string> GetTemplates(this ODataPathTemplate path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            IList<StringBuilder> templates = new List<StringBuilder>
            {
                new StringBuilder()
            };

            int count = path.Segments.Count;
            for (int index = 0; index < count; index++)
            {
                ODataSegmentTemplate segment = path.Segments[index];

                if (segment.Kind == ODataSegmentKind.Key)
                {
                    // for key segment, if it's single key, let's add key as segment template also
                    // otherwise, we only add the key in parenthesis template.
                    KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                    if (keySg.Count == 1)
                    {
                        templates = CombinateTemplates(templates, "(" + segment.Literal + ")", "/" + segment.Literal);
                    }
                    else
                    {
                        templates = CombinateTemplate(templates, "(" + segment.Literal + ")");
                    }

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
                    if (index == count - 1)
                    {
                        // we don't have the other segment
                        templates = CombinateTemplates(templates, $"{navigationLinkSegment.Segment.NavigationProperty.Name}/$ref");
                    }
                    else
                    {
                        ODataSegmentTemplate nextSegment = path.Segments[index + 1];
                        if (nextSegment.Kind == ODataSegmentKind.Key)
                        {
                            // append "navigation property"
                            templates = CombinateTemplates(templates, navigationLinkSegment.Segment.NavigationProperty.Name);

                            // append "key"
                            KeySegmentTemplate keySg = nextSegment as KeySegmentTemplate;
                            if (keySg.Count == 1)
                            {
                                templates = CombinateTemplates(templates, "(" + nextSegment.Literal + ")", "/" + nextSegment.Literal);
                            }
                            else
                            {
                                templates = CombinateTemplate(templates, "(" + nextSegment.Literal + ")");
                            }

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
                    templates = CombinateTemplates(templates, action.Action.FullName(), action.Action.Name);
                }
                else if (segment.Kind == ODataSegmentKind.Function)
                {
                    FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;
                    templates = CombinateTemplates(templates, function.Literal, function.UnqualifiedIdentifier);
                }
                else
                {
                    templates = CombinateTemplate(templates, segment.Literal);
                }
            }

            return templates.Select(t => t.ToString());
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
            Contract.Assert(nextTemplate != null);

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

        #region BackupTheUnusedCodes
#if false
        /// <summary>
        /// Generates all templates for an input function.
        /// </summary>
        /// <param name="edmFunction">The input function.</param>
        /// <returns>All templates.</returns>
        internal static IList<string> GenerateFunctionTemplates(this IEdmFunction edmFunction)
        {
            if (edmFunction == null)
            {
                throw new ArgumentNullException(nameof(edmFunction));
            }

            // split parameters
            (var fixes, var optionals) = SplitParameters(edmFunction);

            // gets all combinations of the optional parameters.
            Stack<IEdmOptionalParameter> current = new Stack<IEdmOptionalParameter>();
            IList<IEdmOptionalParameter[]> full = new List<IEdmOptionalParameter[]>();
            int length = optionals.Count;
            if (optionals.Count > 0)
            {
                Traveral(optionals.ToArray(), 0, length, current, full);
            }

            IList<string> functionNameLists = new List<string>();
            if (edmFunction.IsBound)
            {
                functionNameLists.Add(edmFunction.FullName());
                functionNameLists.Add(edmFunction.Name);
            }
            else
            {
                functionNameLists.Add(edmFunction.Name);
            }

            StringBuilder sb = new StringBuilder();
            IList<string> functionLiterals = new List<string>();
            foreach (var fullName in functionNameLists)
            {
                sb.Clear();
                sb.Append(fullName).Append("(");

                bool isFirst = true;
                foreach (var fixParam in fixes)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }

                    sb.Append($"{fixParam.Name}={{{fixParam.Name}}}");
                }

                if (full.Any())
                {
                    string pathSofar = sb.ToString();
                    foreach (var optional in full)
                    {
                        sb.Clear();
                        foreach (var item in optional)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                sb.Append(",");
                            }

                            sb.Append($"{item.Name}={{{item.Name}}}");
                        }
                        sb.Append(")");
                        functionLiterals.Add(pathSofar + sb.ToString());
                    }
                }
                else
                {
                    sb.Append(")");
                    functionLiterals.Add(sb.ToString());
                }
            }

            return functionLiterals;
        }

        /// <summary>
        /// Traveral the array to output the whole combination.
        /// </summary>
        /// <param name="optionals">The input array</param>
        /// <param name="start">The current start index.</param>
        /// <param name="end">The end index.</param>
        /// <param name="current">The current iteration stack</param>
        /// <param name="full">The whole combination.</param>
        private static void Traveral(IEdmOptionalParameter[] optionals, int start, int end,
            Stack<IEdmOptionalParameter> current, IList<IEdmOptionalParameter[]> full)
        {
            if (start == end)
            {
                full.Add(current.Reverse().ToArray());
                return;
            }

            Traveral(optionals, start + 1, end, current, full);

            current.Push(optionals[start]);

            Traveral(optionals, start + 1, end, current, full);

            current.Pop();
        }

        /// <summary>
        /// Splits the parameters into parts, one is the required parameters, the other is optional parameters.
        /// </summary>
        /// <param name="edmFunction">The input function.</param>
        /// <returns></returns>
        private static (IList<IEdmOperationParameter>, IList<IEdmOptionalParameter>) SplitParameters(IEdmFunction edmFunction)
        {
            IList<IEdmOperationParameter> fixes = new List<IEdmOperationParameter>();
            IList<IEdmOptionalParameter> optionals = new List<IEdmOptionalParameter>();
            int skip = edmFunction.IsBound ? 1 : 0;
            foreach (var parameter in edmFunction.Parameters.Skip(skip))
            {
                IEdmOptionalParameter optional = parameter as IEdmOptionalParameter;
                if (optional != null)
                {
                    optionals.Add(optional);
                }
                else
                {
                    fixes.Add(parameter);
                }
            }

            return (fixes, optionals);
        }
#endif
        #endregion
    }
}
