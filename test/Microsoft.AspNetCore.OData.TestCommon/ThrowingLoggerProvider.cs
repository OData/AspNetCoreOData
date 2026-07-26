//-----------------------------------------------------------------------------
// <copyright file="ThrowingLoggerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.OData.TestCommon;

/// <summary>
/// An <see cref="ILoggerProvider"/> whose logger throws when it writes an entry, so tests can verify that a
/// misbehaving logging sink never changes the outcome of the request. By default it throws for every category;
/// pass a category name to throw only for that category and act as a no-op for all others, which keeps the rest
/// of the framework's logging working while still failing the write under test. Set <c>throwOnIsEnabled</c> to
/// throw from <see cref="ILogger.IsEnabled(LogLevel)"/> instead of <see cref="ILogger.Log"/>, so tests can also
/// exercise a provider that fails before an entry is written.
/// </summary>
public sealed class ThrowingLoggerProvider : ILoggerProvider
{
    private readonly string _throwForCategory;
    private readonly string _message;
    private readonly bool _throwOnIsEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrowingLoggerProvider"/> class.
    /// </summary>
    /// <param name="throwForCategory">
    /// The category the logger throws for, or <c>null</c> to throw for every category.
    /// </param>
    /// <param name="message">The message of the thrown exception.</param>
    /// <param name="throwOnIsEnabled">
    /// When <c>true</c>, the logger throws from <see cref="ILogger.IsEnabled(LogLevel)"/>; otherwise it throws
    /// from <see cref="ILogger.Log"/>.
    /// </param>
    public ThrowingLoggerProvider(string throwForCategory = null, string message = "Simulated logging sink failure.", bool throwOnIsEnabled = false)
    {
        _throwForCategory = throwForCategory;
        _message = message;
        _throwOnIsEnabled = throwOnIsEnabled;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        bool shouldThrow = _throwForCategory == null || string.Equals(categoryName, _throwForCategory, StringComparison.Ordinal);
        return new ThrowingLogger(_message, throwOnLog: shouldThrow && !_throwOnIsEnabled, throwOnIsEnabled: shouldThrow && _throwOnIsEnabled);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    private sealed class ThrowingLogger : ILogger
    {
        private readonly string _message;
        private readonly bool _throwOnLog;
        private readonly bool _throwOnIsEnabled;

        public ThrowingLogger(string message, bool throwOnLog, bool throwOnIsEnabled)
        {
            _message = message;
            _throwOnLog = throwOnLog;
            _throwOnIsEnabled = throwOnIsEnabled;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel)
        {
            if (_throwOnIsEnabled)
            {
                throw new InvalidOperationException(_message);
            }

            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (_throwOnLog)
            {
                throw new InvalidOperationException(_message);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}
