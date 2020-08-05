// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Convention to apply to <see cref="OperationConfiguration"/> instances in the model
    /// </summary>
    internal interface IOperationConvention : IODataModelConvention
    {
        void Apply(OperationConfiguration configuration, ODataModelBuilder model);
    }
}
