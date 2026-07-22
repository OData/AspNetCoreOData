//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ApplyQueryValidation;

public class ApplyValidationItemsController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<ApplyValidationItem>> Get()
    {
        return Ok(ApplyValidationDataSource.Items);
    }
}

// Same data, but the endpoint restricts the function and arithmetic-operator allow-lists. This proves
// end-to-end that those ODataValidationSettings limits are enforced for $apply (groupby/aggregate/
// compute) and top-level $compute, not just for $filter.
public class RestrictedLimitItemsController : ODataController
{
    [EnableQuery(
        AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.Length,
        AllowedArithmeticOperators = AllowedArithmeticOperators.All & ~AllowedArithmeticOperators.Multiply)]
    public ActionResult<IEnumerable<ApplyValidationItem>> Get()
    {
        return Ok(ApplyValidationDataSource.Items);
    }
}
