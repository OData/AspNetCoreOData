//-----------------------------------------------------------------------------
// <copyright file="IODataModelConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Edm;

/// <summary>
/// Defines a contract used to apply extra logic on the model builder.
/// </summary>
public interface IODataModelConfiguration
{
    /// <summary>
    /// Applies model configurations using the provided builder.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
    /// <param name="clrType">The top CLR type.</param>
    /// <returns>The model builder or a totally new builder to use.</returns>
    ODataModelBuilder Apply(HttpContext context, ODataModelBuilder builder, Type clrType);
}

