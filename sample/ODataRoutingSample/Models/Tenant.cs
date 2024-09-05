//-----------------------------------------------------------------------------
// <copyright file="Tenant.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ODataRoutingSample.Models;

public class DriverTenant
{
    [Key]
    public string tenantId { get; set; }

    public IList<DriverDevice> devices { get; set; }

    public IList<DriverFolder> folders { get; set; }

    public IList<DriverPage> pages { get; set; }
}

public class DriverDevice
{
    [Key]
    public string deviceId { get; set; }
}

public class DriverFolder
{
    [Key]
    public Guid folderId { get; set; }
}

public class DriverPage
{
    public int Id { get; set; }
}


public class TestEntity
{
    public int Id { get; set; }

    public HuntingQueryResults Query { get; set; }
}

public class HuntingQueryResults
{
    public IList<HuntingRowResult> Results { get; set; }
}

public class HuntingRowResult
{
    public IDictionary<string, object> DynamicProperties { get; set; }
}

