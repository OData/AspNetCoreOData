// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.ModelBuilder
{
    internal interface IEdmTypeConvention : IODataModelConvention
    {
        void Apply(IEdmTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model);
    }
}
