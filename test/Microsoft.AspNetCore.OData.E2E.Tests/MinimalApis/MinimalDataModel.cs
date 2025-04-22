//-----------------------------------------------------------------------------
// <copyright file="MinimalDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MinimalApis;

public class MiniTodo
{
    public int Id { get; set; }

    public string Owner { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsDone { get; set; }

    public IList<MiniTask> Tasks { get; set; }
}

public class MiniTask
{
    public int Id { get; set; }

    public string Description { get; set; }

    public DateOnly Created { get; set; }

    public bool IsComplete { get; set; }

    public int Priority { get; set; }
}

