// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    ///  Extension methods for <see cref="ODataPath"/>.
    /// </summary>
    public static class ODataPathExtensions
    {
        /// <summary>
        /// Gets a boolean value indicating whether the given path is a stream property path.
        /// </summary>
        /// <param name="path">The given odata path.</param>
        /// <returns>true/false</returns>
        public static bool IsStreamPropertyPath(this ODataPath path)
        {
            if (path == null)
            {
                return false;
            }

            PropertySegment propertySegment = path.LastSegment as PropertySegment;
            if (propertySegment == null)
            {
                return false;
            }

            IEdmTypeReference propertyType = propertySegment.Property.Type;

            // Edm.Stream, or a type definition whose underlying type is Edm.Stream,
            // cannot be used in collections or for non-binding parameters to functions or actions.
            // So, we don't need to test it but leave the codes here for awareness.
            //if (propertyType.IsCollection())
            //{
            //    propertyType = propertyType.AsCollection().ElementType();
            //}

            return propertyType.IsStream();
        }

        /// <summary>
        /// Computes the <see cref="IEdmType"/> of the resource identified by this <see cref="ODataPath"/>.
        /// </summary>
        /// <param name="path">Path to compute the type for.</param>
        /// <returns>The <see cref="IEdmType"/> of the resource, or null if the path does not identify a resource with a type.</returns>
        public static IEdmType GetEdmType(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            ODataPathSegment lastSegment = path.LastSegment;
            return lastSegment.EdmType;
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
                throw Error.ArgumentNull(nameof(path));
            }

            ODataPathNavigationSourceHandler handler = new ODataPathNavigationSourceHandler();
            path.WalkWith(handler);
            return handler.NavigationSource;
        }

        /// <summary>
        /// Get the string representation of <see cref="ODataPath"/> mainly translate Context Url path.
        /// </summary>
        /// <param name="path">Path to compute the set for.</param>
        /// <returns>The string representation of the Context Url path.</returns>
        public static string GetPathString(this ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull(nameof(path));
            }

            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            foreach (var segment in path)
            {
                segment.HandleWith(handler);
            }

            return handler.PathLiteral;
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
                throw Error.ArgumentNull(nameof(segments));
            }

            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            foreach (var segment in segments)
            {
                segment.HandleWith(handler);
            }

            return handler.PathLiteral;
        }

        #region BACKUP
#if false
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
#endif
        #endregion
    }
}
