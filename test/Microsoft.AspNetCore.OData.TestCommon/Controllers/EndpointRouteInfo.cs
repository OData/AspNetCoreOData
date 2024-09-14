//-----------------------------------------------------------------------------
// <copyright file="EndpointRouteInfo.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.TestCommon;

public class EndpointRouteInfo
{
    public string ControllerFullName { get; set; }

    public string ActionFullName { get; set; }

    public string HttpMethods { get; set; }

    public string Template { get; set; }

    public bool IsODataRoute { get; set; }
}
