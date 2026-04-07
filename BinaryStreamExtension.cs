using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Me.Shiokawaii.IO
{
    public static class BinaryStreamExtension
    {
        public static async Task<byte[]> ReadU8AAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            using MemoryStream ms = new();
            await stream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }
        public static byte[] ReadU8A(this Stream stream)
        {
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
        
        public static async Task<byte[]> ReadU8AAsync(this Stream stream, int size, CancellationToken cancellationToken = default)
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
        public static byte[] ReadU8A(this Stream stream, int size)
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
        public static async ValueTask WriteU8AAsync(this Stream stream, byte[] data, CancellationToken cancellationToken = default)
        {
            await stream.WriteAsync(data, cancellationToken);
        }
        public static void WriteU8A(this Stream stream, byte[] data)
        {
            stream.Write(data);
        }
        
        public static async Task<byte> ReadU8Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            return (await stream.ReadU8AAsync(1, cancellationToken))[0];
        }
        public static byte ReadU8(this Stream stream)
        {
            return stream.ReadU8A(1)[0];
        }
        public static async ValueTask WriteU8Async(this Stream stream, byte data, CancellationToken cancellationToken = default)
        {
            await stream.WriteAsync(new byte[] { data }, cancellationToken);
        }
        public static void WriteU8(this Stream stream, byte data)
        {
            stream.Write([data]);
        }
        
        public static async Task<sbyte> ReadS8Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            return (sbyte)(await stream.ReadU8AAsync(1, cancellationToken))[0];
        }
        public static sbyte ReadS8(this Stream stream)
        {
            return (sbyte)(stream.ReadU8A(1))[0];
        }
        public static async ValueTask WriteS8Async(this Stream stream, sbyte data, CancellationToken cancellationToken = default)
        {
            await stream.WriteAsync(MemoryMarshal.AsBytes<sbyte>(new sbyte[] { data }).ToArray(), cancellationToken);
        }
        public static void WriteS8(this Stream stream, sbyte data)
        {
            stream.Write(MemoryMarshal.AsBytes<sbyte>(new sbyte[] { data }).ToArray());
        }
        
        public static async Task<ushort> ReadU16Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(ushort), cancellationToken);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
        public static ushort ReadU16(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(ushort));
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }
        public static async ValueTask WriteU16Async(this Stream stream, ushort data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteU16(this Stream stream, ushort data)
        {
            byte[] buffer = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<short> ReadS16Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(short), cancellationToken);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
        public static short ReadS16(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(short));
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }
        public static async ValueTask WriteS16Async(this Stream stream, short data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteS16(this Stream stream, short data)
        {
            byte[] buffer = new byte[sizeof(short)];
            BinaryPrimitives.WriteInt16BigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<uint> ReadU32Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(uint), cancellationToken);
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
        public static uint ReadU32(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(uint));
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
        public static async ValueTask WriteU32Async(this Stream stream, uint data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteU32(this Stream stream, uint data)
        {
            byte[] buffer = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<int> ReadS32Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(int), cancellationToken);
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }
        public static int ReadS32(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(int));
            return BinaryPrimitives.ReadInt32BigEndian(buffer);
        }
        public static async ValueTask WriteS32Async(this Stream stream, int data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteS32(this Stream stream, int data)
        {
            byte[] buffer = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<ulong> ReadU64Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(ulong), cancellationToken);
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }
        public static ulong ReadU64(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(ulong));
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
        }
        public static async ValueTask WriteU64Async(this Stream stream, ulong data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteU64(this Stream stream, ulong data)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<long> ReadS64Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(long), cancellationToken);
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }
        public static long ReadS64(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(long));
            return BinaryPrimitives.ReadInt64BigEndian(buffer);
        }
        public static async ValueTask WriteS64Async(this Stream stream, long data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteS64(this Stream stream, long data)
        {
            byte[] buffer = new byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<float> ReadF32Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(float), cancellationToken);
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }
        public static float ReadF32(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(float));
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
        }
        public static async ValueTask WriteF32Async(this Stream stream, float data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteF32(this Stream stream, float data)
        {
            byte[] buffer = new byte[sizeof(float)];
            BinaryPrimitives.WriteSingleBigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<double> ReadF64Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(sizeof(double), cancellationToken);
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }
        public static double ReadF64(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(sizeof(double));
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
        }
        public static async ValueTask WriteF64Async(this Stream stream, double data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(buffer, data);
            await stream.WriteAsync(buffer, cancellationToken);
        }
        public static void WriteF64(this Stream stream, double data)
        {
            byte[] buffer = new byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleBigEndian(buffer, data);
            stream.Write(buffer);
        }
        
        public static async Task<uint> ReadU32VAsync(this Stream stream, CancellationToken cancellationToken = default)
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
        public static uint ReadU32V(this Stream stream)
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
        public static async ValueTask<int> WriteU32VAsync(this Stream stream, uint data, CancellationToken cancellationToken = default)
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
        public static int WriteU32V(this Stream stream, uint data)
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
        
        public static async Task<int> ReadS32VAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            return (int)await stream.ReadU32VAsync(cancellationToken);
        }
        public static int ReadS32V(this Stream stream)
        {
            return (int)stream.ReadU32V();
        }
        public static async ValueTask<int> WriteS32VAsync(this Stream stream, int data, CancellationToken cancellationToken = default)
        {
            return await stream.WriteU32VAsync((uint)data, cancellationToken);
        }
        public static int WriteS32V(this Stream stream, int data)
        {
            return stream.WriteU32V((uint)data);
        }
        
        public static async Task<ulong> ReadU64VAsync(this Stream stream, CancellationToken cancellationToken = default)
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
        public static ulong ReadU64V(this Stream stream)
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
        public static async ValueTask<int> WriteU64VAsync(this Stream stream, ulong data, CancellationToken cancellationToken = default)
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
        public static int WriteU64V(this Stream stream, ulong data)
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
        
        public static async Task<long> ReadS64VAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            return (long)await stream.ReadU64VAsync(cancellationToken);
        }
        public static long ReadS64V(this Stream stream)
        {
            return (long)stream.ReadU64V();
        }
        public static async ValueTask<int> WriteS64VAsync(this Stream stream, long data, CancellationToken cancellationToken = default)
        {
            return await stream.WriteU64VAsync((ulong)data, cancellationToken);
        }
        public static int WriteS64V(this Stream stream, long data)
        {
            return stream.WriteU64V((ulong)data);
        }
        
        public static async Task<bool> ReadBoolAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            return (await stream.ReadU8Async(cancellationToken)) != 0;
        }
        public static bool ReadBool(this Stream stream)
        {
            return (stream.ReadU8()) != 0;
        }
        public static async ValueTask WriteBoolAsync(this Stream stream, bool data, CancellationToken cancellationToken = default)
        {
            await stream.WriteAsync(MemoryMarshal.AsBytes<bool>(new bool[] { data }).ToArray(), cancellationToken);
        }
        public static void WriteBool(this Stream stream, bool data)
        {
            stream.Write(MemoryMarshal.AsBytes<bool>(new bool[] { data }).ToArray());
        }
        
        public static async Task<Guid> ReadGuidAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            byte[] buffer = await stream.ReadU8AAsync(16, cancellationToken);
            if (BitConverter.IsLittleEndian) buffer = [buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15]];
            return MemoryMarshal.Cast<byte, Guid>(buffer)[0];
        }
        public static async ValueTask WriteGuidAsync(this Stream stream, Guid data, CancellationToken cancellationToken = default)
        {
            byte[] buffer = MemoryMarshal.AsBytes([data]).ToArray();
            if (!BitConverter.IsLittleEndian)
            {
                await stream.WriteAsync(buffer, cancellationToken);
                return;
            }
            await stream.WriteAsync(new byte[] { buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15] }, cancellationToken);
        }
        public static Guid ReadGuid(this Stream stream)
        {
            byte[] buffer = stream.ReadU8A(16);
            if (BitConverter.IsLittleEndian) buffer = [buffer[3], buffer[2], buffer[1], buffer[0], buffer[5], buffer[4], buffer[7], buffer[6], buffer[8], buffer[9], buffer[10], buffer[11], buffer[12], buffer[13], buffer[14], buffer[15]];
            return MemoryMarshal.Cast<byte, Guid>(buffer)[0];
        }
        public static void WriteGuid(this Stream stream, Guid data)
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