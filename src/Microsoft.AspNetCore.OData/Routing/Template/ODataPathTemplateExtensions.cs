// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// The extension methods for <see cref="ODataPathTemplate"/>
    /// </summary>
    public static class ODataPathTemplateExtensions
    {
        private struct TemplateInfo
        {
            public StringBuilder Template;

            public StringBuilder Display;
        }

        /// <summary>
        /// Generates all templates for the given <see cref="ODataPathTemplate"/>.
        /// All templates mean:
        /// 1) for key segment, we have key in parenthesis and key as segment.
        /// 2) for bound function segment, we have qualified function call and unqualified function call.
        /// </summary>
        /// <param name="path">The given path template.</param>
        /// <param name="options">The route options.</param>
        /// <returns>All path template and its display name..</returns>
        public static IEnumerable<(string, string)> GetTemplates(this ODataPathTemplate path, ODataRouteOptions options = null)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            options = options ?? ODataRouteOptions.Default;

            IList<TemplateInfo> templates = new List<TemplateInfo>
            {
                new TemplateInfo
                {
                    Template = new StringBuilder(),
                    Display = new StringBuilder()
                }
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
                    templates = AppendKeyTemplate(templates, keySg, options);
                    continue;
                }

                if (index != 0)
                {
                    templates = CombinateTemplate(templates, ("/", "/"));
                }

                // create =>  ~.../navigation/{key}/$ref
                if (segment.Kind == ODataSegmentKind.NavigationLink)
                {
                    NavigationLinkSegmentTemplate navigationLinkSegment = (NavigationLinkSegmentTemplate)segment;
                    if (index == count - 1)
                    {
                        // we don't have the other segment
                        string refTemp = $"{navigationLinkSegment.Segment.NavigationProperty.Name}/$ref";
                        templates = CombinateTemplates(templates, (refTemp, refTemp));
                    }
                    else
                    {
                        ODataSegmentTemplate nextSegment = path.Segments[index + 1];
                        if (nextSegment.Kind == ODataSegmentKind.Key)
                        {
                            // append "navigation property"
                            string navTemp = navigationLinkSegment.Segment.NavigationProperty.Name;
                            templates = CombinateTemplates(templates, (navTemp, navTemp));

                            // append "key"
                            KeySegmentTemplate keySg = nextSegment as KeySegmentTemplate;
                            templates = AppendKeyTemplate(templates, keySg, options);

                            // append $ref
                            templates = CombinateTemplates(templates, ("/$ref", "/$ref"));
                            index++; // skip the key segment after $ref.
                        }
                        else
                        {
                            string refTemp = $"{navigationLinkSegment.Segment.NavigationProperty.Name}/$ref";
                            templates = CombinateTemplates(templates, (refTemp, refTemp));
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
                    templates = CombinateTemplate(templates, (segment.Literal, segment.Literal));
                }
            }

            foreach (var template in templates)
            {
                yield return (template.Template.ToString(), template.Display.ToString());
            }

            //return templates.Select(t => t.ToString());
        }

        private static IList<TemplateInfo> AppendKeyTemplate(IList<TemplateInfo> templates, KeySegmentTemplate segment, ODataRouteOptions options)
        {
            Contract.Assert(segment != null);
            Contract.Assert(options != null);

            string displayName = GetDisplayName(segment);
            if (options.EnableKeyInParenthesis && options.EnableKeyAsSegment)
            {
                return CombinateTemplates(templates, ($"({segment.Literal})", $"({displayName})"), ($"/{segment.Literal}", $"/{displayName}"));
            }
            else if (options.EnableKeyInParenthesis)
            {
                return CombinateTemplate(templates, ($"({segment.Literal})", $"({displayName})"));
            }
            else if (options.EnableKeyAsSegment)
            {
                return CombinateTemplate(templates, ($"/{segment.Literal}", $"/{displayName}"));
            }
            else
            {
                throw new ODataException(SRResources.RouteOptionDisabledKeySegment);
            }
        }

        private static string GetDisplayName(this KeySegmentTemplate segment)
        {
            Contract.Assert(segment != null);

            if (segment.KeyMappings.Count == 1)
            {
                return $"{{{segment.KeyMappings.First().Value}}}";
            }

            return string.Join(",", segment.KeyMappings.Select(a => $"{a.Key}={{{a.Value}}}"));
        }

        private static IList<TemplateInfo> AppendActionTemplate(IList<TemplateInfo> templates, ActionSegmentTemplate segment, ODataRouteOptions options)
        {
            Contract.Assert(segment != null);
            Contract.Assert(options != null);

            string fullName = segment.Action.FullName();
            string name = segment.Action.Name;

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplates(templates, (fullName, fullName), (name, name));
            }
            else if (options.EnableQualifiedOperationCall)
            {
                return CombinateTemplate(templates, (fullName, fullName));
            }
            else if (options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplate(templates, (name, name));
            }
            else
            {
                throw new ODataException(Error.Format(SRResources.RouteOptionDisabledOperationSegment, "action"));
            }
        }

        private static IList<TemplateInfo> AppendFunctionTemplate(IList<TemplateInfo> templates, FunctionSegmentTemplate segment, ODataRouteOptions options)
        {
            Contract.Assert(segment != null);
            Contract.Assert(options != null);

            string qualified = segment.GetDisplayName(true);
            string unqulified = segment.GetDisplayName(false);

            if (options.EnableQualifiedOperationCall && options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplates(templates, (segment.Literal, qualified), (segment.UnqualifiedIdentifier, unqulified));
            }
            else if (options.EnableQualifiedOperationCall)
            {
                return CombinateTemplate(templates, (segment.Literal, qualified));
            }
            else if (options.EnableUnqualifiedOperationCall)
            {
                return CombinateTemplate(templates, (segment.UnqualifiedIdentifier, unqulified));
            }
            else
            {
                throw new ODataException(Error.Format(SRResources.RouteOptionDisabledOperationSegment, "function"));
            }
        }

        private static string GetDisplayName(this FunctionSegmentTemplate segment, bool qualified)
        {
            Contract.Assert(segment != null);

            string parameters = "(" + string.Join(",", segment.ParameterMappings.Select(a => $"{a.Key}={{{a.Value}}}")) + ")";
            if (qualified)
            {
                return segment.Function.FullName() + parameters;
            }
            else
            {
                return segment.Function.Name + parameters;
            }
        }

        /// <summary>
        /// Combinates the next template to the existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplate">The nexte template.</param>
        /// <returns>The templates.</returns>
        private static IList<TemplateInfo> CombinateTemplate(IList<TemplateInfo> templates, (string, string) nextTemplate)
        {
            Contract.Assert(templates != null);

            foreach (TemplateInfo sb in templates)
            {
                sb.Template.Append(nextTemplate.Item1);
                sb.Display.Append(nextTemplate.Item2);
            }

            return templates;
        }

        /// <summary>
        /// Combinates the next templates with existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplates">The next templates.</param>
        /// <returns>The new templates.</returns>
        private static IList<TemplateInfo> CombinateTemplates(IList<TemplateInfo> templates, params (string, string)[] nextTemplates)
        {
            Contract.Assert(templates != null);

            IList<TemplateInfo> newList = new List<TemplateInfo>(templates.Count * nextTemplates.Length);

            foreach (TemplateInfo sb in templates)
            {
                string oldTemplate = sb.Template.ToString();
                string oldDisplay = sb.Display.ToString();
                foreach ((string, string) newTemplate in nextTemplates)
                {
                    TemplateInfo templateInfo = new TemplateInfo();
                    templateInfo.Template = new StringBuilder(oldTemplate).Append(newTemplate.Item1);
                    templateInfo.Display = new StringBuilder(oldDisplay).Append(newTemplate.Item2);
                    newList.Add(templateInfo);
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
