// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTemplates(this ODataPathTemplate path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            IList<StringBuilder> templates = new List<StringBuilder>
            {
                new StringBuilder()
            };

            int index = 0;
            foreach (ODataSegmentTemplate segment in path.Segments)
            {
                if (segment.Kind == ODataSegmentKind.Key)
                {
                    KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                    if (keySg.Count == 1)
                    {
                        templates = CombinateTemplates(templates, "(" + segment.Literal + ")", "/" + segment.Literal);
                    }
                    else
                    {
                        templates = CombinateTemplate(templates, "(" + segment.Literal + ")");
                    }

                    index++;
                    continue;
                }

                if (index != 0)
                {
                    templates = CombinateTemplate(templates, "/");
                }
                index++;

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
        /// Generates all templates for an input function.
        /// </summary>
        /// <param name="segment">The input function.</param>
        /// <returns>All templates.</returns>
        public static IList<FunctionSegmentTemplate> GenerateFunctionSegments(this FunctionSegmentTemplate segment)
        {
            if (segment == null)
            {
                throw new ArgumentNullException(nameof(segment));
            }

            // split parameters
            (var fixes, var optionals) = SplitParameters(segment.Function);

            // gets all combinations of the optional parameters.
            Stack<IEdmOptionalParameter> current = new Stack<IEdmOptionalParameter>();
            IList<IEdmOptionalParameter[]> full = new List<IEdmOptionalParameter[]>();
            int length = optionals.Count;
            if (optionals.Count > 0)
            {
                Traveral(optionals.ToArray(), 0, length, current, full);
            }

            IList<FunctionSegmentTemplate> newList = new List<FunctionSegmentTemplate>();
            foreach (var optional in full)
            {
                ISet<string> requiredParameters = new HashSet<string>(fixes.Select(e => e.Name));
                foreach (var optionalParameter in optional)
                {
                    requiredParameters.Add(optionalParameter.Name);
                }

                newList.Add(new FunctionSegmentTemplate(segment.Function, segment.NavigationSource, requiredParameters));
            }

            return newList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<ODataPathTemplate> GetAllPaths(this ODataPathTemplate path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            IList<IList<ODataSegmentTemplate>> templates = new List<IList<ODataSegmentTemplate>>
            {
                new List<ODataSegmentTemplate>()
            };

            foreach (ODataSegmentTemplate segment in path.Segments)
            {
                if (segment.Kind == ODataSegmentKind.Function)
                {
                    FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;
                    IList<FunctionSegmentTemplate> functionTemplates = function.GenerateFunctionSegments();
                    templates = CombinateTemplates(templates, functionTemplates);
                }
                else if (segment.Kind == ODataSegmentKind.FunctionImport)
                {
                    //FunctionImportSegmentTemplate functionImport = (FunctionImportSegmentTemplate)segment;
                    //IList<string> functionTemplates = functionImport.FunctionImport.Function.GenerateFunctionTemplates();
                    //templates = CombinateTemplates(templates, functionTemplates);
                }
                else
                {
                    templates = CombinateTemplate(templates, segment);
                }
            }

            return templates.Select(e => new ODataPathTemplate(e));
        }

        /// <summary>
        /// Combinates the next template to the existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplate">The nexte template.</param>
        /// <returns>The templates.</returns>
        private static IList<IList<ODataSegmentTemplate>> CombinateTemplate(IList<IList<ODataSegmentTemplate>> templates, ODataSegmentTemplate nextTemplate)
        {
            Contract.Assert(templates != null);
            Contract.Assert(nextTemplate != null);

            foreach (IList<ODataSegmentTemplate> sb in templates)
            {
                sb.Add(nextTemplate);
            }

            return templates;
        }

        /// <summary>
        /// Combinates the next templates with existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplates">The nexte templates.</param>
        /// <returns>The new templates.</returns>
        private static IList<IList<ODataSegmentTemplate>> CombinateTemplates<T>(IList<IList<ODataSegmentTemplate>> templates, IList<T> nextTemplates)
            where T : ODataSegmentTemplate
        {
            Contract.Assert(templates != null);

            IList<IList<ODataSegmentTemplate>> newList = new List<IList<ODataSegmentTemplate>>(templates.Count * nextTemplates.Count);

            foreach (IList<ODataSegmentTemplate> sb in templates)
            {
                foreach (ODataSegmentTemplate newTemplate in nextTemplates)
                {
                    IList<ODataSegmentTemplate> newSb = new List<ODataSegmentTemplate>(sb);
                    newSb.Add(newTemplate);
                    newList.Add(newSb);
                }
            }

            return newList;
        }
        /// <summary>
        /// Gets the whole supported template belongs to a <see cref="ODataPathTemplate"/>.
        /// We supports:
        /// 1. Key as segment if it's single key (We doesn't consider the alternate key so far)
        /// 2. Unqualified function/action call
        /// 3. Optional parameters combination.
        /// </summary>
        /// <param name="path">The input path template.</param>
        /// <returns>The whole path template string.</returns>
        public static IEnumerable<string> GetAllTemplates(this ODataPathTemplate path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            IList<StringBuilder> templates = new List<StringBuilder>
            {
                new StringBuilder()
            };

            int index = 0;
            foreach (ODataSegmentTemplate segment in path.Segments)
            {
                if (segment.Kind == ODataSegmentKind.Key)
                {
                    KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                    if (keySg.Count == 1)
                    {
                        templates = CombinateTemplates(templates, "(" + segment.Literal + ")", "/" + segment.Literal);
                    }
                    else
                    {
                        templates = CombinateTemplate(templates, "(" + segment.Literal + ")");
                    }

                    index++;
                    continue;
                }

                if (index != 0)
                {
                    templates = CombinateTemplate(templates, "/");
                }
                index++;

                if (segment.Kind == ODataSegmentKind.Action)
                {
                    ActionSegmentTemplate action = (ActionSegmentTemplate)segment;
                    templates = CombinateTemplates(templates, action.Action.FullName(), action.Action.Name);
                }
                else if (segment.Kind == ODataSegmentKind.Function)
                {
                    FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;
                    IList<string> functionTemplates = function.Function.GenerateFunctionTemplates();
                    templates = CombinateTemplates(templates, functionTemplates);
                }
                else if (segment.Kind == ODataSegmentKind.FunctionImport)
                {
                    FunctionImportSegmentTemplate functionImport = (FunctionImportSegmentTemplate)segment;
                    IList<string> functionTemplates = functionImport.FunctionImport.Function.GenerateFunctionTemplates();
                    templates = CombinateTemplates(templates, functionTemplates);
                }
                else
                {
                    templates = CombinateTemplate(templates, segment.Literal);
                }
            }

            return templates.Select(t => t.ToString());
        }

        /// <summary>
        /// Generates all templates for an input function.
        /// </summary>
        /// <param name="edmFunction">The input function.</param>
        /// <returns>All templates.</returns>
        public static IList<string> GenerateFunctionTemplates(this IEdmFunction edmFunction)
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
        /// <param name="nextTemplates">The nexte templates.</param>
        /// <returns>The new templates.</returns>
        private static IList<StringBuilder> CombinateTemplates(IList<StringBuilder> templates, params string[] nextTemplates)
        {
            return CombinateTemplates(templates, new List<string>(nextTemplates));
        }

        /// <summary>
        /// Combinates the next templates with existing templates.
        /// </summary>
        /// <param name="templates">The existing templates.</param>
        /// <param name="nextTemplates">The nexte templates.</param>
        /// <returns>The new templates.</returns>
        private static IList<StringBuilder> CombinateTemplates(IList<StringBuilder> templates, IList<string> nextTemplates)
        {
            Contract.Assert(templates != null);

            IList<StringBuilder> newList = new List<StringBuilder>(templates.Count * nextTemplates.Count);

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
