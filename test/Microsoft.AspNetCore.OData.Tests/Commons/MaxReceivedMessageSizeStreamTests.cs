//-----------------------------------------------------------------------------
// <copyright file="MaxReceivedMessageSizeStreamTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Commons;

public class MaxReceivedMessageSizeStreamTests
{
    [Theory]
    [InlineData(false, StatusCodes.Status400BadRequest)]
    [InlineData(false, StatusCodes.Status413PayloadTooLarge)]
    [InlineData(true, StatusCodes.Status400BadRequest)]
    [InlineData(true, StatusCodes.Status413PayloadTooLarge)]
    public void Read_InnerBadHttpRequestException_NotifiesOnlyForSizeLimit(bool readByte, int statusCode)
    {
        var expected = new BadHttpRequestException("Read rejected.", statusCode);
        var inner = new Mock<Stream>();
        var buffer = new byte[4];
        inner.Setup(s => s.Read(buffer, 0, buffer.Length)).Throws(expected);
        inner.Setup(s => s.ReadByte()).Throws(expected);
        int notifications = 0;
        using var stream = new MaxReceivedMessageSizeStream(inner.Object, 4, () => notifications++);

        BadHttpRequestException exception = Assert.Throws<BadHttpRequestException>(() =>
        {
            if (readByte) stream.ReadByte();
            else stream.Read(buffer, 0, buffer.Length);
        });

        Assert.Same(expected, exception);
        Assert.Equal(statusCode == StatusCodes.Status413PayloadTooLarge ? 1 : 0, notifications);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public async Task ReadAsync_InnerSizeLimitException_NotifiesAndPreservesException(bool useMemory, int completionMode)
    {
        var expected = new BadHttpRequestException("Read rejected.", StatusCodes.Status413PayloadTooLarge);
        var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var inner = new Mock<Stream>();
        var buffer = new byte[4];
        if (completionMode == 0)
        {
            inner.Setup(s => s.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)).Throws(expected);
            inner.Setup(s => s.ReadAsync(It.IsAny<Memory<byte>>(), CancellationToken.None)).Throws(expected);
        }
        else
        {
            inner.Setup(s => s.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)).Returns(completion.Task);
            inner.Setup(s => s.ReadAsync(It.IsAny<Memory<byte>>(), CancellationToken.None)).Returns(new ValueTask<int>(completion.Task));
            if (completionMode == 1) completion.SetException(expected);
        }

        int notifications = 0;
        using var stream = new MaxReceivedMessageSizeStream(inner.Object, 4, () => notifications++);

        Task<BadHttpRequestException> rejection = Assert.ThrowsAsync<BadHttpRequestException>(async () =>
        {
            if (useMemory) await stream.ReadAsync(buffer.AsMemory());
            else await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
        });
        if (completionMode == 2)
        {
            Assert.False(rejection.IsCompleted);
            completion.SetException(expected);
        }

