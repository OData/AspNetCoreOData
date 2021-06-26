// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="DeltaSet{T}" /> allows and tracks changes to a delta resource set.
    /// </summary>
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "<Pending>")]
    [NonValidatingParameterBinding]
    public class DeltaSet<T> : Collection<IDeltaSetItem>, IDeltaSet, ITypedDelta where T : class
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
        /// <remarks>
        /// TODO: this functionality hasn't finished yet. We'd like to get more feedback about how to
        /// patch the deltaset to the original data source.
        /// </remarks>
        /// <param name="originalSet">The original set.</param>
        internal void Patch(IEnumerable originalSet)
        {
            if (originalSet == null)
            {
                throw Error.ArgumentNull(nameof(originalSet));
            }

            // TODO: work out the patch process
            foreach (IDeltaSetItem delta in this)
            {
                T originalObj = GetOriginal(delta, originalSet);

                switch (delta)
                {
                    case IDelta deltaResource:
                        IDeltaDeletedResource deltaDeleteResource = delta as IDeltaDeletedResource;
                        if (deltaDeleteResource != null)
                        {
                            // TODO: it's a delta deleted resource
                        }
                        else
                        {
                            // TODO: it's a normal (added, updated) resource
                        }
                        break;

                    case IDeltaDeletedLink deltaDeletedLink:
                        // TODO: a delta deleted link
                        break;

                    case IDeltaLink deltaLink:
                        // TODO: a delta added link
                        break;

                    default:
                        throw Error.InvalidOperation($"Unknow delta type {delta.GetType()}");
                }
            }
        }

        /// <summary>
        /// Find the related instance.
        /// </summary>
        /// <param name="deltaItem">The delta item.</param>
        /// <param name="originalSet">The original set.</param>
        /// <returns></returns>
        protected virtual T GetOriginal(IDeltaSetItem deltaItem, IEnumerable originalSet)
        {
            if (deltaItem == null)
            {
                throw Error.ArgumentNull(nameof(deltaItem));
            }

            if (originalSet == null)
            {
                throw Error.ArgumentNull(nameof(originalSet));
            }

            return null;
        }
    }
}