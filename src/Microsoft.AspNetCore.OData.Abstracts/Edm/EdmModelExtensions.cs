// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts.Edm
{
    /// <summary>
    /// 
    /// </summary>
    public static class EdmModelExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmAction> GetAvailableActions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, false).OfType<IEdmAction>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmFunction> GetAvailableFunctions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, false).OfType<IEdmFunction>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmOperation> GetAvailableOperationsBoundToCollection(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <param name="boundToCollection"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmOperation> GetAvailableOperations(this IEdmModel model, IEdmEntityType entityType, bool boundToCollection = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            BindableOperationFinder annotation = model.GetAnnotationValue<BindableOperationFinder>(model);
            if (annotation == null)
            {
                annotation = new BindableOperationFinder(model);
                model.SetAnnotationValue(model, annotation);
            }

            if (boundToCollection)
            {
                return annotation.FindOperationsBoundToCollection(entityType);
            }
            else
            {
                return annotation.FindOperations(entityType);
            }
        }
    }
}