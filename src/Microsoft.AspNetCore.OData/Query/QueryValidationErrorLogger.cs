//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLogger.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
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
    internal static void LogWarningOrError(ILogger logger, LogLevel logLevel, HttpContext httpContext, Exception exception)
    {
        if (logger == null || httpContext == null || !logger.IsEnabled(logLevel))
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

        try
        {
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
                exception?.Message);
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
            return string.Concat("$select=", rawValues.Select, ", $expand=", rawValues.Expand);
        }

        if (hasSelect)
        {
            return string.Concat("$select=", rawValues.Select);
        }

        if (hasExpand)
        {
            return string.Concat("$expand=", rawValues.Expand);
        }

        return string.Empty;
    }
}
