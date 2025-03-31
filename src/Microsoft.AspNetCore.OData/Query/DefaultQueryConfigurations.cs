//-----------------------------------------------------------------------------
// <copyright file="DefaultQueryConfigurations.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.ModelBuilder.Config;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This class describes the default configurations to use during query composition.
/// </summary>
public class DefaultQueryConfigurations : DefaultQuerySettings
{
    // We will add other query settings, for example, $compute, $search here
    // In the next major release, we should remove the inheritance from 'DefaultQuerySettings'.

    /// <summary>
    /// Enable all query options.
    /// </summary>
    /// <returns></returns>
    public DefaultQueryConfigurations EnableAll()
    {
        EnableExpand = true;
        EnableSelect = true;
        EnableFilter = true;
        EnableOrderBy = true;
        EnableCount = true;
        EnableSkipToken = true;
        MaxTop = null;
        return this;
    }

    public DefaultQueryConfigurations UpdateAll(DefaultQueryConfigurations options)
    {
        EnableExpand = options.EnableExpand;
        EnableSelect = options.EnableSelect;
        EnableFilter = options.EnableFilter;
        EnableOrderBy = options.EnableOrderBy;
        EnableCount = options.EnableCount;
        EnableSkipToken = options.EnableSkipToken;
        MaxTop = options.MaxTop;
        return this;
    }
}
