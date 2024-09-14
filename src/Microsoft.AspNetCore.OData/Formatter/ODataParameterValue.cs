//-----------------------------------------------------------------------------
// <copyright file="ODataParameterValue.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter;

/// <summary>
/// OData parameter value used for function parameter binding.
/// </summary>
public class ODataParameterValue
{
    /// <summary>
    /// This prefix is used to identify parameters in [FromODataUri] binding scenario.
    /// </summary>
    public const string ParameterValuePrefix = "DF908045-6922-46A0-82F2-2F6E7F43D1B1_";

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataParameterValue"/> class.
    /// </summary>
    /// <param name="paramValue">The parameter value.</param>
    /// <param name="paramType">The parameter type.</param>
    public ODataParameterValue(object paramValue, IEdmTypeReference paramType)
    {
        if (paramType == null)
        {
            throw new ArgumentNullException(nameof(paramType));
        }

        Value = paramValue;
        EdmType = paramType;
    }

    /// <summary>
    /// Gets the parameter type.
    /// </summary>
    public IEdmTypeReference EdmType { get; }

    /// <summary>
    /// Gets the parameter value.
    /// </summary>
    public object Value { get; }
}
