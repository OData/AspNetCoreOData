//-----------------------------------------------------------------------------
// <copyright file="ODataMiniOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData;

/// <summary>
/// The options for minimal API scenarios
/// </summary>
public class ODataMiniOptions
{
    private DefaultQueryConfigurations _queryConfigurations = new DefaultQueryConfigurations();
    private bool _enableNoDollarQueryOptions = true;
    private bool _enableCaseInsensitive = true;
    private bool _enableContinueOnErrorHeader = false;
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
    private ODataVersion _version = ODataVersionConstraint.DefaultODataVersion;
    private long _maxReceivedMessageSize = ODataBatchHandler.DefaultMaxReceivedMessageSize;
    private bool _enableQueryValidationErrorLogging = false;
    private LogLevel _queryValidationErrorLogLevel = LogLevel.Warning;

    /// <summary>
    /// Gets the query configurations. for example, enable '$select' or not.
    /// </summary>
    public DefaultQueryConfigurations QueryConfigurations { get => _queryConfigurations; }

    /// <summary>
    /// Gets the OData version.
    /// Please call 'SetVersion()' to config.
    /// </summary>
    public ODataVersion Version { get => _version; }

    /// <summary>
    /// Gets whether or not the OData system query options should be prefixed with '$'.
    /// Please call 'SetNoDollarQueryOptions()' to config.
    /// </summary>
    public bool EnableNoDollarQueryOptions { get => _enableNoDollarQueryOptions; }

    /// <summary>
    /// Ges whether or not case insensitive.
    /// Please call 'SetCaseInsensitive()' to config.
    /// </summary>
    public bool EnableCaseInsensitive { get => _enableCaseInsensitive; }

    /// <summary>
    /// Gets whether or not continue on error.
    /// Please call 'SetContinueOnErrorHeader()' to config.
    /// </summary>
    public bool EnableContinueOnErrorHeader { get => _enableContinueOnErrorHeader; }

    /// <summary>
    /// Gets TimeZoneInfo for the <see cref="DateTime"/> serialization and deserialization.
    /// Please call 'SetTimeZoneInfo()' to config.
    /// </summary>
    public TimeZoneInfo TimeZone { get => _timeZone; }

    /// <summary>
    /// Gets the maximum size, in bytes, of an OData request or response message body.
    /// Default is 100 MB (104,857,600 bytes). Please call 'SetMaxReceivedMessageSize()' to config.
    /// </summary>
    public long MaxReceivedMessageSize { get => _maxReceivedMessageSize; }

    /// <summary>
    /// Gets a value indicating whether diagnostic details are recorded when an incoming query fails validation.
    /// Please call 'SetQueryValidationErrorLogging()' to config. The default value is <c>false</c>.
    /// </summary>
    public bool EnableQueryValidationErrorLogging { get => _enableQueryValidationErrorLogging; }

    /// <summary>
    /// Gets the <see cref="LogLevel"/> at which query validation diagnostics are written when
    /// <see cref="EnableQueryValidationErrorLogging"/> is enabled. Please call 'SetQueryValidationErrorLogLevel()' to
    /// config. The default value is <see cref="LogLevel.Warning"/>.
    /// </summary>
    public LogLevel QueryValidationErrorLogLevel { get => _queryValidationErrorLogLevel; }

    /// <summary>
    /// Config whether or not no '$' sign query option.
    /// </summary>
    /// <param name="enableNoDollarQueryOptions">Case insensitive or not.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetNoDollarQueryOptions(bool enableNoDollarQueryOptions)
    {
        _enableNoDollarQueryOptions = enableNoDollarQueryOptions;
        return this;
    }

    /// <summary>
    /// Config the case insensitive.
    /// </summary>
    /// <param name="enableCaseInsensitive">Case insensitive or not.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetCaseInsensitive(bool enableCaseInsensitive)
    {
        _enableCaseInsensitive = enableCaseInsensitive;
        return this;
    }

    /// <summary>
    /// Config whether or not diagnostic details are recorded when an incoming query fails validation. When
    /// enabled, the endpoint's route template, the queried type, the requested <c>$select</c> and <c>$expand</c>, and the
    /// failure reason are written.
    /// </summary>
    /// <param name="enableQueryValidationErrorLogging">Whether to record query validation diagnostics.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetQueryValidationErrorLogging(bool enableQueryValidationErrorLogging)
    {
        _enableQueryValidationErrorLogging = enableQueryValidationErrorLogging;
        return this;
    }

