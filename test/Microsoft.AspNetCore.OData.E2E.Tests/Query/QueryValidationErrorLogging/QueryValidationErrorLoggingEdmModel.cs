//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLoggingEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationErrorLogging;

public class QueryValidationErrorLoggingEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<LoggingCustomer>("LoggingCustomers");
        builder.EntitySet<LoggingCustomer>("PlainCustomers");
        builder.EntitySet<LoggingCustomer>("OptOutCustomers");
        return builder.GetEdmModel();
    }
}
