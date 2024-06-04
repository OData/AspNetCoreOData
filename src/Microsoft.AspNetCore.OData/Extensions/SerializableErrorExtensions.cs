//-----------------------------------------------------------------------------
// <copyright file="SerializableErrorExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="SerializableError"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SerializableErrorExtensions
    {
        /// <summary>
        /// Converts the <paramref name="serializableError"/> to an <see cref="ODataError"/>.
        /// </summary>
        /// <param name="serializableError">The <see cref="SerializableError"/> instance to convert.</param>
        /// <returns>The converted <see cref="ODataError"/></returns>
        public static ODataError CreateODataError(this SerializableError serializableError)
        {
            if (serializableError == null)
            {
                throw Error.ArgumentNull(nameof(serializableError));
            }

            //Clone for removal of handled entries
            var errors = serializableError.ToDictionary(pair => pair.Key, pair => pair.Value);

            var innerError = errors.ToODataInnerError();

            string errorCode = errors.GetPropertyValue<string>(SerializableErrorKeys.ErrorCodeKey);
            string message = errors.GetPropertyValue<string>(SerializableErrorKeys.MessageKey);

            errors.Remove(SerializableErrorKeys.ErrorCodeKey);
            errors.Remove(SerializableErrorKeys.MessageKey);

            return new ODataError
            {
                Code = string.IsNullOrWhiteSpace(errorCode) ? null : errorCode,
                Message = string.IsNullOrWhiteSpace(message) ? errors.ConvertModelStateErrors() : message,
                Details = errors.CreateErrorDetails(),
                InnerError = innerError
            };
        }

        private static ODataInnerError ToODataInnerError(this Dictionary<string, object> errors)
        {
            string innerErrorMessage = errors.GetPropertyValue<string>(SerializableErrorKeys.ExceptionMessageKey);

            if (innerErrorMessage == null)
            {
                string messageDetail = errors.GetPropertyValue<string>(SerializableErrorKeys.MessageDetailKey);

                if (messageDetail == null)
                {
                    SerializableError modelStateError = errors.GetPropertyValue<SerializableError>(SerializableErrorKeys.ModelStateKey);

                    errors.Remove(SerializableErrorKeys.ModelStateKey);

                    return (modelStateError == null) ? null
                        : new ODataInnerError(
                            new Dictionary<string, ODataValue>
                            {
                                {
                                    SerializableErrorKeys.MessageKey, new ODataPrimitiveValue(ConvertModelStateErrors(modelStateError))
                                }
                            });
                }

                errors.Remove(SerializableErrorKeys.MessageDetailKey);

                return new ODataInnerError(new Dictionary<string, ODataValue> { { SerializableErrorKeys.MessageKey, new ODataPrimitiveValue(messageDetail) } });
            }

            errors.Remove(SerializableErrorKeys.ExceptionMessageKey);

            string typeName = errors.GetPropertyValue<string>(SerializableErrorKeys.ExceptionTypeKey);
            string stackTrace = errors.GetPropertyValue<string>(SerializableErrorKeys.StackTraceKey);

            Dictionary<string, ODataValue> properties = new Dictionary<string, ODataValue>
            {
                { SerializableErrorKeys.MessageKey, new ODataPrimitiveValue(innerErrorMessage) }
            };

            if (typeName != null)
            {
                properties.Add(SerializableErrorKeys.ExceptionTypeKey, new ODataPrimitiveValue(typeName));
            }

            if (stackTrace != null)
            {
                properties.Add(SerializableErrorKeys.StackTraceKey, new ODataPrimitiveValue(stackTrace));
            }

            ODataInnerError innerError = new ODataInnerError(properties);

            errors.Remove(SerializableErrorKeys.ExceptionTypeKey);
            errors.Remove(SerializableErrorKeys.StackTraceKey);

            SerializableError innerExceptionError = errors.GetPropertyValue<SerializableError>(SerializableErrorKeys.InnerExceptionKey);

            errors.Remove(SerializableErrorKeys.InnerExceptionKey);

            if (innerExceptionError != null)
            {
                innerError.InnerError = ToODataInnerError(innerExceptionError);
            }

            return innerError;
        }

        // Convert the model state errors in to a string (for debugging only).
        // This should be improved once ODataError allows more details.
        [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "The default format provider is fine here.")]
        private static string ConvertModelStateErrors(this IReadOnlyDictionary<string, object> errors)
        {
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, object> modelStateError in errors.Where(kvp => kvp.Value != null))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                if (!string.IsNullOrWhiteSpace(modelStateError.Key))
                {
                    builder.AppendLine($"{modelStateError.Key}:");
                }

                IEnumerable<string> errorMessages = modelStateError.Value as IEnumerable<string>;
                if (errorMessages != null)
                {
                    foreach (string errorMessage in errorMessages)
                    {
                        builder.AppendLine(errorMessage);
                    }
                }
                else
                {
                    builder.AppendLine(modelStateError.Value.ToString());
                }
            }

            var result = builder.ToString();

            return !result.EndsWith(Environment.NewLine, StringComparison.Ordinal) ? result : result.Substring(0, result.Length - Environment.NewLine.Length);
        }

        private static ICollection<ODataErrorDetail> CreateErrorDetails(this IReadOnlyDictionary<string, object> errors)
        {
            return errors.SelectMany(CreateErrorDetails).ToList();
        }

        private static IEnumerable<ODataErrorDetail> CreateErrorDetails(KeyValuePair<string, object> pair)
        {
            var errors = pair.Value as IEnumerable<string>;

            if (errors != null)
            {
                return errors.Select(error => new ODataErrorDetail
                {
                    Target = string.IsNullOrWhiteSpace(pair.Key) ? null : pair.Key,
                    Message = error
                });
            }

            return new[]
            {
                new ODataErrorDetail
                {
                    Target = pair.Key,
                    Message = pair.Value?.ToString()
                }
            };
        }

        private static TValue GetPropertyValue<TValue>(this IReadOnlyDictionary<string, object> error, string errorKey)
        {
            object value;

            if (error.TryGetValue(errorKey, out value) && value is TValue)
            {
                return (TValue)value;
            }

            return default(TValue);
        }
    }

    /// <summary>
    ///     Different keys for adding entries to an <see cref="SerializableError" /> instance so
    ///     that it can be parsed to a <see cref="ODataError" /> instance
    /// </summary>
    public static class SerializableErrorKeys
    {
        /// <summary>
        /// Provides a key for the Message.
        /// </summary>
        public static readonly string MessageKey = "message";

        /// <summary>
        /// Provides a key for the MessageDetail.
        /// </summary>
        public static readonly string MessageDetailKey = "MessageDetail";

        /// <summary>
        /// Provides a key for the ModelState.
        /// </summary>
        public static readonly string ModelStateKey = "ModelState";

        /// <summary>
        /// Provides a key for the ExceptionMessage.
        /// </summary>
        public static readonly string ExceptionMessageKey = "ExceptionMessage";

        /// <summary>
        /// Provides a key for the ExceptionType.
        /// </summary>
        public static readonly string ExceptionTypeKey = "type";

        /// <summary>
        /// Provides a key for the StackTrace.
        /// </summary>
        public static readonly string StackTraceKey = "stacktrace";

        /// <summary>
        /// Provides a key for the InnerException.
        /// </summary>
        public static readonly string InnerExceptionKey = "InnerException";

        /// <summary>
        /// Provides a key for the MessageLanguage.
        /// </summary>
        public static readonly string MessageLanguageKey = "MessageLanguage";

        /// <summary>
        /// Provides a key for the ErrorCode.
        /// </summary>
        public static readonly string ErrorCodeKey = "ErrorCode";
    }
}
