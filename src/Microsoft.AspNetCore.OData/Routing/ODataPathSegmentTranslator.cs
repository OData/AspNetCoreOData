//-----------------------------------------------------------------------------
// <copyright file="ODataPathSegmentTranslator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing;

/// <summary>
/// Translator the parameter alias, convert node, returned entity set into OData path segment.
/// </summary>
public class ODataPathSegmentTranslator : PathSegmentTranslator<ODataPathSegment>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parameterAliasNodes"></param>
    /// <returns></returns>
    public static SingleValueNode TranslateParameterAlias(
        SingleValueNode node,
        IDictionary<string, SingleValueNode> parameterAliasNodes)
    {
        if (node == null)
        {
            throw Error.ArgumentNull(nameof(node));
        }

        if (parameterAliasNodes == null)
        {
            throw Error.ArgumentNull(nameof(parameterAliasNodes));
        }

        ParameterAliasNode parameterAliasNode = node as ParameterAliasNode;

        if (parameterAliasNode == null)
        {
            return node;
        }

        SingleValueNode singleValueNode;

        if (parameterAliasNodes.TryGetValue(parameterAliasNode.Alias, out singleValueNode) &&
            singleValueNode != null)
        {
            if (singleValueNode is ParameterAliasNode)
            {
                singleValueNode = TranslateParameterAlias(singleValueNode, parameterAliasNodes);
            }

            return singleValueNode;
        }

        // Parameter alias value is assumed to be null if it is not found.
        // Do not need to translate the parameter alias node from the query string
        // because this method only deals with the parameter alias node mapping from ODL parser.
        return null;
    }
}
