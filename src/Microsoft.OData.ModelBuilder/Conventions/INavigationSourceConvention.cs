// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.OData.ModelBuilder.Conventions
{
    /// <summary>
    /// Convention to apply to <see cref="OperationConfiguration"/> instances in the model
    /// </summary>
    internal interface INavigationSourceConvention : IODataModelConvention
    {
        void Apply(NavigationSourceConfiguration configuration, ODataModelBuilder model);
    }
}
