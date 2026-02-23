using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO
{
    public class HostStream : IDisposable, IAsyncDisposable
    {
        [Obsolete("Implementation currently not reliable.")]
        public readonly SerialStream Stream;
        internal readonly Dictionary<Guid, ClientStream> Clients = [];
        internal readonly SemaphoreSlim Sync = new(1, 1);
        internal readonly SemaphoreSlim WriteSync = new(1, 1);
        public event Func<ClientStream, bool, Task> OnOpen = (stream, own) => Task.CompletedTask;
        private bool Disposed = false;
        public HostStream(Stream stream, bool autoClose, bool littleEndian = false)
        {
            Stream = new(stream)
            {
                AutoClose = autoClose,
                DynamicPrefix = true,
                LongPrefix = true,
                LittleEndian = littleEndian,
            };
        }
        public async Task ListenAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            while (true)
            {
                Guid channel = await Stream.ReadAsync<Guid>(cancellationToken);
                await Sync.WaitAsync(cancellationToken);
                try
                {
                    byte[] data = await Stream.ReadAsync<byte[]>(cancellationToken);
                    if (!Clients.TryGetValue(channel, out ClientStream? client))
                    {
                        if (data.Length <= 0) continue;
                        Clients.Add(channel, client = new(this, channel));
                        _ = OnOpen(client, false);
                    }
                    await client.Input.WriteAsync(data, cancellationToken);
                    if (data.Length <= 0)
                    {
                        Clients.Remove(channel);
                        await client.DisposeAsync();
                    }
                }
                finally
                {
                    Sync.Release();
                }
            }
        }
        public async Task<ClientStream> OpenAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            ClientStream? client = null;
            await Sync.WaitAsync(cancellationToken);
            try
            {
                Guid channel = Guid.NewGuid();
                while (Clients.ContainsKey(channel)) channel = Guid.NewGuid();
                Clients.Add(channel, client = new(this, channel));
                _ = OnOpen(client, true);
            }
            finally
            {
                Sync.Release();
            }
            return client;
        }
        public async Task<ClientStream> OpenAsync(Guid channel, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            await Sync.WaitAsync(cancellationToken);
            try
            {
                if (!Clients.TryGetValue(channel, out ClientStream? client))
                {
                    Clients.Add(channel, client = new(this, channel));
                    _ = OnOpen(client, true);
                }
                return client;
            }
            finally
            {
                Sync.Release();
            }
        }
        public void Dispose()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            Sync.Wait();
            foreach (ClientStream client in Clients.Values)
            {
                client.Input.Dispose();
            }
            Stream.Dispose();
            Sync.Dispose();
            WriteSync.Dispose();
        }
        public async ValueTask DisposeAsync()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            await Sync.WaitAsync();
            foreach (ClientStream client in Clients.Values)
            {
                await client.Input.DisposeAsync();
            }
            Stream.Dispose();
            Sync.Dispose();
            WriteSync.Dispose();
        }
    }
}
