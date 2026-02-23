using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO
{
    [Obsolete("Implementation currently not reliable.")]
    public class ClientStream : Stream
    {
        private readonly HostStream Host;
        public readonly Guid Channel;
        internal readonly ChannelStream Input;
        private bool Disposed = false;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        internal ClientStream(HostStream host, Guid channel)
        {
            Host = host;
            Channel = channel;
            Input = ChannelStream.CreateLoopback();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return Input.Read(buffer, offset, count);
        }
        public override int Read(Span<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return Input.Read(buffer);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return Input.ReadAsync(buffer, offset, count, cancellationToken);
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return Input.ReadAsync(buffer, cancellationToken);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (count <= 0) return;
            Host.WriteSync.Wait();
            try
            {
                Host.Stream.Write(Channel);
                Host.Stream.Write(buffer[offset..(offset + count)]);
            }
            finally
            {
                Host.WriteSync.Release();
            }
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Write(buffer.ToArray(), 0, buffer.Length);
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (count <= 0) return;
            await Host.WriteSync.WaitAsync(cancellationToken);
            try
            {
                await Host.Stream.WriteAsync(Channel, cancellationToken);
                await Host.Stream.WriteAsync(buffer[offset..(offset + count)], cancellationToken);
            }
            finally
            {
                Host.WriteSync.Release();
            }
        }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await WriteAsync(buffer.ToArray(), 0, buffer.Length, cancellationToken);
        }
        public override void Flush()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Input.Flush();
        }
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            await Input.FlushAsync(cancellationToken);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Close()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Dispose(true);
        }
        protected override void Dispose(bool disposing)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (!disposing) return;
            Disposed = true;
            Host.WriteSync.Wait();
            try
            {
                Host.Stream.Write(new byte[0]); //Array.Empty<byte> somehow has a different type.
            }
            finally
            {
                Host.WriteSync.Release();
            }
            Host.Sync.Wait();
            try
            {
                Host.Clients.Remove(Channel);
                Input.Dispose();
            }
            finally
            {
                Host.Sync.Release();
            }
        }
        public override async ValueTask DisposeAsync()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            await Host.WriteSync.WaitAsync();
            try
            {
                await Host.Stream.WriteAsync(Array.Empty<byte>);
            }
            finally
            {
                Host.WriteSync.Release();
            }
            await Host.Sync.WaitAsync();
            try
            {
                Host.Clients.Remove(Channel);
                Input.Dispose();
            }
            finally
            {
                Host.Sync.Release();
            }
        }
    }
}
