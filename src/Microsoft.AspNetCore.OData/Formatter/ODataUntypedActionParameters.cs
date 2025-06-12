//-----------------------------------------------------------------------------
// <copyright file="ODataUntypedActionParameters.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Deltas;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Formatter;

/// <summary>
/// ActionPayload holds the Parameter names and values provided by a client in a POST request
/// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "ODataUntypedActionParameters is more appropriate here.")]
[NonValidatingParameterBinding]
public class ODataUntypedActionParameters : Dictionary<string, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataUntypedActionParameters"/> class.
    /// </summary>
    /// <param name="action">The OData action of this parameters.</param>
    public ODataUntypedActionParameters(IEdmAction action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Gets the OData action of this parameters.
    /// </summary>
    public IEdmAction Action { get; }

    /// <summary>
    /// Binds the <see cref="HttpContext"/> and <see cref="ParameterInfo"/> to generate the <see cref="Delta{T}"/>.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="parameter">The parameter info.</param>
    /// <returns>The built <see cref="ODataUntypedActionParameters"/></returns>
    public static async ValueTask<ODataUntypedActionParameters> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        return await context.BindODataParameterAsync<ODataUntypedActionParameters>(parameter);
    }
}
