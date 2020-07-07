// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts.Annotations
{
    /// <summary>
    /// This annotation indicates the mapping from an <see cref="IEdmOperation"/> to a <see cref="String"/>.
    /// The <see cref="IEdmOperation"/> is a bound action/function and the <see cref="String"/> is the
    /// entity set name given by user to indicate the entity set returned from this action/function.
    /// </summary>
    public class ReturnedEntitySetAnnotation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entitySetName"></param>
        public ReturnedEntitySetAnnotation(string entitySetName)
        {
            if (String.IsNullOrEmpty(entitySetName))
            {
                throw new ArgumentNullException(nameof(entitySetName));
            }

            EntitySetName = entitySetName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string EntitySetName { get; }
    }
}