    /// <summary>
    /// Config the <see cref="LogLevel"/> at which query validation diagnostics are written.
    /// </summary>
    /// <param name="logLevel">The level at which query validation diagnostics are written.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetQueryValidationErrorLogLevel(LogLevel logLevel)
    {
        _queryValidationErrorLogLevel = logLevel;
        return this;
    }

    /// <summary>
    /// Config the value indicating if batch requests should continue on error.
    /// </summary>
    /// <param name="enableContinueOnErrorHeader">the value indicating if batch requests should continue on error.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetContinueOnErrorHeader(bool enableContinueOnErrorHeader)
    {
        _enableContinueOnErrorHeader = enableContinueOnErrorHeader;
        return this;
    }

    /// <summary>
    /// Config the time zone information.
    /// </summary>
    /// <param name="tzi">Case insensitive or not.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetTimeZoneInfo(TimeZoneInfo tzi)
    {
        _timeZone = tzi;
        return this;
    }

    /// <summary>
    /// Config the OData version.
    /// </summary>
    /// <param name="version">The OData version.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetVersion(ODataVersion version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Enables all OData query options.
    /// </summary>
    /// <param name="maxTopValue"></param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions EnableAll(int? maxTopValue = null)
    {
        _queryConfigurations.EnableExpand = true;
        _queryConfigurations.EnableSelect = true;
        _queryConfigurations.EnableFilter = true;
        _queryConfigurations.EnableOrderBy = true;
        _queryConfigurations.EnableCount = true;
        _queryConfigurations.EnableSkipToken = true;
        SetMaxTop(maxTopValue);
        return this;
    }

    /// <summary>
    /// Enable $expand query options.
    /// </summary>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions Expand()
    {
        _queryConfigurations.EnableExpand = true;
        return this;
    }

    /// <summary>
    /// Enable $select query options.
    /// </summary>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions Select()
    {
        _queryConfigurations.EnableSelect = true;
        return this;
    }

    /// <summary>
    /// Enable $filter query options.
    /// </summary>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions Filter()
    {
        _queryConfigurations.EnableFilter = true;
        return this;
    }

    /// <summary>
    /// Enable $orderby query options.
    /// </summary>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions OrderBy()
    {
        _queryConfigurations.EnableOrderBy = true;
        return this;
    }

    /// <summary>
    /// Enable $count query options.
    /// </summary>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions Count()
    {
        _queryConfigurations.EnableCount = true;
        return this;
    }

    /// <summary>
    /// Enable $skiptoken query option.
    /// </summary>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SkipToken()
    {
        _queryConfigurations.EnableSkipToken = true;
        return this;
    }

    /// <summary>
    /// Config the maximum size, in bytes, of an OData request or response message body.
    /// </summary>
    /// <param name="maxReceivedMessageSize">The maximum size in bytes.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetMaxReceivedMessageSize(long maxReceivedMessageSize)
    {
        if (maxReceivedMessageSize <= 0)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxReceivedMessageSize), maxReceivedMessageSize, 1);
        }

        _maxReceivedMessageSize = maxReceivedMessageSize;
        return this;
    }

    /// <summary>
    ///Sets the maximum value of $top that a client can request.
    /// </summary>
    /// <param name="maxTopValue">The maximum value of $top that a client can request.</param>
    /// <returns>The current <see cref="ODataMiniOptions"/> instance to enable further configuration.</returns>
    public ODataMiniOptions SetMaxTop(int? maxTopValue)
    {
        if (maxTopValue.HasValue && maxTopValue.Value < 0)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxTopValue), maxTopValue, 0);
        }

        _queryConfigurations.MaxTop = maxTopValue;
        return this;
    }

    internal void UpdateFrom(ODataMiniOptions otherOptions)
    {
        this._queryConfigurations.UpdateAll(otherOptions.QueryConfigurations);
        this._version = otherOptions.Version;
        this._enableNoDollarQueryOptions = otherOptions.EnableNoDollarQueryOptions;
        this._enableCaseInsensitive = otherOptions.EnableCaseInsensitive;
        this._enableContinueOnErrorHeader = otherOptions.EnableContinueOnErrorHeader;
        this._timeZone = otherOptions.TimeZone;
        this._maxReceivedMessageSize = otherOptions.MaxReceivedMessageSize;
        this._enableQueryValidationErrorLogging = otherOptions.EnableQueryValidationErrorLogging;
        this._queryValidationErrorLogLevel = otherOptions.QueryValidationErrorLogLevel;
    }
}
