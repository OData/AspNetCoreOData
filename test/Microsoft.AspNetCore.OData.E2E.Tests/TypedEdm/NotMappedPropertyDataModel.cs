//-----------------------------------------------------------------------------
// <copyright file="NotMappedPropertyDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.OData.E2E.Tests.TypedEdm;

/// <summary>
/// A CLR class backing an open EDM entity type. The <see cref="PasswordHash"/> property is
/// decorated with <see cref="NotMappedAttribute"/> so the OData convention builder excludes it
/// from the EDM model.
/// </summary>
public class UserAccount
{
    public int Id { get; set; }

    public string Name { get; set; }

    public IDictionary<string, object> DynamicProperties { get; set; }

    /// <summary>
    /// Excluded from the EDM model via <see cref="NotMappedAttribute"/>.
    /// </summary>
    [NotMapped]
    public string PasswordHash { get; set; }
}