        BadHttpRequestException exception = await rejection;
        Assert.Same(expected, exception);
        Assert.Equal(1, notifications);
    }

    [Fact]
    public void Read_LocalSizeLimitExceeded_NotifiesBeforeExceptionEscapes()
    {
        bool notified = false;
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[8]), 4, () => notified = true);
        var buffer = new byte[8];

        BadHttpRequestException exception = Assert.Throws<BadHttpRequestException>(() => stream.Read(buffer, 0, buffer.Length));

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, exception.StatusCode);
        Assert.True(notified);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 4)]
    [InlineData(3, 4)]
    [InlineData(4, 4)]
    public void Read_ContentAtOrUnderLimit_ReturnsAllBytes(int length, int limit)
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[length]), limit);
        var buffer = new byte[8];

        int read = stream.Read(buffer, 0, buffer.Length);

        Assert.Equal(length, read);
        Assert.Equal(0, stream.Read(buffer, 0, buffer.Length));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(8)]
    public void Read_OversizedBody_ConsumesAtMostOneByteBeyondLimit(int count)
    {
        var inner = new MemoryStream(new byte[8]);
        using var stream = new MaxReceivedMessageSizeStream(inner, 4);
        var buffer = new byte[8];

        BadHttpRequestException exception = Assert.Throws<BadHttpRequestException>(() =>
        {
            while (stream.Read(buffer, 0, count) != 0) { }
        });

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, exception.StatusCode);
        Assert.Contains("a total of 5 bytes has been read", exception.Message);
        Assert.Equal(5, inner.Position);
        Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());
        Assert.Equal(5, inner.Position);
    }

    [Fact]
    public void ReadSpan_OversizedBody_Throws()
    {
        var inner = new MemoryStream(new byte[8]);
        using var stream = new MaxReceivedMessageSizeStream(inner, 4);
        var buffer = new byte[8];

        Assert.Throws<BadHttpRequestException>(() => stream.Read(buffer.AsSpan()));

        Assert.Equal(5, inner.Position);
    }

    [Fact]
    public void ReadByte_OversizedBody_ThrowsAndRemainsTerminalAfterRewind()
    {
        var inner = new MemoryStream(new byte[2]);
        using var stream = new MaxReceivedMessageSizeStream(inner, 1);

        Assert.Equal(0, stream.ReadByte());
        Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());
        stream.Position = 0;

        Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());
        Assert.Equal(0, inner.Position);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadAsync_OversizedBody_ThrowsAndRemainsTerminal(bool useMemory)
    {
        var inner = new MemoryStream(new byte[8]);
        using var stream = new MaxReceivedMessageSizeStream(inner, 4);
        var buffer = new byte[8];

        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
        {
            if (useMemory) await stream.ReadAsync(buffer.AsMemory());
            else await stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
        });
        await Assert.ThrowsAsync<BadHttpRequestException>(async () => await stream.ReadAsync(buffer.AsMemory()));

        Assert.Equal(5, inner.Position);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadAsync_IncompleteRead_EnforcesLimitAfterCompletion(bool useMemory)
    {
        var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var inner = new Mock<Stream>();
        var buffer = new byte[8];
        inner.Setup(s => s.ReadAsync(buffer, 0, 5, CancellationToken.None)).Returns(completion.Task);
        inner.Setup(s => s.ReadAsync(It.IsAny<Memory<byte>>(), CancellationToken.None)).Returns(new ValueTask<int>(completion.Task));
        using var stream = new MaxReceivedMessageSizeStream(inner.Object, 4);

        Task<int> read = useMemory ? stream.ReadAsync(buffer.AsMemory()).AsTask() : stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
        Assert.False(read.IsCompleted);
        completion.SetResult(5);
        BadHttpRequestException exception = await Assert.ThrowsAsync<BadHttpRequestException>(() => read);

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, exception.StatusCode);
        Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());
        inner.Verify(s => s.ReadByte(), Times.Never());
    }

    [Fact]
    public void ReadAsync_CompletedArrayRead_ReusesUnderlyingTask()
    {
        var inner = new Mock<Stream>();
        var buffer = new byte[16];
        Task<int> completed = Task.FromResult(16);
        inner.Setup(s => s.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)).Returns(completed);
        using var stream = new MaxReceivedMessageSizeStream(inner.Object, 16);

        Task<int> read = stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);

        Assert.Same(completed, read);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadAsync_Cancellation_PropagatesWithoutConsumingQuota(bool useMemory)
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[4]), 4);
        var buffer = new byte[4];
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            if (useMemory) await stream.ReadAsync(buffer.AsMemory(), cancellation.Token);
            else await stream.ReadAsync(buffer, 0, buffer.Length, cancellation.Token);
        });

        Assert.Equal(4, stream.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public async Task ReadAsync_InnerFailure_IsPropagated()
    {
        var expected = new IOException("Read failed.");
        var inner = new Mock<Stream>();
        var buffer = new byte[4];
        inner.Setup(s => s.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)).Returns(Task.FromException<int>(expected));
        using var stream = new MaxReceivedMessageSizeStream(inner.Object, 4);

        IOException exception = await Assert.ThrowsAsync<IOException>(() => stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None));

        Assert.Same(expected, exception);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(0, -1)]
    [InlineData(2, 1)]
    [InlineData(1, 2)]
    public async Task Read_InvalidRange_IsNotHiddenByQuotaClamping(int offset, int count)
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[8]), 1);
        var buffer = new byte[2];

        Assert.ThrowsAny<ArgumentException>(() => stream.Read(buffer, offset, count));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => stream.ReadAsync(buffer, offset, count, CancellationToken.None));

        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public async Task Read_NullBuffer_IsRejected()
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[8]), 1);

        Assert.Throws<ArgumentNullException>(() => stream.Read(null, 0, 2));
        await Assert.ThrowsAsync<ArgumentNullException>(() => stream.ReadAsync(null, 0, 2, CancellationToken.None));
    }

    [Fact]
    public void Read_ZeroCount_DoesNotConsumeContent()
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[1]), 1);
        var buffer = new byte[1];

        Assert.Equal(0, stream.Read(buffer, 0, 0));
        Assert.Equal(1, stream.Read(buffer, 0, 1));
        Assert.Equal(-1, stream.ReadByte());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Read_BufferedBodyAtLimit_CanBeReplayed(bool useSeek)
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[4]), 4);
        var buffer = new byte[4];
        Assert.Equal(4, stream.Read(buffer, 0, buffer.Length));

        if (useSeek) stream.Seek(0, SeekOrigin.Begin);
        else stream.Position = 0;

        Assert.Equal(4, stream.Read(buffer, 0, buffer.Length));
        Assert.Equal(0, stream.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Read_AfterPartialReplay_StillRejectsBytesBeyondBodyLimit()
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[8]), 4);
        var buffer = new byte[4];
        Assert.Equal(2, stream.Read(buffer, 0, 2));
        stream.Seek(0, SeekOrigin.Begin);
        Assert.Equal(4, stream.Read(buffer, 0, 4));

        BadHttpRequestException exception = Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());

        Assert.Contains("a total of 5 bytes has been read", exception.Message);
        Assert.Equal(5, stream.Position);
    }

    [Fact]
    public void Constructor_PositionAlreadyOverLimit_RejectsBody()
    {
        var inner = new MemoryStream(new byte[8]) { Position = 5 };

        BadHttpRequestException exception = Assert.Throws<BadHttpRequestException>(() => new MaxReceivedMessageSizeStream(inner, 4));

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, exception.StatusCode);
        inner.Dispose();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Seek_ForwardPastLimit_Throws(bool useSeek)
    {
        using var stream = new MaxReceivedMessageSizeStream(new MemoryStream(new byte[8]), 4);

        Assert.Throws<BadHttpRequestException>(() =>
        {
            if (useSeek) stream.Seek(5, SeekOrigin.Begin);
            else stream.Position = 5;
        });

        Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());
    }

    [Fact]
    public void Constructor_ExistingPosition_CountsPreviouslyConsumedBytes()
    {
        var inner = new MemoryStream(new byte[8]) { Position = 3 };
        using var stream = new MaxReceivedMessageSizeStream(inner, 4);

        Assert.Equal(0, stream.ReadByte());
        Assert.Throws<BadHttpRequestException>(() => stream.ReadByte());
        Assert.Equal(5, inner.Position);
    }

    [Fact]
    public void Read_NonSeekableStream_EnforcesCumulativeLimit()
    {
        var inner = new Mock<Stream>();
        var buffer = new byte[2];
        inner.Setup(s => s.Read(buffer, 0, 2)).Returns(2);
        inner.Setup(s => s.Read(buffer, 0, 1)).Returns(1);
        using var stream = new MaxReceivedMessageSizeStream(inner.Object, 4);

        Assert.Equal(2, stream.Read(buffer, 0, 2));
        Assert.Equal(2, stream.Read(buffer, 0, 2));
        Assert.Throws<BadHttpRequestException>(() => stream.Read(buffer, 0, 2));

        inner.Verify(s => s.Read(buffer, 0, 1), Times.Once());
    }
}
