// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.ModelBuilder.Conventions
{
    /// <summary>
    ///  Represents a set of conventions used to build an Edm model.
    /// </summary>
    public interface IODataModelConventionSetBuilder
    {
        /// <summary>
        /// Builds and returns the convention set to use.
        /// </summary>
        /// <returns> The convention set to use. </returns>
        ODataModelConventionSet Build();
    }
}
