//-----------------------------------------------------------------------------
// <copyright file="EdmModelLinkBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// Extension methods to set the link builder.
    /// </summary>
    public static class EdmModelLinkBuilderExtensions
    {
        /// <summary>
        /// Sets the ID link builder for the given <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="idLinkBuilder">The Id link builder.</param>
        public static void HasIdLink(this IEdmModel model, IEdmNavigationSource navigationSource, SelfLinkBuilder<Uri> idLinkBuilder)
        {
            NavigationSourceLinkBuilderAnnotation annotation = model.GetNavigationSourceLinkBuilder(navigationSource);
            Contract.Assert(annotation != null);
            annotation.IdLinkBuilder = idLinkBuilder;
        }

        /// <summary>
        /// Sets the Edit link builder for the given <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="editLinkBuilder">The Edit link builder.</param>
        public static void HasEditLink(this IEdmModel model, IEdmNavigationSource navigationSource, SelfLinkBuilder<Uri> editLinkBuilder)
        {
            NavigationSourceLinkBuilderAnnotation annotation = model.GetNavigationSourceLinkBuilder(navigationSource);
            Contract.Assert(annotation != null);
            annotation.EditLinkBuilder = editLinkBuilder;
        }

        /// <summary>
        /// Sets the Read link builder for the given <see cref="IEdmNavigationSource"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="readLinkBuilder">The Read link builder.</param>
        public static void HasReadLink(this IEdmModel model, IEdmNavigationSource navigationSource, SelfLinkBuilder<Uri> readLinkBuilder)
        {
            NavigationSourceLinkBuilderAnnotation annotation = model.GetNavigationSourceLinkBuilder(navigationSource);
            Contract.Assert(annotation != null);
            annotation.ReadLinkBuilder = readLinkBuilder;
        }

        /// <summary>
        /// Sets the navigation property link builder for the given <see cref="IEdmNavigationSource"/> and <see cref="IEdmNavigationProperty"/>.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <param name="linkBuilder">The navigation property link builder.</param>
        public static void HasNavigationPropertyLink(this IEdmModel model, IEdmNavigationSource navigationSource,
            IEdmNavigationProperty navigationProperty, NavigationLinkBuilder linkBuilder)
        {
            NavigationSourceLinkBuilderAnnotation annotation = model.GetNavigationSourceLinkBuilder(navigationSource);
            Contract.Assert(annotation != null);
            annotation.AddNavigationPropertyLinkBuilder(navigationProperty, linkBuilder);
        }

        /// <summary>
        /// Gets the <see cref="NavigationSourceLinkBuilderAnnotation"/> to be used while generating self and navigation
        /// links for the given navigation source.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the navigation source.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The <see cref="NavigationSourceLinkBuilderAnnotation"/> if set for the given the singleton; otherwise,
        /// a new <see cref="NavigationSourceLinkBuilderAnnotation"/> that generates URLs that follow OData URL conventions.
        /// </returns>
        public static NavigationSourceLinkBuilderAnnotation GetNavigationSourceLinkBuilder(this IEdmModel model,
            IEdmNavigationSource navigationSource)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            NavigationSourceLinkBuilderAnnotation annotation = model
                .GetAnnotationValue<NavigationSourceLinkBuilderAnnotation>(navigationSource);
            if (annotation == null)
            {
                // construct and set a navigation source link builder that follows OData URL conventions.
                annotation = new NavigationSourceLinkBuilderAnnotation(navigationSource, model);
                model.SetNavigationSourceLinkBuilder(navigationSource, annotation);
            }

            return annotation;
        }

        /// <summary>
        /// Sets the <see cref="NavigationSourceLinkBuilderAnnotation"/> to be used while generating self and navigation
        /// links for the given navigation source.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the navigation source.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="navigationSourceLinkBuilder">The <see cref="NavigationSourceLinkBuilderAnnotation"/> to set.</param>
        public static void SetNavigationSourceLinkBuilder(this IEdmModel model, IEdmNavigationSource navigationSource,
            NavigationSourceLinkBuilderAnnotation navigationSourceLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(navigationSource, navigationSourceLinkBuilder);
        }

        /// <summary>
        /// Gets the <see cref="OperationLinkBuilder"/> to be used while generating operation links for the given action.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the operation.</param>
        /// <param name="operation">The operation for which the link builder is needed.</param>
        /// <returns>The <see cref="OperationLinkBuilder"/> for the given operation if one is set; otherwise, a new
        /// <see cref="OperationLinkBuilder"/> that generates operation links following OData URL conventions.</returns>
        public static OperationLinkBuilder GetOperationLinkBuilder(this IEdmModel model, IEdmOperation operation)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (operation == null)
            {
                throw Error.ArgumentNull("operation");
            }

            OperationLinkBuilder linkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(operation);
            if (linkBuilder == null)
            {
                linkBuilder = GetDefaultOperationLinkBuilder(operation);
                model.SetOperationLinkBuilder(operation, linkBuilder);
            }

            return linkBuilder;
        }

        /// <summary>
        /// Sets the <see cref="OperationLinkBuilder"/> to be used for generating the OData operation link for the given operation.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="operation">The operation for which the operation link is to be generated.</param>
        /// <param name="operationLinkBuilder">The <see cref="OperationLinkBuilder"/> to set.</param>
        public static void SetOperationLinkBuilder(this IEdmModel model, IEdmOperation operation, OperationLinkBuilder operationLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(operation, operationLinkBuilder);
        }

        private static OperationLinkBuilder GetDefaultOperationLinkBuilder(IEdmOperation operation)
        {
            OperationLinkBuilder linkBuilder = null;
            if (operation.Parameters != null)
            {
                if (operation.Parameters.First().Type.IsEntity())
                {
                    if (operation is IEdmAction)
                    {
                        linkBuilder = new OperationLinkBuilder(
                            (ResourceContext resourceContext) =>
                                resourceContext.GenerateActionLink(operation), followsConventions: true);
                    }
                    else
                    {
                        linkBuilder = new OperationLinkBuilder(
                            (ResourceContext resourceContext) =>
                                resourceContext.GenerateFunctionLink(operation), followsConventions: true);
                    }
                }
                else if (operation.Parameters.First().Type.IsCollection())
                {
                    if (operation is IEdmAction)
                    {
                        linkBuilder =
                            new OperationLinkBuilder(
                                (ResourceSetContext reseourceSetContext) =>
                                    reseourceSetContext.GenerateActionLink(operation), followsConventions: true);
                    }
                    else
                    {
                        linkBuilder =
                            new OperationLinkBuilder(
                                (ResourceSetContext reseourceSetContext) =>
                                    reseourceSetContext.GenerateFunctionLink(operation), followsConventions: true);
                    }
                }
            }

            return linkBuilder;
        }
    }
}
