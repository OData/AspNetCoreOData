// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType
{
    /// <summary>
    /// An interface that defines a property to access a collection of <see cref="MediaTypeMapping"/> objects.
    /// </summary>
    interface IMediaTypeMappingCollection
    {
        /// <summary>
        /// Gets a collection of <see cref="MediaTypeMapping"/> objects.
        /// </summary>
        ICollection<MediaTypeMapping> MediaTypeMappings { get; }
    }
}
