//-----------------------------------------------------------------------------
// <copyright file="ODataPathTemplate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Routing.Template;

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
    /// Generates all templates for the given <see cref="ODataPathTemplate"/> using the given <see cref="ODataRouteOptions"/>.
    /// All templates mean:
    /// 1) for key segment, we have key in parenthesis and key as segment.
    /// 2) for bound function/action segment, we have qualified function call and unqualified function call.
    /// All of such might be based on route options.
    /// </summary>
    /// <param name="options">The route options.</param>
    /// <returns>All path templates.</returns>
    public virtual IEnumerable<string> GetTemplates(ODataRouteOptions options = null)
    {
        options = options ?? ODataRouteOptions.Default;

        Stack<string> stack = new Stack<string>();

        IList<string> templates = new List<string>();

        ProcessSegment(stack, 0, Count, templates, options);

        return templates;
    }

    private void ProcessSegment(Stack<string> stack, int index, int count, IList<string> templates, ODataRouteOptions options)
    {
        if (index == count)
        {
            string pathTemplate = string.Join("", stack.Reverse());
            if (pathTemplate.StartsWith('/'))
            {
                pathTemplate = pathTemplate.Substring(1); // remove the first "/"
            }

            templates.Add(pathTemplate);
            return;
        }

        ODataSegmentTemplate segment = this[index];

        foreach (string template in segment.GetTemplates(options))
        {
            stack.Push(template);

            ProcessSegment(stack, index + 1, count, templates, options);

            stack.Pop();
        }
    }
}
