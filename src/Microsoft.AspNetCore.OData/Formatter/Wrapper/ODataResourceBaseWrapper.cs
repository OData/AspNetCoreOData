// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Formatter.Wrapper
{
    /// <summary>
    /// Encapsulates an <see cref="ODataResourceBase"/> and its nested resource infos.
    /// </summary>
    public abstract class ODataResourceBaseWrapper : ODataItemWrapper
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceBaseWrapper"/>.
        /// </summary>
        protected ODataResourceBaseWrapper()
        {
            NestedResourceInfos = new List<ODataNestedResourceInfoWrapper>();
        }

        /// <summary>
        /// Gets the inner nested resource infos.
        /// </summary>
        public IList<ODataNestedResourceInfoWrapper> NestedResourceInfos { get; }
    }
}
