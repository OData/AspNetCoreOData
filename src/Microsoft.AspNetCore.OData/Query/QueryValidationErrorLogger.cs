//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLogger.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// Writes structured diagnostics about a query that failed validation, using request state that is available
/// even when the query options could not be fully parsed. This is shared by the controller
/// (<see cref="EnableQueryAttribute"/>) and minimal API (<see cref="ODataQueryEndpointFilter"/>) paths so both
/// report the same information. It never changes the response produced for the failed query.
/// </summary>
internal static class QueryValidationErrorLogger
{
    private const string MessageTemplate =
        "OData query validation failed. Endpoint: {Endpoint}, Type: {QueryType}, Query options: {QueryOptions}. {Reason}";

    /// <summary>
    /// Writes the diagnostic entry for a failed query validation at the specified level. Does nothing when no
    /// logger is available or the level is not enabled.
    /// </summary>
    /// <param name="logger">The logger to write to, or <c>null</c> when none is available.</param>
    /// <param name="logLevel">The level at which the diagnostic is written.</param>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="exception">The exception raised while validating the query.</param>
    internal static void LogQueryValidationFailure(ILogger logger, LogLevel logLevel, HttpContext httpContext, Exception exception)
    {
        if (logger == null || httpContext == null)
        {
            return;
        }

        try
        {
            // The IsEnabled check and the request-state lookup below are inside the guarded region so a
            // misbehaving logging provider or an unexpected state read can never change the request outcome.
            if (!logger.IsEnabled(logLevel))
            {
                return;
            }

            // The processed query options are captured before validation runs; they carry the raw values that
            // were normalized during parsing, so the requested set is reported regardless of whether the request
            // used the '$' prefix. They may be null when the query options could not be built, in which case the
            // element type and requested options are omitted.
            ODataQueryOptions processedQueryOptions = null;
            if (httpContext.Items.TryGetValue(nameof(RequestQueryData), out object item) &&
                item is RequestQueryData requestQueryData)
            {
                processedQueryOptions = requestQueryData.ProcessedQueryOptions;
            }

            // Record the matched endpoint's route template (for example, "v1.0/Users({key})") rather than the
            // concrete request path. The template identifies the endpoint and keeps the route prefix while
            // representing entity keys as placeholders, so the same endpoint is reported consistently across
            // requests. It is null when the endpoint is not a routed endpoint, in which case it is omitted.
            string endpoint = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern?.RawText;

            logger.Log(
                logLevel,
                exception,
                MessageTemplate,
                endpoint,
                processedQueryOptions?.Context?.ElementType?.FullTypeName(),
                FormatRequestedQueryOptions(processedQueryOptions?.RawValues),
                Sanitize(exception?.Message));
        }
        catch (Exception)
        {
            // Recording the diagnostic must never change the request outcome. If the configured logging
            // provider throws while writing this entry, suppress it so the original validation response and
            // the exception raised for the failed query are preserved unchanged.
        }
    }

    /// <summary>
    /// Builds a compact description of the requested query options that reference properties, including only
    /// the options that were supplied so empty options are not reported.
    /// </summary>
    /// <param name="rawValues">The raw query option values, or <c>null</c> when unavailable.</param>
    /// <returns>The requested query options, or an empty string when none apply.</returns>
    private static string FormatRequestedQueryOptions(ODataRawQueryOptions rawValues)
    {
        if (rawValues == null)
        {
            return string.Empty;
        }

        bool hasSelect = !string.IsNullOrEmpty(rawValues.Select);
        bool hasExpand = !string.IsNullOrEmpty(rawValues.Expand);

        if (hasSelect && hasExpand)
        {
            return string.Concat("$select=", Sanitize(rawValues.Select), "&$expand=", Sanitize(rawValues.Expand));
        }

        if (hasSelect)
        {
            return string.Concat("$select=", Sanitize(rawValues.Select));
        }

        if (hasExpand)
        {
            return string.Concat("$expand=", Sanitize(rawValues.Expand));
        }

        return string.Empty;
    }

    /// <summary>
    /// Neutralizes untrusted request-supplied text so it cannot forge additional log entries (CWE-117).
    /// Carriage returns, line feeds and other control characters, along with the Unicode line (U+2028) and
    /// paragraph (U+2029) separators, are replaced with a space, and the result is capped to bound oversized
    /// values. The query option values and the exception message originate from the request, so they are
    /// sanitized before being written as structured log arguments.
    /// </summary>
    /// <param name="value">The value to sanitize, or <c>null</c>.</param>
    /// <returns>The sanitized value, or the original value when it is null or empty.</returns>
    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        const int MaxLength = 2048;
        int length = Math.Min(value.Length, MaxLength);

        StringBuilder builder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            char c = value[i];
            builder.Append(char.IsControl(c) || c == '\u2028' || c == '\u2029' ? ' ' : c);
        }

        return builder.ToString();
    }
}
