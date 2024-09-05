//-----------------------------------------------------------------------------
// <copyright file="RegressionsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Regressions;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public int Age { get; set; }

    //Navigations
    [ForeignKey("Files")]
    public int? DataFileRef { get; set; }

    public virtual DataFile Files { get; set; }
}

public class DataFile
{
    [Key]
    public int FileId { get; set; }

    [Required]
    public string FileName { get; set; }
}
