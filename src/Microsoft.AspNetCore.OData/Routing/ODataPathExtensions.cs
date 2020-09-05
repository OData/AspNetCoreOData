// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    ///  Extension methods for <see cref="ODataPath"/>.
    /// </summary>
    public static class ODataPathExtensions
    {
        /// <summary>
        /// Computes the <see cref="IEdmType"/> of the resource identified by this <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="path">Path to compute the type for.</param>
        /// <returns>The <see cref="IEdmType"/> of the resource, or null if the path does not identify a resource with a type.</returns>
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
        /// Computes the <see cref="IEdmNavigationSource"/> of the resource identified by this <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="path">Path to compute the set for.</param>
        /// <returns>The <see cref="IEdmNavigationSource"/> of the resource, or null if the path does not identify a resource that is part of a set.</returns>
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

            SingletonSegment singleton = lastSegment as SingletonSegment;
            if (singleton != null)
            {
                return singleton.Singleton;
            }


            // TODO
            return null;
        }

        /// <summary>
        /// Get the string representation of <see cref="ODataPath"/> mainly translate Context Url path.
        /// </summary>
        /// <param name="segments">The path segments.</param>
        /// <returns>The string representation of the Context Url path.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathTemplatesegment"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TranslatePathTemplateSegment(this PathTemplateSegment pathTemplatesegment, out string value)
        {
            if (pathTemplatesegment == null)
            {
                throw Error.ArgumentNull("pathTemplatesegment");
            }

            string pathTemplateSegmentLiteralText = pathTemplatesegment.LiteralText;
            if (pathTemplateSegmentLiteralText == null)
            {
                throw new ODataException(Error.Format(SRResources.InvalidAttributeRoutingTemplateSegment, string.Empty));
            }

            if (pathTemplateSegmentLiteralText.StartsWith("{", StringComparison.Ordinal)
                && pathTemplateSegmentLiteralText.EndsWith("}", StringComparison.Ordinal))
            {
                string[] keyValuePair = pathTemplateSegmentLiteralText.Substring(1,
                    pathTemplateSegmentLiteralText.Length - 2).Split(':');
                if (keyValuePair.Length != 2)
                {
                    throw new ODataException(Error.Format(
                        SRResources.InvalidAttributeRoutingTemplateSegment,
                        pathTemplateSegmentLiteralText));
                }
                value = "{" + keyValuePair[0] + "}";
                return keyValuePair[1];
            }

            value = string.Empty;
            return string.Empty;
        }
    }
}
