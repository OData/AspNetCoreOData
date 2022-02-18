﻿//-----------------------------------------------------------------------------
// <copyright file="AutoSelectExpandHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Annotations;
using Microsoft.OData.ModelBuilder.Config;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal static class AutoSelectExpandHelper
    {
        #region Auto Select and Expand Test
        /// <summary>
        /// Tests whether there are auto select properties.
        /// So far, we only test one depth for auto select, shall we go through the deeper depth?
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The type from value or from path.</param>
        /// <param name="property">The property from path, it can be null.</param>
        /// <returns>true if the structured type has auto select properties; otherwise false.</returns>
        public static bool HasAutoSelectProperty(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            List<IEdmStructuredType> structuredTypes = new List<IEdmStructuredType>();
            structuredTypes.Add(structuredType);
            structuredTypes.AddRange(edmModel.FindAllDerivedTypes(structuredType));

            foreach (IEdmStructuredType edmStructuredType in structuredTypes)
            {
                // for top type, let's retrieve its properties and the properties from base type of top type if has.
                // for derived type, let's retrieve the declared properties.
                IEnumerable<IEdmStructuralProperty> properties = edmStructuredType == structuredType
                        ? edmStructuredType.StructuralProperties()
                        : edmStructuredType.DeclaredStructuralProperties();

                foreach (IEdmStructuralProperty subProperty in properties)
                {
                    if (IsAutoSelect(subProperty, property, edmStructuredType, edmModel))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tests whether there are auto expand properties.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="property">The property from path, it can be null.</param>
        /// <returns>true if the structured type has auto expand properties; otherwise false.</returns>
        public static bool HasAutoExpandProperty(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            return edmModel.HasAutoExpandProperty(structuredType, property, new HashSet<IEdmStructuredType>());
        }

        private static bool HasAutoExpandProperty(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty pathProperty, ISet<IEdmStructuredType> visited)
        {
            if (visited.Contains(structuredType))
            {
                return false;
            }
            visited.Add(structuredType);

            List<IEdmStructuredType> structuredTypes = new List<IEdmStructuredType>();
            structuredTypes.Add(structuredType);
            structuredTypes.AddRange(edmModel.FindAllDerivedTypes(structuredType));

            foreach (IEdmStructuredType edmStructuredType in structuredTypes)
            {
                // for top type, let's retrieve its properties and the properties from base type of top type if has.
                // for derived type, let's retrieve the declared properties.
                IEnumerable<IEdmProperty> properties = edmStructuredType == structuredType
                        ? edmStructuredType.Properties()
                        : edmStructuredType.DeclaredProperties;

                foreach (IEdmProperty property in properties)
                {
                    switch (property.PropertyKind)
                    {
                        case EdmPropertyKind.Structural:
                            IEdmStructuralProperty structuralProperty = (IEdmStructuralProperty)property;
                            IEdmTypeReference typeRef = property.Type.GetElementTypeOrSelf();
                            if (typeRef.IsComplex() && edmModel.CanExpand(typeRef.AsComplex().ComplexDefinition(), structuralProperty))
                            {
                                IEdmStructuredType subStrucutredType = typeRef.AsStructured().StructuredDefinition();
                                if (edmModel.HasAutoExpandProperty(subStrucutredType, structuralProperty, visited))
                                {
                                    return true;
                                }
                            }
                            break;

                        case EdmPropertyKind.Navigation:
                            IEdmNavigationProperty navigationProperty = (IEdmNavigationProperty)property;
                            if (IsAutoExpand(navigationProperty, pathProperty, edmStructuredType, edmModel))
                            {
                                return true; // find an auto-expand navigation property path
                            }
                            break;
                    }
                }
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Gets the auto select paths.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="pathProperty">The property from path, it can be null.</param>
        /// <param name="querySettings">The query settings.</param>
        /// <returns>The auto select paths.</returns>
        public static IList<SelectModelPath> GetAutoSelectPaths(this IEdmModel edmModel, IEdmStructuredType structuredType,
            IEdmProperty pathProperty, ModelBoundQuerySettings querySettings = null)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            List<SelectModelPath> autoSelectProperties = new List<SelectModelPath>();

            List<IEdmStructuredType> structuredTypes = new List<IEdmStructuredType>();
            structuredTypes.Add(structuredType);
            structuredTypes.AddRange(edmModel.FindAllDerivedTypes(structuredType));

            foreach (IEdmStructuredType edmStructuredType in structuredTypes)
            {
                // for top type, let's retrieve its properties and the properties from base type of top type if has.
                // for derived type, let's retrieve the declared properties.
                IEnumerable<IEdmStructuralProperty> properties = (edmStructuredType == structuredType) ?
                    edmStructuredType.StructuralProperties() :
                    properties = edmStructuredType.DeclaredStructuralProperties();

                foreach (IEdmStructuralProperty property in properties)
                {
                    if (IsAutoSelect(property, pathProperty, edmStructuredType, edmModel, querySettings))
                    {
                        if (edmStructuredType == structuredType)
                        {
                            autoSelectProperties.Add(new SelectModelPath(new[] { property }));
                        }
                        else
                        {
                            autoSelectProperties.Add(new SelectModelPath(new IEdmElement[] { edmStructuredType, property }));
                        }
                    }
                }
            }

            return autoSelectProperties;
        }

        /// <summary>
        /// Gets the auto expand paths.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="structuredType">The Edm structured type.</param>
        /// <param name="property">The property starting from, it can be null.</param>
        /// <param name="isSelectPresent">Is $select presented.</param>
        /// <param name="querySettings">The query settings.</param>
        /// <returns>The auto expand paths.</returns>
        public static IList<ExpandModelPath> GetAutoExpandPaths(this IEdmModel edmModel, IEdmStructuredType structuredType,
            IEdmProperty property, bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            Stack<IEdmElement> nodes = new Stack<IEdmElement>();
            ISet<IEdmStructuredType> visited = new HashSet<IEdmStructuredType>();
            IList<ExpandModelPath> results = new List<ExpandModelPath>();

            // type and property from path is higher priority
            edmModel.GetAutoExpandPaths(structuredType, property, nodes, visited, results, isSelectPresent, querySettings);

            Contract.Assert(nodes.Count == 0);
            return results;
        }

        public static bool IsAutoExpand(IEdmProperty navigationProperty,
            IEdmProperty pathProperty, IEdmStructuredType pathStructuredType, IEdmModel edmModel,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            QueryableRestrictionsAnnotation annotation = EdmHelpers.GetPropertyRestrictions(navigationProperty, edmModel);
            if (annotation != null && annotation.Restrictions.AutoExpand)
            {
                return !annotation.Restrictions.DisableAutoExpandWhenSelectIsPresent || !isSelectPresent;
            }

            if (querySettings == null)
            {
                querySettings = edmModel.GetModelBoundQuerySettings(pathProperty, pathStructuredType);
            }

            if (querySettings != null && querySettings.IsAutomaticExpand(navigationProperty.Name))
            {
                return true;
            }

            return false;
        }

        public static bool IsAutoSelect(IEdmProperty property, IEdmProperty pathProperty,
            IEdmStructuredType pathStructuredType, IEdmModel edmModel, ModelBoundQuerySettings querySettings = null)
        {
            if (querySettings == null)
            {
                querySettings = edmModel.GetModelBoundQuerySettings(pathProperty, pathStructuredType);
            }

            if (querySettings != null && querySettings.IsAutomaticSelect(property.Name))
            {
                return true;
            }

            return false;
        }

        private static bool CanExpand(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            // first for back-compability, check the queryable restriction
            QueryableRestrictionsAnnotation annotation = EdmHelpers.GetPropertyRestrictions(property, edmModel);
            if (annotation != null && annotation.Restrictions.NotExpandable)
            {
                return false;
            }

            ModelBoundQuerySettings settings = edmModel.GetModelBoundQuerySettingsOrNull(structuredType, property);
            if (settings != null && !settings.Expandable(property.Name))
            {
                return false;
            }

            return true;
        }

        private static void GetAutoExpandPaths(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty pathProperty,
            Stack<IEdmElement> nodes, ISet<IEdmStructuredType> visited, IList<ExpandModelPath> results,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            if (visited.Contains(structuredType))
            {
                return;
            }
            visited.Add(structuredType);

            List<IEdmStructuredType> structuredTypes = new List<IEdmStructuredType>();
            structuredTypes.Add(structuredType);
            structuredTypes.AddRange(edmModel.FindAllDerivedTypes(structuredType));

            foreach (IEdmStructuredType edmStructuredType in structuredTypes)
            {
                IEnumerable<IEdmProperty> properties;

                if (edmStructuredType == structuredType)
                {
                    // for base type, let's retrieve its properties and the properties from base type of base type if have.
                    properties = edmStructuredType.Properties();
                }
                else
                {
                    // for derived type, let's retrieve the declared properties.
                    properties = edmStructuredType.DeclaredProperties;
                    nodes.Push(edmStructuredType); // add a type cast for derived type
                }

                foreach (IEdmProperty property in properties)
                {
                    switch (property.PropertyKind)
                    {
                        case EdmPropertyKind.Structural:
                            IEdmStructuralProperty structuralProperty = (IEdmStructuralProperty)property;
                            IEdmTypeReference typeRef = property.Type.GetElementTypeOrSelf();
                            if (typeRef.IsComplex() && edmModel.CanExpand(typeRef.AsComplex().ComplexDefinition(), structuralProperty))
                            {
                                IEdmStructuredType subStructuredType = typeRef.AsStructured().StructuredDefinition();

                                nodes.Push(structuralProperty);

                                edmModel.GetAutoExpandPaths(subStructuredType, structuralProperty, nodes, visited, results, isSelectPresent, querySettings);

                                nodes.Pop();
                            }
                            break;

                        case EdmPropertyKind.Navigation:
                            IEdmNavigationProperty navigationProperty = (IEdmNavigationProperty)property;
                            if (IsAutoExpand(navigationProperty, pathProperty, edmStructuredType, edmModel, isSelectPresent, querySettings))
                            {
                                nodes.Push(navigationProperty);
                                results.Add(new ExpandModelPath(nodes.Reverse())); // found  an auto-expand navigation property path
                                nodes.Pop();
                            }
                            break;
                    }
                }

                if (edmStructuredType != structuredType)
                {
                    nodes.Pop(); // pop the type cast for derived type
                }
            }
        }
    }
}
