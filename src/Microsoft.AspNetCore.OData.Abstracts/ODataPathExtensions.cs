// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Abstracts
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
    }
}
