// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.OData.ModelBuilder
{
    /// <summary>
    /// Utility class for creating and unwrapping <see cref="Exception"/> instances.
    /// </summary>
    internal static class Error
    {
        /// <summary>
        /// Formats the specified resource string using <see cref="CultureInfo"/>.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The formatted string.</returns>
        internal static string Format(string format, params object[] args)
        {
            return String.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with the provided properties.
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentException Argument(string messageFormat, params object[] messageArgs)
        {
            return new ArgumentException(Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with the provided properties.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentException Argument(string parameterName, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentException(Error.Format(messageFormat, messageArgs), parameterName);
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
        /// Creates an <see cref="ArgumentNullException"/> with the provided properties.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentNullException ArgumentNull(string parameterName, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentNullException(parameterName, Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentException"/> with a default message.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentException ArgumentNullOrEmpty(string parameterName)
        {
            return Error.Argument(parameterName, SRResources.ArgumentNullOrEmpty, parameterName);
        }

        internal static ArgumentException ArgumentNullOrWhitespace(string parameterName)
        {
            return Error.Argument(parameterName, SRResources.ArgumentNullOrWhitespace, parameterName);
        }

        /// <summary>
        /// Creates an <see cref="ArgumentNullException"/> with the provided properties.
        /// </summary>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The purpose of this API is to return an error for properties")]
        internal static ArgumentNullException PropertyNull()
        {
            return new ArgumentNullException("value");
        }

        /// <summary>
        /// Creates an <see cref="ArgumentOutOfRangeException"/> with the provided properties.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName, object actualValue, string messageFormat, params object[] messageArgs)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, Error.Format(messageFormat, messageArgs));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentOutOfRangeException"/> with a message saying that the argument must be greater than or equal to <paramref name="minValue"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <param name="minValue">The minimum size of the argument.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentOutOfRangeException ArgumentMustBeGreaterThanOrEqualTo(string parameterName, object actualValue, object minValue)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, Error.Format(SRResources.ArgumentMustBeGreaterThanOrEqualTo, minValue));
        }

        /// <summary>
        /// Creates an <see cref="ArgumentOutOfRangeException"/> with a message saying that the argument must be less than or equal to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that caused the current exception.</param>
        /// <param name="actualValue">The value of the argument that causes this exception.</param>
        /// <param name="maxValue">The maximum size of the argument.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static ArgumentOutOfRangeException ArgumentMustBeLessThanOrEqualTo(string parameterName, object actualValue, object maxValue)
        {
            return new ArgumentOutOfRangeException(parameterName, actualValue, Error.Format(SRResources.ArgumentMustBeLessThanOrEqualTo, maxValue));
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
        /// Creates an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static InvalidOperationException InvalidOperation(Exception innerException, string messageFormat, params object[] messageArgs)
        {
            return new InvalidOperationException(Error.Format(messageFormat, messageArgs), innerException);
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
}
