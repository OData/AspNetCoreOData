//-----------------------------------------------------------------------------
// <copyright file="Error.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Common;

[ExcludeFromCodeCoverage]
internal static class Error
{
    /// <summary>
    /// Formats the specified resource string using <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>The formatted string.</returns>
    internal static string Format(string format, params object[] args)
    {
        return String.Format(CultureInfo.CurrentCulture, format, args);
    }

    /// <summary>
    /// Creates an <see cref="ArgumentNullException"/> with the provided properties.
    /// </summary>
    /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
    /// <returns>The logged <see cref="Exception"/>.</returns>
    internal static ArgumentNullException ArgumentNull(string parameterName)
    {
        return new ArgumentNullException(parameterName);
    }

    /// <summary>
    /// Creates an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
    /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
    /// <returns>The logged <see cref="Exception"/>.</returns>
    internal static InvalidOperationException InvalidOperation(string messageFormat, params object[] messageArgs)
    {
        return new InvalidOperationException(Error.Format(messageFormat, messageArgs));
    }

    /// <summary>
    /// Creates an <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
    /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
    /// <returns>The logged <see cref="Exception"/>.</returns>
    internal static NotSupportedException NotSupported(string messageFormat, params object[] messageArgs)
    {
        return new NotSupportedException(Error.Format(messageFormat, messageArgs));
    }
}
