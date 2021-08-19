//-----------------------------------------------------------------------------
// <copyright file="BindingPathHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal class BindingPathHelper
    {
        public static bool MatchBindingPath(IEdmPathExpression bindingPath, IList<ODataSegmentTemplate> parsedSegments)
        {
            List<string> paths = bindingPath.PathSegments.ToList();

            // If binding path only includes navigation property name, it matches.
            if (paths.Count == 1)
            {
                return true;
            }

            int pathIndex = paths.Count - 2; // Skip the last segment which is navigation property name.

            // Match from tail to head.
            for (int segmentIndex = parsedSegments.Count - 1; segmentIndex >= 0; segmentIndex--)
            {
                ODataSegmentTemplate segment = parsedSegments[segmentIndex];

                PropertySegmentTemplate propertySegment = segment as PropertySegmentTemplate;
                NavigationSegmentTemplate navigationSegment = segment as NavigationSegmentTemplate;
                // Containment navigation property or complex property in binding path.
                if (propertySegment != null ||
                    (navigationSegment != null && navigationSegment.Segment.NavigationSource is IEdmContainedEntitySet))
                {
                    string pathName = propertySegment != null ?
                        propertySegment.Property.Name :
                        navigationSegment.NavigationProperty.Name;

                    if (pathIndex < 0 || string.CompareOrdinal(paths[pathIndex], pathName) != 0)
                    {
                        return false;
                    }

                    pathIndex--;
                }
                else if (segment is CastSegmentTemplate)
                {
                    CastSegmentTemplate cast = (CastSegmentTemplate)segment;
                    // May need match type if the binding path contains type cast.
                    if (pathIndex >= 0 && paths[pathIndex].Contains(".", System.StringComparison.Ordinal))
                    {
                        if (string.CompareOrdinal(paths[pathIndex], cast.CastType.AsElementType().FullTypeName()) != 0)
                        {
                            return false;
                        }

                        pathIndex--;
                    }
                }
                else if (segment is EntitySetSegmentTemplate
                        || segment is SingletonSegmentTemplate
                        || navigationSegment != null)
                {
                    // Containment navigation property in first if statement for NavigationPropertySegment.
                    break;
                }
            }

            // Return true if all the segments in binding path have been matched.
            return pathIndex == -1 ? true : false;
        }
    }
}
