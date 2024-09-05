//-----------------------------------------------------------------------------
// <copyright file="IMediaTypeMappingCollection.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Formatter.MediaType;

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
