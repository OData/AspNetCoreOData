//-----------------------------------------------------------------------------
// <copyright file="ODataPathExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing;

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

        for (int i = path.Count - 1; i >= 0; --i)
        {
            ODataPathSegment segment = path[i];
            if (segment is EntitySetSegment entitySetSegment)
            {
                return entitySetSegment.EntitySet;
            }

            if (segment is KeySegment keySegment)
            {
                return keySegment.NavigationSource;
            }

            if (segment is NavigationPropertyLinkSegment navigationPropertyLinkSegment)
            {
                return navigationPropertyLinkSegment.NavigationSource;
            }

            if (segment is NavigationPropertySegment navigationPropertySegment)
            {
                return navigationPropertySegment.NavigationSource;
            }

            if (segment is OperationImportSegment operationImportSegment)
            {
                return operationImportSegment.EntitySet;
            }

            if (segment is OperationSegment operationSegment)
            {
                return operationSegment.EntitySet;
            }

            if (segment is SingletonSegment singleton)
            {
                return singleton.Singleton;
            }

            if (segment is TypeSegment typeSegment)
            {
                return typeSegment.NavigationSource;
            }

            if (segment is PropertySegment)
            {
                continue;
            }

            return null;
        }

        return null;
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

    internal static bool IsUntypedPropertyPath(this ODataPath path)
    {
        if (path == null)
        {
            return false;
        }

        // TODO: do we need take the type cast into consideration?
        if (path.LastSegment is PropertySegment propertySegment)
        {
            return propertySegment.Property.Type.IsUntypedOrCollectionUntyped();
        }

        // TODO, Shall we take the dynamic property path segment into consideration?
        return false;
    }

    /// <summary>
    /// Gets the property and structured type from <see cref="ODataPath"/>.
    /// TODO: The logic implenetation is not good and do need refactor it later.
    /// </summary>
    /// <param name="path">The OData path.</param>
    /// <returns>The property, structured type and the name.</returns>
    internal static (IEdmProperty, IEdmStructuredType, string) GetPropertyAndStructuredTypeFromPath(this ODataPath path)
    {
        if (path == null)
        {
            return (null, null, string.Empty);
        }

        IEdmStructuredType structuredType = null;
        string typeCast = string.Empty;
        IEnumerable<ODataPathSegment> reverseSegments = path.Reverse();
        foreach (var segment in reverseSegments)
        {
            if (segment is NavigationPropertySegment navigationPathSegment)
            {
                IEdmProperty property = navigationPathSegment.NavigationProperty;
                if (structuredType == null)
                {
                    structuredType = navigationPathSegment.NavigationProperty.ToEntityType();
                }

                string name = navigationPathSegment.NavigationProperty.Name + typeCast;
                return (property, structuredType, name);
            }

            if (segment is OperationSegment operationSegment)
            {
                if (structuredType == null)
                {
                    structuredType = operationSegment.EdmType.AsElementType() as IEdmStructuredType;
                }

                string name = operationSegment.Operations.First().FullName() + typeCast;
                return (null, structuredType, name);
            }

            if (segment is PropertySegment propertyAccessPathSegment)
            {
                IEdmProperty property = propertyAccessPathSegment.Property;
                if (structuredType == null)
                {
                    structuredType = property.Type.GetElementType() as IEdmStructuredType;
                }

                string name = property.Name + typeCast;
                return (property, structuredType, name);
            }

            if (segment is EntitySetSegment entitySetSegment)
            {
                if (structuredType == null)
                {
                    structuredType = entitySetSegment.EntitySet.EntityType;
                }

                string name = entitySetSegment.EntitySet.Name + typeCast;
                return (null, structuredType, name);
            }

            if (segment is SingletonSegment singletonSegment)
            {
                if (structuredType == null)
                {
                    structuredType = singletonSegment.Singleton.EntityType;
                }

                string name = singletonSegment.Singleton.Name + typeCast;
                return (null, structuredType, name);
            }

            if (segment is OperationImportSegment operationImportSegment)
            {
                IEdmOperationImport operationImport = operationImportSegment.OperationImports.First();
                IEdmTypeReference edmType = operationImport.Operation.ReturnType;
                if (edmType == null)
                {
                    return (null, null, operationImport.Name);
                }

                return (null, edmType.Definition.AsElementType() as IEdmStructuredType, operationImport.Name);
            }

            if (segment is TypeSegment typeSegment)
            {
                structuredType = typeSegment.EdmType.AsElementType() as IEdmStructuredType;
                typeCast = "/" + structuredType;
            }
            else if (segment is KeySegment || segment is CountSegment)
            {
                // do nothing, just go to next segment, what about if meet OperationSegment?
            }
            else
            {
                // if we meet any other segments, just return (null, null, string.Empty);
                break;
            }
        }

        return (null, null, string.Empty);
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
