// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// </summary>
    public static class ODataPathExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllTemplates(this ODataPathTemplate path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            int index = 0;
            IList<StringBuilder> templates = new List<StringBuilder>
            {
                new StringBuilder()
            };

            foreach (ODataSegmentTemplate segment in path.Segments)
            {
                if (segment.Kind == ODataSegmentKind.Key)
                {
                    KeySegmentTemplate keySg = segment as KeySegmentTemplate;
                    if (keySg.Count == 1)
                    {
                        IList<string> keyTemplates = new List<string>
                        {
                            "(" + segment.Literal + ")",
                            "/" + segment.Literal
                        };

                        templates = Append(templates, keyTemplates);
                    }
                    else
                    {
                        string keyTemplate = "(" + segment.Literal + ")";
                        foreach (StringBuilder sb in templates)
                        {
                            sb.Append(keyTemplate);
                        }
                    }

                    continue;
                }

                if (index != 0)
                {
                    foreach (StringBuilder sb in templates)
                    {
                        sb.Append("/");
                    }
                }
                index++;

                if (segment.Kind == ODataSegmentKind.Action)
                {
                    ActionSegmentTemplate action = (ActionSegmentTemplate)segment;

                    IList<string> actioniTemplates = new List<string>
                    {
                        action.Action.FullName(),
                        action.Action.Name
                    };

                    templates = Append(templates, actioniTemplates);
                }
                else if (segment.Kind == ODataSegmentKind.Function)
                {
                    FunctionSegmentTemplate function = (FunctionSegmentTemplate)segment;

                    IList<string> functionTemplates = function.Function.GenerateFunctionPath();

                    templates = Append(templates, functionTemplates);
                }
                else if (segment.Kind == ODataSegmentKind.FunctionImport)
                {
                    FunctionImportSegmentTemplate functionImport = (FunctionImportSegmentTemplate)segment;

                    IList<string> functionTemplates = functionImport.FunctionImport.Function.GenerateFunctionPath();

                    templates = Append(templates, functionTemplates);
                }
                else
                {
                    foreach (StringBuilder sb in templates)
                    {
                        sb.Append(segment.Literal);
                    }
                }
            }

            return templates.Select(t => t.ToString());
        }

        private static IList<StringBuilder> Append(IList<StringBuilder> templates, IList<string> nextTemplates)
        {
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

        private static IList<StringBuilder> Append(IList<StringBuilder> templates, string nextTemplate)
        {
            foreach (StringBuilder sb in templates)
            {
                sb.Append(nextTemplate);
            }

            return templates;
        }


        private static StringBuilder[] DoubleTemplates(StringBuilder[] templates)
        {
            int length = templates.Length;
            StringBuilder[] newTemplates = new StringBuilder[length * 2];
            for (int i = 0; i < length; i++)
            {
                newTemplates[i] = templates[i];
            }

            for (int i = length; i < 2 * length; i++)
            {
                newTemplates[i] = new StringBuilder(templates[i - length].ToString());
            }

            return newTemplates;
        }

        public static IList<string> GenerateFunctionPath(this IEdmFunction edmFunction)
        {
            (var fixes, var optionals) = ProcessFunction(edmFunction);

            Stack<IEdmOptionalParameter> current = new Stack<IEdmOptionalParameter>();
            IList<HashSet<IEdmOptionalParameter>> full = new List<HashSet<IEdmOptionalParameter>>();
            int length = optionals.Count;
            Generator(optionals.ToArray(), 0, length, current, full);

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

        public static void Generator(IEdmOptionalParameter[] optionals, int start, int end, Stack<IEdmOptionalParameter> current,
            IList<HashSet<IEdmOptionalParameter>> full)
        {
            if (start == end)
            {
                full.Add(current.ToHashSet());
                return;
            }

            Generator(optionals, start + 1, end, current, full);

            current.Push(optionals[start]);

            Generator(optionals, start + 1, end, current, full);

            current.Pop();
        }

        public static (HashSet<IEdmOperationParameter>, HashSet<IEdmOptionalParameter>) ProcessFunction(IEdmFunction edmFunction)
        {
            HashSet<IEdmOperationParameter> fixes = new HashSet<IEdmOperationParameter>();
            HashSet<IEdmOptionalParameter> optionals = new HashSet<IEdmOptionalParameter>();
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
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEdmType GetEdmType(this ODataPath path)
        {
            if (path == null)
            {
                return null;
            }

            ODataPathSegment lastSegment = path.LastSegment;

            EntitySetSegment entitySet = lastSegment as EntitySetSegment;
            if (entitySet != null)
            {
                return entitySet.EdmType;
            }


            // TODO
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEdmNavigationSource GetNavigationSource(this ODataPath path)
        {
            if (path == null)
            {
                return null;
            }

            ODataPathSegment lastSegment = path.LastSegment;

            EntitySetSegment entitySet = lastSegment as EntitySetSegment;
            if (entitySet != null)
            {
                return entitySet.EntitySet;
            }


            // TODO
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public static string GetPathString(this IList<ODataPathSegment> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            foreach (var segment in segments)
            {
                segment.HandleWith(handler);
            }

            return handler.PathLiteral;
        }
    }
}
