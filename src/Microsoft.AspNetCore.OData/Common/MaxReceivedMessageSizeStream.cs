//-----------------------------------------------------------------------------
// <copyright file="MaxReceivedMessageSizeStream.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Common;

internal sealed class MaxReceivedMessageSizeStream : Stream
{
    private readonly Stream _innerStream;
    private readonly Action _onLimitExceeded;
    private long _bytesRead;
    // Rewinding must not make a genuinely oversized request readable again.
    private bool _limitExceeded;

    internal MaxReceivedMessageSizeStream(Stream innerStream, long maxReceivedMessageSize, Action onLimitExceeded = null)
    {
        if (maxReceivedMessageSize < 0)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxReceivedMessageSize), maxReceivedMessageSize, 0);
        }

        _innerStream = innerStream ?? throw Error.ArgumentNull(nameof(innerStream));
        _onLimitExceeded = onLimitExceeded;
        MaxReceivedMessageSize = maxReceivedMessageSize;
        RecordPosition();
    }

    internal long MaxReceivedMessageSize { get; }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanTimeout => _innerStream.CanTimeout;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set
        {
            _innerStream.Position = value;
            RecordPosition();
        }
    }

    public override int ReadTimeout
    {
        get => _innerStream.ReadTimeout;
        set => _innerStream.ReadTimeout = value;
    }

    public override int WriteTimeout
    {
        get => _innerStream.WriteTimeout;
        set => _innerStream.WriteTimeout = value;
    }

    public override void Flush() => _innerStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => _innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateReadArguments(buffer, offset, count);
        try
        {
            return RecordBytesRead(_innerStream.Read(buffer, offset, GetReadSize(count)));
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    public override int Read(Span<byte> buffer)
    {
        try
        {
            return RecordBytesRead(_innerStream.Read(buffer[..GetReadSize(buffer.Length)]));
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    public override int ReadByte()
    {
        try
        {
            ThrowIfLimitExceeded();
            int value = _innerStream.ReadByte();
            if (value != -1)
            {
                RecordBytesRead(1);
            }

            return value;
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateReadArguments(buffer, offset, count);
        try
        {
            Task<int> read = _innerStream.ReadAsync(buffer, offset, GetReadSize(count), cancellationToken);
            if (read.IsCompletedSuccessfully)
            {
                // Reuse completed reads instead of allocating another task for each buffered read.
                RecordBytesRead(read.Result);
                return read;
            }

            return AwaitReadAsync(read);
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        try
        {
            ValueTask<int> read = _innerStream.ReadAsync(buffer[..GetReadSize(buffer.Length)], cancellationToken);
            // Consume the underlying ValueTask only once, including IValueTaskSource-backed reads.
            return read.IsCompletedSuccessfully ? new ValueTask<int>(RecordBytesRead(read.Result)) : AwaitReadAsync(read);
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    private async Task<int> AwaitReadAsync(Task<int> read)
    {
        try
        {
            return RecordBytesRead(await read.ConfigureAwait(false));
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    private async ValueTask<int> AwaitReadAsync(ValueTask<int> read)
    {
        try
        {
            return RecordBytesRead(await read.ConfigureAwait(false));
        }
        catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _onLimitExceeded?.Invoke();
            throw;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long position = _innerStream.Seek(offset, origin);
        RecordPosition();
        return position;
    }

    public override void SetLength(long value) => _innerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer) => _innerStream.Write(buffer);

    public override void WriteByte(byte value) => _innerStream.WriteByte(value);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _innerStream.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _innerStream.WriteAsync(buffer, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerStream.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _innerStream.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private int GetReadSize(int requestedCount)
    {
        ThrowIfLimitExceeded();
        if (requestedCount == 0)
        {
            return 0;
        }

        // Buffered bytes may be replayed without increasing the received message size.
        long position = _innerStream.CanSeek ? _innerStream.Position : _bytesRead;
        long remaining = MaxReceivedMessageSize - position;
        if (remaining < 0)
        {
            ThrowLimitExceeded(position);
        }

        // One extra byte distinguishes an exact-limit body from an oversized one without draining it.
        return remaining >= requestedCount ? requestedCount : (int)remaining + 1;
    }

    private int RecordBytesRead(int count)
    {
        if (_innerStream.CanSeek)
        {
            RecordPosition();
        }
        else
        {
            long remaining = MaxReceivedMessageSize - _bytesRead;
            if (count > remaining)
            {
                ThrowLimitExceeded(count > long.MaxValue - _bytesRead ? long.MaxValue : _bytesRead + count);
            }

            _bytesRead += count;
        }

        return count;
    }

    private void RecordPosition()
    {
        if (_innerStream.CanSeek)
        {
            long position = _innerStream.Position;
            _bytesRead = Math.Max(_bytesRead, position);
            if (position > MaxReceivedMessageSize)
            {
                ThrowLimitExceeded(_bytesRead);
            }
        }
    }

    private void ThrowIfLimitExceeded()
    {
        if (_limitExceeded)
        {
            ThrowLimitExceeded(_bytesRead);
        }
    }

    private void ThrowLimitExceeded(long totalBytesRead)
    {
        _bytesRead = totalBytesRead;
        _limitExceeded = true;
        throw new BadHttpRequestException(Error.Format(SRResources.MaxReceivedMessageSizeExceeded, totalBytesRead, MaxReceivedMessageSize), StatusCodes.Status413PayloadTooLarge);
    }

    private static void ValidateReadArguments(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
        {
            throw Error.ArgumentNull(nameof(buffer));
        }

        if (offset < 0)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(offset), offset, 0);
        }

        if (count < 0)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(count), count, 0);
        }

        if (buffer.Length - offset < count)
        {
            throw Error.Argument(nameof(buffer), "Offset and length were out of bounds for the array.");
        }
    }
}
