// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Utility class for creating and unwrapping <see cref="Exception"/> instances.
    /// </summary>
    internal static class ExceptionUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageFormat">A composite format string explaining the reason for the exception.</param>
        /// <param name="messageArgs">An object array that contains zero or more objects to format.</param>
        /// <returns>The logged <see cref="Exception"/>.</returns>
        internal static void ThrowODataException(string messageFormat, params object[] messageArgs)
        {
            throw new ODataException(string.Format(CultureInfo.CurrentCulture, messageFormat, messageArgs));
        }

        public static void ThrowODataExceptionIfNotMeet(this Func<bool> testFunc, string messageFormat, params object[] messageArgs)
        {
            if (!testFunc())
            {
                throw new ODataException(string.Format(CultureInfo.CurrentCulture, messageFormat, messageArgs));
            }
        }
    }
}
