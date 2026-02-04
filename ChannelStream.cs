using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO
{
    public sealed class ChannelStream : Stream
    {
        public static (ChannelStream a, ChannelStream b) CreatePair()
        {
            Channel<byte[]> a = Channel.CreateUnbounded<byte[]>(new() { SingleReader = false, SingleWriter = false }); 
            Channel<byte[]> b = Channel.CreateUnbounded<byte[]>(new() { SingleReader = false, SingleWriter = false });
            return (new(a.Reader, b.Writer), new(b.Reader, a.Writer));
        }
        public static ChannelStream CreateLoopback()
        {
            Channel<byte[]> a = Channel.CreateUnbounded<byte[]>(new() { SingleReader = false, SingleWriter = false }); 
            return new(a.Reader, a.Writer);
        }
        private readonly ChannelReader<byte[]>? Reader;
        private readonly ChannelWriter<byte[]>? Writer;
        private byte[] LastRead = [];
        private int LastReadPosition = 0;
        private bool Disposed = false;
        public override bool CanRead => Reader is not null;
        public override bool CanSeek => false;
        public override bool CanWrite => Writer is not null;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public ChannelStream(ChannelReader<byte[]>? reader, ChannelWriter<byte[]>? writer)
        {
            Reader = reader;
            Writer = writer;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).Result;
        }
        public override int Read(Span<byte> buffer)
        {
            byte[] data = new byte[buffer.Length];
            int read = ReadAsync(data, 0, buffer.Length).Result;
            data.AsSpan().CopyTo(buffer);
            return read;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).Wait();
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            WriteAsync(new ReadOnlyMemory<byte>(buffer.ToArray())).AsTask().Wait();
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Contract.Requires(CanRead);
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (LastRead.Length <= LastReadPosition)
            {
                do LastRead = await Reader!.ReadAsync(cancellationToken);
                while (LastRead.Length == 0);
                LastReadPosition = 0;
            }
            int length = int.Min(buffer.Length, LastRead.Length - LastReadPosition);
            LastRead.AsSpan(LastReadPosition, length).CopyTo(buffer.Span);
            LastReadPosition += length;
            return length;
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Contract.Requires(CanWrite);
            ObjectDisposedException.ThrowIf(Disposed, this);
            await Writer!.WriteAsync(buffer[offset..(offset+count)], cancellationToken);
        }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Writer!.WriteAsync(buffer.ToArray(), cancellationToken);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }
        public override void Flush()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return;
        }
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return Task.CompletedTask;
        }
        public override void Close()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Writer?.TryComplete();
        }
        protected override void Dispose(bool disposing)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (!disposing) return;
            Disposed = true;
            Writer?.TryComplete();
        }
        public override ValueTask DisposeAsync()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            Writer?.TryComplete();
            return ValueTask.CompletedTask;
        }
    }
}
