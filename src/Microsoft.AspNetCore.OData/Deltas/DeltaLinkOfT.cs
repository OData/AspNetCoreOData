// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Deltas
{
    /// <summary>
    /// <see cref="DeltaLink{T}" /> allows and tracks changes to delta added link.
    /// </summary>
    public class DeltaLink<T> : DeltaLinkBase<T>, IDeltaLink where T: class
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DeltaLink{T}"/>.
        /// </summary>
        public DeltaLink()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DeltaLink{T}"/>.
        /// </summary>
        /// <param name="structuralType">The derived structural type for which the changes would be tracked.</param>
        public DeltaLink(Type structuralType)
            : base(structuralType)
        {
        }

        /// <inheritdoc />
        public override DeltaKind Kind => DeltaKind.DeltaLink;
    }
}