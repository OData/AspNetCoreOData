// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="DeltaDeletedResource{T}" /> allows and tracks changes to a delta deleted resource.
    /// </summary>
    public class DeltaDeletedResource<T> : Delta<T>, IDeltaDeletedResource where T: class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        public DeltaDeletedResource()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaDeletedResource{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived entity type or complex type for which the changes would be tracked.
        /// <paramref name="structuralType"/> should be assignable to instances of <typeparamref name="T"/>.
        /// </param>
        public DeltaDeletedResource(Type structuralType)
            : base(structuralType)
        {
        }

        /// <inheritdoc />
        public Uri Id { get; set; }

        /// <inheritdoc />
        public DeltaDeletedEntryReason? Reason { get; set; }

        /// <inheritdoc />
        public override DeltaKind Kind => DeltaKind.DeltaDeletedResource;
    }
}