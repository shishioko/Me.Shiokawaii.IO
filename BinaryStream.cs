using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO
{
    public class BinaryStream
    {
        public bool AutoClose { get; init; } = false;
        public bool LittleEndian { get; init; } = false;
        private readonly Stream BaseStream;
        private bool Disposed = false;
        public BinaryStream(Stream baseStream)
        {
            BaseStream = baseStream;
        }
        #region U8A
        public async ValueTask<byte[]> ReadU8AAsync(int length, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            byte[] buffer = new byte[length];
            await BaseStream.ReadAtLeastAsync(buffer.AsMemory(), length, true, cancellationToken);
            return buffer;
        }
        public byte[] ReadU8A(int length)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            byte[] buffer = new byte[length];
            BaseStream.ReadAtLeast(buffer, length, true);
            return buffer;
        }
        public async ValueTask WriteU8AAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            await BaseStream.WriteAsync(buffer, cancellationToken);
        }
        public void WriteU8A(byte[] buffer)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            BaseStream.Write(buffer);
        }
        #endregion
        #region U1
        public async ValueTask<bool> ReadU1Async(CancellationToken cancellationToken = default)
        {
            return await ReadU8Async(cancellationToken) != 0;
        }
        public bool ReadU1(int length)
        {
            return ReadU8() != 0;
        }
        public async ValueTask WriteU1Async(bool data, CancellationToken cancellationToken = default)
        {
            await WriteU8Async((byte)(data ? 1 : 0), cancellationToken);
        }
        public void WriteU1(bool data)
        {
            WriteU8((byte)(data ? 1 : 0));
        }
        #endregion
        #region U8
        public async ValueTask<byte> ReadU8Async(CancellationToken cancellationToken = default)
        {
            return (await ReadU8AAsync(1, cancellationToken))[0];
        }
        public byte ReadU8()
        {
            return ReadU8A(1)[0];
        }
        public async ValueTask WriteU8Async(byte data, CancellationToken cancellationToken = default)
        {
            await WriteU8AAsync([data], cancellationToken);
        }
        public void WriteU8(byte data)
        {
            WriteU8A([data]);
        }
        #endregion
        #region S8
        public async ValueTask<sbyte> ReadS8Async(CancellationToken cancellationToken = default)
        {
            return (sbyte)(await ReadU8AAsync(1, cancellationToken))[0];
        }
        public sbyte ReadS8()
        {
            return (sbyte)ReadU8A(1)[0];
        }
        public async ValueTask WriteS8Async(sbyte data, CancellationToken cancellationToken = default)
        {
            await WriteU8AAsync([(byte)data], cancellationToken);
        }
        public void WriteS8(sbyte data)
        {
            WriteU8A([(byte)data]);
        }
        #endregion
        #region U16
        public async ValueTask<ushort> ReadU16Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(ushort), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
        public ushort ReadU16()
        {
            byte[] buffer = ReadU8A(sizeof(ushort));
            if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
        public async ValueTask WriteU16Async(ushort data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(ushort)];
            if (LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(buffer, data);
            else BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteU16(ushort data)
        {
            byte[] buffer = new byte[sizeof(ushort)];
            if (LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(buffer, data);
            else BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region S16
        public async ValueTask<short> ReadS16Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(short), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
        public short ReadS16()
        {
            byte[] buffer = ReadU8A(sizeof(short));
            if (LittleEndian) return BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
        public async ValueTask WriteS16Async(short data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(short)];
            if (LittleEndian) BinaryPrimitives.WriteInt16LittleEndian(buffer, data);
            else BinaryPrimitives.WriteInt16BigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteS16(short data)
        {
            byte[] buffer = new byte[sizeof(short)];
            if (LittleEndian) BinaryPrimitives.WriteInt16LittleEndian(buffer, data);
            else BinaryPrimitives.WriteInt16BigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region U32
        public async ValueTask<uint> ReadU32Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(uint), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
        public uint ReadU32()
        {
            byte[] buffer = ReadU8A(sizeof(uint));
            if (LittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
        public async ValueTask WriteU32Async(uint data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(uint)];
            if (LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(buffer, data);
            else BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteU32(uint data)
        {
            byte[] buffer = new byte[sizeof(uint)];
            if (LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(buffer, data);
            else BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region S32
        public async ValueTask<int> ReadS32Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(int), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }
        public int ReadS32()
        {
            byte[] buffer = ReadU8A(sizeof(int));
            if (LittleEndian) return BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }
        public async ValueTask WriteS32Async(int data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(int)];
            if (LittleEndian) BinaryPrimitives.WriteInt32LittleEndian(buffer, data);
            else BinaryPrimitives.WriteInt32BigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteS32(int data)
        {
            byte[] buffer = new byte[sizeof(int)];
            if (LittleEndian) BinaryPrimitives.WriteInt32LittleEndian(buffer, data);
            else BinaryPrimitives.WriteInt32BigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region U64
        public async ValueTask<ulong> ReadU64Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(ulong), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }
        public ulong ReadU64()
        {
            byte[] buffer = ReadU8A(sizeof(ulong));
            if (LittleEndian) return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }
        public async ValueTask WriteU64Async(ulong data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            if (LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(buffer, data);
            else BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteU64(ulong data)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            if (LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(buffer, data);
            else BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region S64
        public async ValueTask<long> ReadS64Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(long), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }
        public long ReadS64()
        {
            byte[] buffer = ReadU8A(sizeof(long));
            if (LittleEndian) return BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }
        public async ValueTask WriteS64Async(long data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(long)];
            if (LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(buffer, data);
            else BinaryPrimitives.WriteInt64BigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteS64(long data)
        {
            byte[] buffer = new byte[sizeof(long)];
            if (LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(buffer, data);
            else BinaryPrimitives.WriteInt64BigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region F32
        public async ValueTask<float> ReadF32Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(float), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadSingleLittleEndian(buffer);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }
        public float ReadF32()
        {
            byte[] buffer = ReadU8A(sizeof(float));
            if (LittleEndian) return BinaryPrimitives.ReadSingleLittleEndian(buffer);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }
        public async ValueTask WriteF32Async(float data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(float)];
            if (LittleEndian) BinaryPrimitives.WriteSingleLittleEndian(buffer, data);
            else BinaryPrimitives.WriteSingleBigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteF32(float data)
        {
            byte[] buffer = new byte[sizeof(float)];
            if (LittleEndian) BinaryPrimitives.WriteSingleLittleEndian(buffer, data);
            else BinaryPrimitives.WriteSingleBigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        #region F64
        public async ValueTask<double> ReadF64Async(CancellationToken cancellationToken = default)
        {
            byte[] buffer = await ReadU8AAsync(sizeof(double), cancellationToken);
            if (LittleEndian) return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }
        public double ReadF64()
        {
            byte[] buffer = ReadU8A(sizeof(double));
            if (LittleEndian) return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }
        public async ValueTask WriteF64Async(double data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(double)];
            if (LittleEndian) BinaryPrimitives.WriteDoubleLittleEndian(buffer, data);
            else BinaryPrimitives.WriteDoubleBigEndian(buffer, data);
            await WriteU8AAsync(buffer, cancellationToken);
        }
        public void WriteF64(double data)
        {
            byte[] buffer = new byte[sizeof(double)];
            if (LittleEndian) BinaryPrimitives.WriteDoubleLittleEndian(buffer, data);
            else BinaryPrimitives.WriteDoubleBigEndian(buffer, data);
            WriteU8A(buffer);
        }
        #endregion
        
        #region VU32
        public async ValueTask<uint> ReadVU32Async(CancellationToken cancellationToken = default)
        {
            uint data = 0;
            int position = 0;
            while (true)
            {
                byte current = await ReadU8Async(cancellationToken);
                data |= (current & 127U) << position;
                if ((current & 128) == 0) return data;
                position += 7;
                if (position >= sizeof(uint) * 8) throw new ProtocolViolationException($"VarInt size exceeded!");
            }
        }
        public uint ReadVU32()
        {
            uint data = 0;
            int position = 0;
            while (true)
            {
                byte current = ReadU8();
                data |= (current & 127U) << position;
                if ((current & 128) == 0) return data;
                position += 7;
                if (position >= sizeof(uint) * 8) throw new ProtocolViolationException($"VarInt size exceeded!");
            }
        }
        public async ValueTask WriteVU32Async(uint data, CancellationToken cancellationToken = default)
        {
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                await WriteU8Async(current, cancellationToken);
            }
            while (data != 0);
        }
        public void WriteVU32(uint data)
        {
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                WriteU8(current);
            }
            while (data != 0);
        }
        #endregion
        #region VS32
        public async ValueTask<int> ReadVS32Async(CancellationToken cancellationToken = default)
        {
            return (int)await ReadVU32Async(cancellationToken);
        }
        public int ReadVS32()
        {
            return (int)ReadVU32();
        }
        public async ValueTask WriteVS32Async(int data, CancellationToken cancellationToken = default)
        {
            await WriteU32Async((uint)data, cancellationToken);
        }
        public void WriteVS32(int data)
        {
            WriteU32((uint)data);
        }
        #endregion
        #region VU64
        public async ValueTask<ulong> ReadVU64Async(CancellationToken cancellationToken = default)
        {
            ulong data = 0;
            int position = 0;
            while (true)
            {
                byte current = await ReadU8Async(cancellationToken);
                data |= (current & 127U) << position;
                if ((current & 128) == 0) return data;
                position += 7;
                if (position >= sizeof(ulong) * 8) throw new ProtocolViolationException($"VarLong size exceeded!");
            }
        }
        public ulong ReadVU64()
        {
            ulong data = 0;
            int position = 0;
            while (true)
            {
                byte current = ReadU8();
                data |= (current & 127U) << position;
                if ((current & 128) == 0) return data;
                position += 7;
                if (position >= sizeof(ulong) * 8) throw new ProtocolViolationException($"VarLong size exceeded!");
            }
        }
        public async ValueTask WriteVU64Async(ulong data, CancellationToken cancellationToken = default)
        {
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                await WriteU8Async(current, cancellationToken);
            }
            while (data != 0);
        }
        public void WriteVU64(ulong data)
        {
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                WriteU8(current);
            }
            while (data != 0);
        }
        #endregion
        #region VS64
        public async ValueTask<long> ReadVS64Async(CancellationToken cancellationToken = default)
        {
            return (long)await ReadVU64Async(cancellationToken);
        }
        public long ReadVS64()
        {
            return (long)ReadVU64();
        }
        public async ValueTask WriteVS64Async(long data, CancellationToken cancellationToken = default)
        {
            await WriteU64Async((ulong)data, cancellationToken);
        }
        public void WriteVS64(long data)
        {
            WriteU64((ulong)data);
        }
        #endregion
        
        #region U0
        public async ValueTask<ulong> ReadU0Async(int size, CancellationToken cancellationToken = default)
        {
            if (size < 0) throw new ArgumentException("Integer size is negative.");
            if (size > 8) throw new ArgumentException("Integer size exceeds maximum bound.");
            ulong data = 0;
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < size; i++)
                {
                    data |= (ulong)await ReadU8Async(cancellationToken) << (i * 8);
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    data <<= 8;
                    data |= await ReadU8Async(cancellationToken);
                }
            }
            return data;
        }
        public ulong ReadU0(int size)
        {
            if (size < 0) throw new ArgumentException("Integer size is negative.");
            if (size > 8) throw new ArgumentException("Integer size exceeds maximum bound.");
            ulong data = 0;
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < size; i++)
                {
                    data |= (ulong)ReadU8() << (i * 8);
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    data <<= 8;
                    data |= ReadU8();
                }
            }
            return data;
        }
        public async ValueTask WriteU0Async(ulong data, int size, CancellationToken cancellationToken = default)
        {
            if (size < 0) throw new ArgumentException("Integer size is negative.");
            if (size > 8) throw new ArgumentException("Integer size exceeds maximum bound.");
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < size; i++)
                {
                    await WriteU8Async((byte)(data >> (8 * i)), cancellationToken);
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    await WriteU8Async((byte)data, cancellationToken);
                    data >>= 8;
                }
            }
        }
        public void WriteU0(ulong data, int size)
        {
            if (size < 0) throw new ArgumentException("Integer size is negative.");
            if (size > 8) throw new ArgumentException("Integer size exceeds maximum bound.");
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < size; i++)
                {
                    WriteU8((byte)(data >> (8 * i)));
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    WriteU8((byte)data);
                    data >>= 8;
                }
            }
        }
        #endregion
        #region S0
        public async ValueTask<long> ReadS0Async(int size, CancellationToken cancellationToken = default)
        {
            return (long)await ReadU0Async(size, cancellationToken);
        }
        public long ReadS0(int size)
        {
            return (long)ReadU0(size);
        }
        public async ValueTask WriteS0Async(long data, int size, CancellationToken cancellationToken = default)
        {
            await WriteU0Async((ulong)data, size, cancellationToken);
        }
        public void WriteS0(long data, int size)
        {
            WriteU0((ulong)data, size);
        }
        #endregion

        #region Guid
        public async ValueTask<Guid> ReadGuidAsync(CancellationToken cancellationToken = default)
        {
            return new Guid(await ReadU8AAsync(16, cancellationToken), !BitConverter.IsLittleEndian);
        }
        public Guid ReadGuid()
        {
            return new Guid(ReadU8A(16), !BitConverter.IsLittleEndian);
        }
        public async ValueTask WriteGuidAsync(Guid data, CancellationToken cancellationToken = default)
        {
            await WriteU8AAsync(data.ToByteArray(!BitConverter.IsLittleEndian), cancellationToken);
        }
        public void WriteGuid(Guid data)
        {
            WriteU8A(data.ToByteArray(!BitConverter.IsLittleEndian));
        }
        #endregion
    }
}