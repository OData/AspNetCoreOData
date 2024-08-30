//-----------------------------------------------------------------------------
// <copyright file="ODataActionParameters.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;

namespace Microsoft.AspNetCore.OData.Formatter;

/// <summary>
/// ActionPayload holds the Parameter names and values provided by a client in a POST request
/// to invoke a particular Action. The Parameter values are stored in the dictionary keyed using the Parameter name.
/// </summary>
[NonValidatingParameterBinding]
[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "ODataActionParameters is more appropriate here.")]
public class ODataActionParameters : Dictionary<string, object>
{
}
