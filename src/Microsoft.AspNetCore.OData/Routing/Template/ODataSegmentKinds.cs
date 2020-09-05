// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// The segment kind.
    /// </summary>
    public enum ODataSegmentKind
    {
        /// <summary>
        /// $metadata
        /// </summary>
        Metadata,

        /// <summary>
        /// Entity set segment.
        /// </summary>
        EntitySet,

        /// <summary>
        /// Singleton segment.
        /// </summary>
        Singleton,

        /// <summary>
        /// Key segment.
        /// </summary>
        Key,

        /// <summary>
        /// Type cast segment.
        /// </summary>
        Cast,

        /// <summary>
        /// Property segment.
        /// </summary>
        Property,

        /// <summary>
        /// Navigation property segment.
        /// </summary>
        Navigation,

        /// <summary>
        /// Bound function segment.
        /// </summary>
        Function,

        /// <summary>
        /// Bound action segment.
        /// </summary>
        Action,

        /// <summary>
        /// Function import segment.
        /// </summary>
        FunctionImport,

        /// <summary>
        /// Action import segment.
        /// </summary>
        ActionImport,

        /// <summary>
        /// $value segment.
        /// </summary>
        Value,

        /// <summary>
        /// $ref segment.
        /// </summary>
        Ref,

        /// <summary>
        /// $count segment.
        /// </summary>
        Count,

        /// <summary>
        /// Path segment.
        /// </summary>
        PathTemplate,

        /// <summary>
        /// Dynamic segment.
        /// </summary>
        Dynamic,
    }
}
