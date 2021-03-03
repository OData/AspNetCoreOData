// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="DeltaSet{T}" /> allows and tracks changes to a delta resource set.
    /// </summary>
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
    [NonValidatingParameterBinding]
    public class DeltaSet<T> : DeltaSet, ITypedDelta where T : class
    {
        /// <summary>
        /// Gets the actual type of the structural object for which the changes are tracked.
        /// </summary>
        public Type StructuredType => typeof(T);

        /// <summary>
        /// Gets the expected type of the entity for which the changes are tracked.
        /// </summary>
        public Type ExpectedClrType => typeof(T);

        /// <summary>
        ///  Overwrites the <paramref name="originalSet"/> entity with the changes tracked by this Delta resource set.
        /// </summary>
        /// <param name="originalSet">The original set.</param>
        public virtual void Patch(IEnumerable<T> originalSet)
        {
            if (originalSet == null)
            {
                throw Error.ArgumentNull(nameof(originalSet));
            }

            // TODO: work out the patch process
            foreach (IDeltaItem delta in this)
            {
                T originalObj = GetOriginal(delta, originalSet);

                switch (delta.Kind)
                {
                    case DeltaKind.DeltaResource:
                        // A Delta resource is a delta
                        IDelta deltaResource = (IDelta)delta;
                        break;

                    case DeltaKind.DeltaDeletedResource:
                        // A Delta deleted resource is a delta deleted resource
                        IDeltaDeletedResource deltaDeletedResource = (IDeltaDeletedResource)delta;
                        break;

                    case DeltaKind.DeltaDeletedLink:
                        IDeltaDeletedLink deltaDeletedLink = (IDeltaDeletedLink)delta;
                        break;

                    case DeltaKind.DeltaLink:
                        IDeltaLink deltaLink = (IDeltaLink)delta;
                        break;

                    case DeltaKind.Unknown:
                    default:
                        //throw Error.InvalidOperation(SRResources.CannotSetDynamicPropertyDictionary, propertyInfo.Name,
                        //    entity.GetType().FullName);
                        throw Error.InvalidOperation("Unknow delta kind");
                }
            }
        }

        /// <summary>
        /// Find the related instance.
        /// </summary>
        /// <param name="deltaItem"></param>
        /// <param name="originalSet"></param>
        /// <returns></returns>
        protected virtual T GetOriginal(IDeltaItem deltaItem, IEnumerable<T> originalSet)
        {
            return null;
        }
    }
}