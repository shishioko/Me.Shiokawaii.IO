using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


namespace Me.Shiokawaii.IO
{
    public static class BinaryStreamExtension
    {
        extension(Stream stream)
        {
            public async Task<byte[]> ReadU8AAsync(CancellationToken cancellationToken = default)
            {
                using MemoryStream ms = new();
                await stream.CopyToAsync(ms, cancellationToken);
                return ms.ToArray();
            }
            public byte[] ReadU8A()
            {
                using MemoryStream ms = new();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
            
            public async Task<byte[]> ReadU8AAsync(int size, CancellationToken cancellationToken = default)
            {
                byte[] data = new byte[size];
                int position = 0;
                while (position < size)
                {
                    int read = await stream.ReadAsync(data.AsMemory(position, size - position), cancellationToken);
                    position += read;
                    if (read == 0) throw new EndOfStreamException();
                }
                return data;
            }
            public byte[] ReadU8A(int size)
            {
                byte[] data = new byte[size];
                int position = 0;
                while (position < size)
                {
                    int read = stream.Read(data, position, size - position);
                    position += read;
                    if (read == 0) throw new EndOfStreamException();
                }
                return data;
            }
            public async ValueTask WriteU8AAsync(byte[] data, CancellationToken cancellationToken = default)
            {
                await stream.WriteAsync(data, cancellationToken);
            }
            public void WriteU8A(byte[] data)
            {
                stream.Write(data);
            }
            
            public async Task<byte> ReadU8Async(CancellationToken cancellationToken = default)
            {
                return (await stream.ReadU8AAsync(1, cancellationToken))[0];
            }
            public byte ReadU8()
            {
                return stream.ReadU8A(1)[0];
            }
            public async ValueTask WriteU8Async(byte data, CancellationToken cancellationToken = default)
            {
                await stream.WriteAsync(new byte[] { data }, cancellationToken);
            }
            public void WriteU8(byte data)
            {
                stream.Write([data]);
            }
            
            public async Task<sbyte> ReadS8Async(CancellationToken cancellationToken = default)
            {
                return (sbyte)(await stream.ReadU8AAsync(1, cancellationToken))[0];
            }
            public sbyte ReadS8()
            {
                return (sbyte)(stream.ReadU8A(1))[0];
            }
            public async ValueTask WriteS8Async(sbyte data, CancellationToken cancellationToken = default)
            {
                await stream.WriteAsync(MemoryMarshal.AsBytes<sbyte>(new sbyte[] { data }).ToArray(), cancellationToken);
            }
            public void WriteS8(sbyte data)
            {
                stream.Write(MemoryMarshal.AsBytes<sbyte>(new sbyte[] { data }).ToArray());
            }
            
            public async Task<ushort> ReadU16Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(ushort), cancellationToken);
                return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            public ushort ReadU16()
            {
                byte[] buffer = stream.ReadU8A(sizeof(ushort));
                return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            public async ValueTask WriteU16Async(ushort data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(ushort)];
                BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteU16(ushort data)
            {
                byte[] buffer = new byte[sizeof(ushort)];
                BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<short> ReadS16Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(short), cancellationToken);
                return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            public short ReadS16()
            {
                byte[] buffer = stream.ReadU8A(sizeof(short));
                return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            public async ValueTask WriteS16Async(short data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(short)];
                BinaryPrimitives.WriteInt16BigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteS16(short data)
            {
                byte[] buffer = new byte[sizeof(short)];
                BinaryPrimitives.WriteInt16BigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<uint> ReadU32Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(uint), cancellationToken);
                return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            public uint ReadU32()
            {
                byte[] buffer = stream.ReadU8A(sizeof(uint));
                return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            public async ValueTask WriteU32Async(uint data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(uint)];
                BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteU32(uint data)
            {
                byte[] buffer = new byte[sizeof(uint)];
                BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<int> ReadS32Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(int), cancellationToken);
                return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            public int ReadS32()
            {
                byte[] buffer = stream.ReadU8A(sizeof(int));
                return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            public async ValueTask WriteS32Async(int data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32BigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteS32(int data)
            {
                byte[] buffer = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32BigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<ulong> ReadU64Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(ulong), cancellationToken);
                return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            public ulong ReadU64()
            {
                byte[] buffer = stream.ReadU8A(sizeof(ulong));
                return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            public async ValueTask WriteU64Async(ulong data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(ulong)];
                BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteU64(ulong data)
            {
                byte[] buffer = new byte[sizeof(ulong)];
                BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<long> ReadS64Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(long), cancellationToken);
                return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            public long ReadS64()
            {
                byte[] buffer = stream.ReadU8A(sizeof(long));
                return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            public async ValueTask WriteS64Async(long data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(long)];
                BinaryPrimitives.WriteInt64BigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteS64(long data)
            {
                byte[] buffer = new byte[sizeof(long)];
                BinaryPrimitives.WriteInt64BigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<float> ReadF32Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(float), cancellationToken);
                return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            public float ReadF32()
            {
                byte[] buffer = stream.ReadU8A(sizeof(float));
                return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            public async ValueTask WriteF32Async(float data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(float)];
                BinaryPrimitives.WriteSingleBigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteF32(float data)
            {
                byte[] buffer = new byte[sizeof(float)];
                BinaryPrimitives.WriteSingleBigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<double> ReadF64Async(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(sizeof(double), cancellationToken);
                return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
            public double ReadF64()
            {
                byte[] buffer = stream.ReadU8A(sizeof(double));
                return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
            public async ValueTask WriteF64Async(double data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[sizeof(double)];
                BinaryPrimitives.WriteDoubleBigEndian(buffer, data);
                await stream.WriteAsync(buffer, cancellationToken);
            }
            public void WriteF64(double data)
            {
                byte[] buffer = new byte[sizeof(double)];
                BinaryPrimitives.WriteDoubleBigEndian(buffer, data);
                stream.Write(buffer);
            }
            
            public async Task<uint> ReadU32VAsync(CancellationToken cancellationToken = default)
            {
                uint data = 0;
                int position = 0;
                while (true)
                {
                    byte current = await stream.ReadU8Async(cancellationToken);
                    data |= (current & 127U) << position;
                    if ((current & 128) == 0) return data;
                    position += 7;
                    if (position >= sizeof(uint) * 8) throw new ProtocolViolationException();
                }
            }
            public uint ReadU32V()
            {
                uint data = 0;
                int position = 0;
                while (true)
                {
                    byte current = stream.ReadU8();
                    data |= (current & 127U) << position;
                    if ((current & 128) == 0) return data;
                    position += 7;
                    if (position >= sizeof(uint) * 8) throw new ProtocolViolationException();
                }
            }
            public async ValueTask<int> WriteU32VAsync(uint data, CancellationToken cancellationToken = default)
            {
                int size = 0;
                do
                {
                    byte current = (byte)data;
                    data >>= 7;
                    if (data != 0) current |= 128;
                    else current &= 127;
                    await stream.WriteU8Async(current, cancellationToken);
                    size++;
                }
                while (data != 0);
                return size;
            }
            public int WriteU32V(uint data)
            {
                int size = 0;
                do
                {
                    byte current = (byte)data;
                    data >>= 7;
                    if (data != 0) current |= 128;
                    else current &= 127;
                    stream.WriteU8(current);
                    size++;
                }
                while (data != 0);
                return size;
            }
            
            public async Task<int> ReadS32VAsync(CancellationToken cancellationToken = default)
            {
                return (int)await stream.ReadU32VAsync(cancellationToken);
            }
            public int ReadS32V()
            {
                return (int)stream.ReadU32V();
            }
            public async ValueTask<int> WriteS32VAsync(int data, CancellationToken cancellationToken = default)
            {
                return await stream.WriteU32VAsync((uint)data, cancellationToken);
            }
            public int WriteS32V(int data)
            {
                return stream.WriteU32V((uint)data);
            }
            
            public async Task<ulong> ReadU64VAsync(CancellationToken cancellationToken = default)
            {
                ulong data = 0;
                int position = 0;
                while (true)
                {
                    byte current = await stream.ReadU8Async(cancellationToken);
                    data |= (current & 127U) << position;
                    if ((current & 128) == 0) return data;
                    position += 7;
                    if (position >= 64) throw new ProtocolViolationException();
                }
            }
            public ulong ReadU64V()
            {
                ulong data = 0;
                int position = 0;
                while (true)
                {
                    byte current = stream.ReadU8();
                    data |= (current & 127U) << position;
                    if ((current & 128) == 0) return data;
                    position += 7;
                    if (position >= 64) throw new ProtocolViolationException();
                }
            }
            public async ValueTask<int> WriteU64VAsync(ulong data, CancellationToken cancellationToken = default)
            {
                int size = 0;
                do
                {
                    byte current = (byte)data;
                    data >>= 7;
                    if (data != 0) current |= 128;
                    else current &= 127;
                    await stream.WriteU8Async(current, cancellationToken);
                    size++;
                }
                while (data != 0);
                return size;
            }
            public int WriteU64V(ulong data)
            {
                int size = 0;
                do
                {
                    byte current = (byte)data;
                    data >>= 7;
                    if (data != 0) current |= 128;
                    else current &= 127;
                    stream.WriteU8(current);
                    size++;
                }
                while (data != 0);
                return size;
            }
            
            public async Task<long> ReadS64VAsync(CancellationToken cancellationToken = default)
            {
                return (long)await stream.ReadU64VAsync(cancellationToken);
            }
            public long ReadS64V()
            {
                return (long)stream.ReadU64V();
            }
            public async ValueTask<int> WriteS64VAsync(long data, CancellationToken cancellationToken = default)
            {
                return await stream.WriteU64VAsync((ulong)data, cancellationToken);
            }
            public int WriteS64V(long data)
            {
                return stream.WriteU64V((ulong)data);
            }
            
            public async Task<bool> ReadBoolAsync(CancellationToken cancellationToken = default)
            {
                return (await stream.ReadU8Async(cancellationToken)) != 0;
            }
            public bool ReadBool()
            {
                return (stream.ReadU8()) != 0;
            }
            public async ValueTask WriteBoolAsync(bool data, CancellationToken cancellationToken = default)
            {
                await stream.WriteAsync(MemoryMarshal.AsBytes<bool>(new bool[] { data }).ToArray(), cancellationToken);
            }
            public void WriteBool(bool data)
            {
                stream.Write(MemoryMarshal.AsBytes<bool>(new bool[] { data }).ToArray());
            }
            
            public async Task<Guid> ReadGuidAsync(CancellationToken cancellationToken = default)
            {
                byte[] buffer = await stream.ReadU8AAsync(16, cancellationToken);
                if (BitConverter.IsLittleEndian) buffer = [buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15]];
                return MemoryMarshal.Cast<byte, Guid>(buffer)[0];
            }
            public Guid ReadGuid()
            {
                byte[] buffer = stream.ReadU8A(16);
                if (BitConverter.IsLittleEndian) buffer = [buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15]];
                return MemoryMarshal.Cast<byte, Guid>(buffer)[0];
            }
            public async ValueTask WriteGuidAsync(Guid data, CancellationToken cancellationToken = default)
            {
                byte[] buffer = MemoryMarshal.AsBytes([data]).ToArray();
                if (!BitConverter.IsLittleEndian)
                {
                    await stream.WriteAsync(buffer, cancellationToken);
                    return;
                }
                await stream.WriteAsync(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15] }, cancellationToken);
            }
            public void WriteGuid(Guid data)
            {
                byte[] buffer = MemoryMarshal.AsBytes([data]).ToArray();
                if (!BitConverter.IsLittleEndian)
                {
                    stream.Write(buffer);
                    return;
                }
                stream.Write([buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15]]);
            }
        }
    }
}