//-----------------------------------------------------------------------------
// <copyright file="BulkOperationEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using static Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation.BulkOperationDataModel;

namespace Microsoft.AspNetCore.OData.E2E.Tests.BulkOperation;

internal class BulkOperationEdmModel
{
    public static IEdmModel GetConventionModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Employee>("Employees");
        builder.EntitySet<Friend>("Friends");
        builder.EntitySet<Order>("Orders");

        builder.Namespace = typeof(Employee).Namespace;
        builder.MaxDataServiceVersion = EdmConstants.EdmVersion401;
        builder.DataServiceVersion = EdmConstants.EdmVersion401;

        var edmModel = builder.GetEdmModel();

        return edmModel;
    }
}
