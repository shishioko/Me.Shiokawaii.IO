using System;
using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO
{
    public class SerialStream : IDisposable, IAsyncDisposable
    {
        public bool AutoClose { get; init; } = false;
        public bool DisallowNull { get; init; } = false;
        public bool DynamicPrefix { get; init; } = false;
        public bool LongPrefix { get; init; } = false;
        public bool LittleEndian { get; init; } = false;
        //public bool CheckSignature { get; init; } = false;
        private readonly Stream BaseStream;
        private bool Disposed = false;
        public SerialStream(Stream baseStream)
        {
            BaseStream = baseStream;
        }
        public T Read<T>()
        {
            return (T)Read(typeof(T))!;
        }
        public object? Read(Type type)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (type.IsPointer) throw new InvalidDataException();
            if (type.IsClass)
            {
                if (!DisallowNull || Nullable.GetUnderlyingType(type) is not null) if (!Read<bool>()) return null;
            }
            if (type.IsArray)
            {
                if (type == typeof(bool[]))
                {
                    return MemoryMarshal.Cast<byte, bool>(Read<byte[]>()).ToArray();
                }
                if (type == typeof(byte[]))
                {
                    return ReadBuffer(ReadLength());
                }
                if (type == typeof(sbyte[]))
                {
                    return MemoryMarshal.Cast<sbyte, bool>(Read<sbyte[]>()).ToArray();
                }
                else
                {
                    int dimensions = type.GetArrayRank();
                    long[] indices = new long[dimensions];
                    long[] lengths = new long[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        indices[i] = 0;
                        lengths[i] = ReadLength();
                    }
                    Array array = Array.CreateInstance(type.GetElementType()!, lengths);
                    Type basetype = type.GetElementType()!;
                    while (indices[0] < lengths[0])
                    {
                        for (int i = dimensions - 1; i >= 0; i++)
                        {
                            if (indices[i] < lengths[i]) break;
                            if (i == 0) break;
                            indices[i] = 0;
                            indices[i - 1]++;
                            continue;
                        }
                        if (indices[0] < lengths[0]) break;
                        array.SetValue(Read(basetype), indices);
                        indices[dimensions - 1]++;
                    }
                    return array;
                }
            }
            else if (type.IsEnum)
            {
                return Enum.ToObject(type, Read(type.GetEnumUnderlyingType())!);
            }
            else if (type == typeof(nuint))
            {
                Span<byte> buffer = ReadBuffer(sizeof(ulong));
                if (LittleEndian) return (nuint)BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                else return (nuint)BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else if (type == typeof(nint))
            {
                Span<byte> buffer = ReadBuffer(sizeof(long));
                if (LittleEndian) return (nint)BinaryPrimitives.ReadInt64LittleEndian(buffer);
                else return (nint)BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else if (type == typeof(bool))
            {
                Span<byte> buffer = new byte[1];
                if (BaseStream.Read(buffer) <= 0) throw new EndOfStreamException();
                return MemoryMarshal.Cast<byte, bool>(buffer)[0];
            }
            else if (type == typeof(byte))
            {
                byte[] buffer = new byte[1];
                if (BaseStream.Read(buffer, 0, buffer.Length) <= 0) throw new EndOfStreamException();
                return buffer[0];
            }
            else if (type == typeof(sbyte))
            {
                Span<byte> buffer = new byte[1];
                if (BaseStream.Read(buffer) <= 0) throw new EndOfStreamException();
                return MemoryMarshal.Cast<byte, sbyte>(buffer)[0];
            }
            else if (type == typeof(ushort))
            {
                Span<byte> buffer = ReadBuffer(sizeof(ushort));
                if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (type == typeof(short))
            {
                Span<byte> buffer = ReadBuffer(sizeof(short));
                if (LittleEndian) return BinaryPrimitives.ReadInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            else if (type == typeof(uint))
            {
                Span<byte> buffer = ReadBuffer(sizeof(uint));
                if (LittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else if (type == typeof(int))
            {
                Span<byte> buffer = ReadBuffer(sizeof(int));
                if (LittleEndian) return BinaryPrimitives.ReadInt32LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            else if (type == typeof(ulong))
            {
                Span<byte> buffer = ReadBuffer(sizeof(ulong));
                if (LittleEndian) return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else if (type == typeof(long))
            {
                Span<byte> buffer = ReadBuffer(sizeof(long));
                if (LittleEndian) return BinaryPrimitives.ReadInt64LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else if (type == typeof(float))
            {
                Span<byte> buffer = ReadBuffer(sizeof(float));
                if (LittleEndian) return BinaryPrimitives.ReadSingleLittleEndian(buffer);
                else return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            else if (type == typeof(double))
            {
                Span<byte> buffer = ReadBuffer(sizeof(double));
                if (LittleEndian) return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
                else return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
            else if (type == typeof(decimal))
            {
                Span<int> buffer = new Span<int>(null, 0, 4);
                for (int i = 0; i < sizeof(decimal) / sizeof(int); i++) buffer[i] = Read<int>();
                return new decimal(buffer);
            }

            else if (type == typeof(char))
            {
                Span<byte> buffer = ReadBuffer(sizeof(ushort));
                if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (type == typeof(string))
            {
                return Encoding.Default.GetString(ReadBuffer(ReadLength()));
            }
            else
            {
                object data = RuntimeHelpers.GetUninitializedObject(type);
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    field.SetValue(data, Read(field.FieldType));
                }
                return data;
            }
            long ReadLength()
            {
                if (DynamicPrefix)
                {
                    ulong value = 0x00;
                    int position = 0;
                    while (true)
                    {
                        ulong current = Read<byte>();
                        value |= (current & 0x7F) << position;
                        position += 7;
                        if ((current & 0x80) == 0x00) break;
                        if (position >= (LongPrefix ? sizeof(ulong) : sizeof(uint)) * 8) throw new InvalidDataException();
                    }
                    return (long)value;
                }
                else
                {
                    if (LongPrefix) return Read<long>();
                    else return Read<int>();
                }
            }
            byte[] ReadBuffer(long length)
            {
                byte[] buffer = new byte[length];
                int position = 0;
                while (position < buffer.Length)
                {
                    int read = BaseStream.Read(buffer, position, buffer.Length - position);
                    if (read <= 0) throw new EndOfStreamException();
                    position += read;
                }
                return buffer;
            }
        }
        public void Write(object? data)
        {
            Write(data?.GetType(), data);
        }
        public void Write<T>(T data)
        {
            Write(typeof(T), data);
        }
        public void Write(Type? type, object? data)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            if (type is null)
            {
                if (DisallowNull) throw new InvalidDataException();
                Write(false);
                return;
            }
            if (type.IsPointer) throw new InvalidDataException();
            if (type.IsClass)
            {
                bool set = data is not null;
                if (!set && DisallowNull && Nullable.GetUnderlyingType(type) is null) throw new InvalidDataException();
                if (!DisallowNull || Nullable.GetUnderlyingType(type) is not null) Write(set);
                if (!set) return;
            }
            if (!data!.GetType().IsSubclassOf(type) && data.GetType() != type) throw new InvalidDataException();
            if (type.IsArray)
            {
                if (data is bool[] u1a)
                {
                    WriteLength(u1a.LongLength);
                    BaseStream.Write(MemoryMarshal.AsBytes(new Span<bool>(u1a)));
                }
                else if (data is byte[] u8a)
                {
                    WriteLength(u8a.LongLength);
                    BaseStream.Write(u8a);
                }
                else if (data is sbyte[] s8a)
                {
                    WriteLength(s8a.LongLength);
                    BaseStream.Write(MemoryMarshal.AsBytes(new Span<sbyte>(s8a)));
                }
                else
                {
                    Array array = (data as Array)!;
                    Type basetype = type.GetElementType()!;
                    int dimensions = array.Rank;
                    long[] indices = new long[dimensions];
                    long[] lengths = new long[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        indices[i] = 0;
                        WriteLength(lengths[i] = array.GetLongLength(i));
                        if (array.GetLowerBound(i) > 0) throw new InvalidDataException();
                    }
                    while (indices[0] < lengths[0])
                    {
                        for (int i = dimensions - 1; i >= 0; i++)
                        {
                            if (indices[i] < lengths[i]) break;
                            if (i == 0) break;
                            indices[i] = 0;
                            indices[i - 1]++;
                            continue;
                        }
                        if (indices[0] < lengths[0]) break;
                        Write(basetype, array.GetValue(indices));
                        indices[dimensions - 1]++;
                    }
                }
            }
            else if (type.IsEnum)
            {
                Type basetype = type.GetEnumUnderlyingType();
                Write(basetype, Convert.ChangeType(data, basetype));
            }
            else if (data is nuint u0)
            {
                Span<byte> buffer = stackalloc byte[sizeof(ulong)];
                if (LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(buffer, u0);
                else BinaryPrimitives.WriteUInt64BigEndian(buffer, u0);
                BaseStream.Write(buffer);
            }
            else if (data is nint s0)
            {
                Span<byte> buffer = stackalloc byte[sizeof(long)];
                if (LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(buffer, s0);
                else BinaryPrimitives.WriteInt64BigEndian(buffer, s0);
                BaseStream.Write(buffer);
            }
            else if (data is bool u1) BaseStream.Write(MemoryMarshal.AsBytes([u1]));
            else if (data is byte u8) BaseStream.Write([u8]);
            else if (data is sbyte s8) BaseStream.Write(MemoryMarshal.AsBytes([s8]));
            else if (data is ushort u16)
            {
                Span<byte> buffer = stackalloc byte[sizeof(ushort)];
                if (LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(buffer, u16);
                else BinaryPrimitives.WriteUInt16BigEndian(buffer, u16);
                BaseStream.Write(buffer);
            }
            else if (data is short s16)
            {
                Span<byte> buffer = stackalloc byte[sizeof(short)];
                if (LittleEndian) BinaryPrimitives.WriteInt16LittleEndian(buffer, s16);
                else BinaryPrimitives.WriteInt16BigEndian(buffer, s16);
                BaseStream.Write(buffer);
            }
            else if (data is uint u32)
            {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                if (LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(buffer, u32);
                else BinaryPrimitives.WriteUInt32BigEndian(buffer, u32);
                BaseStream.Write(buffer);
            }
            else if (data is int s32)
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                if (LittleEndian) BinaryPrimitives.WriteInt32LittleEndian(buffer, s32);
                else BinaryPrimitives.WriteInt32BigEndian(buffer, s32);
                BaseStream.Write(buffer);
            }
            else if (data is ulong u64)
            {
                Span<byte> buffer = stackalloc byte[sizeof(ulong)];
                if (LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(buffer, u64);
                else BinaryPrimitives.WriteUInt64BigEndian(buffer, u64);
                BaseStream.Write(buffer);
            }
            else if (data is long s64)
            {
                Span<byte> buffer = stackalloc byte[sizeof(long)];
                if (LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(buffer, s64);
                else BinaryPrimitives.WriteInt64BigEndian(buffer, s64);
                BaseStream.Write(buffer);
            }
            else if (data is float f32)
            {
                Span<byte> buffer = stackalloc byte[sizeof(float)];
                if (LittleEndian) BinaryPrimitives.WriteSingleLittleEndian(buffer, f32);
                else BinaryPrimitives.WriteSingleBigEndian(buffer, f32);
                BaseStream.Write(buffer);
            }
            else if (data is double f64)
            {
                Span<byte> buffer = stackalloc byte[sizeof(double)];
                if (LittleEndian) BinaryPrimitives.WriteDoubleLittleEndian(buffer, f64);
                else BinaryPrimitives.WriteDoubleBigEndian(buffer, f64);
                BaseStream.Write(buffer);
            }
            else if (data is decimal f128)
            {
                Span<int> buffer = decimal.GetBits(f128);
                for (int i = 0; i < sizeof(decimal) / sizeof(int); i++) Write(buffer[i]);
            }
            else if (data is char tu16)
            {
                Span<byte> buffer = stackalloc byte[sizeof(ushort)];
                if (LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(buffer, tu16);
                else BinaryPrimitives.WriteUInt16BigEndian(buffer, tu16);
                BaseStream.Write(buffer);
            }
            else if (data is string tu16a)
            {
                byte[] buffer = Encoding.Default.GetBytes(tu16a);
                WriteLength(buffer.LongLength);
                BaseStream.Write(buffer);
            }
            else
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    Write(field.FieldType, field.GetValue(data));
                }
            }
            void WriteLength(long length)
            {
                if (!LongPrefix && length > int.MaxValue) throw new InvalidDataException();
                if (DynamicPrefix)
                {
                    ulong value = LongPrefix ? (ulong)length : (uint)length;
                    do
                    {
                        ulong current = value & 0x7F;
                        value >>= 7;
                        BaseStream.Write([(byte)(current | (value != 0UL ? 0x80UL : 0x00UL))]);
                    }
                    while (value != 0);
                }
                else
                {
                    if (LongPrefix) Write(length);
                    else Write((int)length);
                }
            }
        }
        public async Task<T> ReadAsync<T>(CancellationToken cancellationToken = default)
        {
            return (T)(await ReadAsync(typeof(T), cancellationToken))!;
        }
        public async Task<object?> ReadAsync(Type type, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            if (type.IsPointer) throw new InvalidDataException();
            if (type.IsClass)
            {
                if (!DisallowNull || Nullable.GetUnderlyingType(type) is not null) if (!await ReadAsync<bool>(cancellationToken)) return null;
            }
            if (type.IsArray)
            {
                if (type == typeof(bool[]))
                {
                    return MemoryMarshal.Cast<byte, bool>(await ReadAsync<byte[]>(cancellationToken)).ToArray();
                }
                if (type == typeof(byte[]))
                {
                    return await ReadBufferAsync(await ReadLengthAsync(cancellationToken), cancellationToken);
                }
                if (type == typeof(sbyte[]))
                {
                    return MemoryMarshal.Cast<sbyte, bool>(await ReadAsync<sbyte[]>(cancellationToken)).ToArray();
                }
                else
                {
                    int dimensions = type.GetArrayRank();
                    long[] indices = new long[dimensions];
                    long[] lengths = new long[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        indices[i] = 0;
                        lengths[i] = await ReadLengthAsync(cancellationToken);
                    }
                    Array array = Array.CreateInstance(type.GetElementType()!, lengths);
                    Type basetype = type.GetElementType()!;
                    while (indices[0] < lengths[0])
                    {
                        for (int i = dimensions - 1; i >= 0; i++)
                        {
                            if (indices[i] < lengths[i]) break;
                            if (i == 0) break;
                            indices[i] = 0;
                            indices[i - 1]++;
                            continue;
                        }
                        if (indices[0] < lengths[0]) break;
                        array.SetValue(await ReadAsync(basetype, cancellationToken), indices);
                        indices[dimensions - 1]++;
                    }
                    return array;
                }
            }
            else if (type.IsEnum)
            {
                return Enum.ToObject(type, (await ReadAsync(type.GetEnumUnderlyingType(), cancellationToken))!);
            }
            else if (type == typeof(nuint))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(ulong), cancellationToken);
                if (LittleEndian) return (nuint)BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                else return (nuint)BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else if (type == typeof(nint))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(long), cancellationToken);
                if (LittleEndian) return (nint)BinaryPrimitives.ReadInt64LittleEndian(buffer);
                else return (nint)BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else if (type == typeof(bool))
            {
                byte[] buffer = new byte[1];
                if (await BaseStream.ReadAsync(buffer, cancellationToken) <= 0) throw new EndOfStreamException();
                return MemoryMarshal.Cast<byte, bool>(buffer)[0];
            }
            else if (type == typeof(byte))
            {
                byte[] buffer = new byte[1];
                if (await BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken) <= 0) throw new EndOfStreamException();
                return buffer[0];
            }
            else if (type == typeof(sbyte))
            {
                byte[] buffer = new byte[1];
                if (await BaseStream.ReadAsync(buffer, cancellationToken) <= 0) throw new EndOfStreamException();
                return MemoryMarshal.Cast<byte, sbyte>(buffer)[0];
            }
            else if (type == typeof(ushort))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(ushort), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (type == typeof(short))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(short), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            else if (type == typeof(uint))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(uint), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else if (type == typeof(int))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(int), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadInt32LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            else if (type == typeof(ulong))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(ulong), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else if (type == typeof(long))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(long), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadInt64LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else if (type == typeof(float))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(float), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadSingleLittleEndian(buffer);
                else return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            else if (type == typeof(double))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(double), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
                else return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            }
            else if (type == typeof(decimal))
            {
                int[] buffer = new int[4];
                for (int i = 0; i < sizeof(decimal) / sizeof(int); i++) buffer[i] = await ReadAsync<int>(cancellationToken);
                return new decimal(buffer);
            }

            else if (type == typeof(char))
            {
                byte[] buffer = await ReadBufferAsync(sizeof(ushort), cancellationToken);
                if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (type == typeof(string))
            {
                return Encoding.Default.GetString(await ReadBufferAsync(await ReadLengthAsync(cancellationToken), cancellationToken));
            }
            else
            {
                object data = RuntimeHelpers.GetUninitializedObject(type);
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    field.SetValue(data, await ReadAsync(field.FieldType, cancellationToken));
                }
                return data;
            }
            async Task<long> ReadLengthAsync(CancellationToken cancellationToken = default)
            {
                if (DynamicPrefix)
                {
                    ulong value = 0x00;
                    int position = 0;
                    while (true)
                    {
                        ulong current = await ReadAsync<byte>(cancellationToken);
                        value |= (current & 0x7F) << position;
                        position += 7;
                        if ((current & 0x80) == 0x00) break;
                        if (position >= (LongPrefix ? sizeof(ulong) : sizeof(uint)) * 8) throw new InvalidDataException();
                    }
                    return (long)value;
                }
                else
                {
                    if (LongPrefix) return await ReadAsync<long>();
                    else return await ReadAsync<int>();
                }
            }
            async Task<byte[]> ReadBufferAsync(long length, CancellationToken cancellationToken = default)
            {
                byte[] buffer = new byte[length];
                int position = 0;
                while (position < buffer.Length)
                {
                    int read = await BaseStream.ReadAsync(buffer, position, buffer.Length - position, cancellationToken);
                    if (read <= 0) throw new EndOfStreamException();
                    position += read;
                }
                return buffer;
            }
        }

        public async Task WriteAsync(object? data, CancellationToken cancellationToken = default)
        {
            await WriteAsync(data?.GetType(), data, cancellationToken);
        }
        public async Task WriteAsync<T>(T data, CancellationToken cancellationToken = default)
        {
            await WriteAsync(typeof(T), data, cancellationToken);
        }
        public async Task WriteAsync(Type? type, object? data, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            if (type is null)
            {
                if (DisallowNull) throw new InvalidDataException();
                await WriteAsync(false, cancellationToken);
                return;
            }
            if (type.IsPointer) throw new InvalidDataException();
            if (type.IsClass)
            {
                bool set = data is not null;
                if (!set && DisallowNull && Nullable.GetUnderlyingType(type) is null) throw new InvalidDataException();
                if (!DisallowNull || Nullable.GetUnderlyingType(type) is not null) await WriteAsync(set, cancellationToken);
                if (!set) return;
            }
            if (!data!.GetType().IsSubclassOf(type) && data.GetType() != type) throw new InvalidDataException();
            if (type.IsArray)
            {
                if (data is bool[] u1a)
                {
                    await WriteLengthAsync(u1a.LongLength, cancellationToken);
                    await BaseStream.WriteAsync(new Memory<byte>(MemoryMarshal.AsBytes(new Span<bool>(u1a)).ToArray()), cancellationToken);
                }
                else if (data is byte[] u8a)
                {
                    await WriteLengthAsync(u8a.LongLength, cancellationToken);
                    await BaseStream.WriteAsync(u8a, cancellationToken);
                }
                else if (data is sbyte[] s8a)
                {
                    await WriteLengthAsync(s8a.LongLength, cancellationToken);
                    await BaseStream.WriteAsync(new Memory<byte>(MemoryMarshal.AsBytes(new Span<sbyte>(s8a)).ToArray()), cancellationToken);
                }
                else
                {
                    Array array = (data as Array)!;
                    Type basetype = type.GetElementType()!;
                    int dimensions = array.Rank;
                    long[] indices = new long[dimensions];
                    long[] lengths = new long[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        indices[i] = 0;
                        await WriteLengthAsync(lengths[i] = array.GetLongLength(i), cancellationToken);
                        if (array.GetLowerBound(i) > 0) throw new InvalidDataException();
                    }
                    while (indices[0] < lengths[0])
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        for (int i = dimensions - 1; i >= 0; i++)
                        {
                            if (indices[i] < lengths[i]) break;
                            if (i == 0) break;
                            indices[i] = 0;
                            indices[i - 1]++;
                            continue;
                        }
                        if (indices[0] < lengths[0]) break;
                        await WriteAsync(basetype, array.GetValue(indices), cancellationToken);
                        indices[dimensions - 1]++;
                    }
                }
            }
            else if (type.IsEnum)
            {
                Type basetype = type.GetEnumUnderlyingType();
                await WriteAsync(basetype, Convert.ChangeType(data, basetype), cancellationToken);
            }
            else if (data is nuint u0)
            {
                byte[] buffer = new byte[sizeof(ulong)];
                if (LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(buffer, u0);
                else BinaryPrimitives.WriteUInt64BigEndian(buffer, u0);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is nint s0)
            {
                byte[] buffer = new byte[sizeof(long)];
                if (LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(buffer, s0);
                else BinaryPrimitives.WriteInt64BigEndian(buffer, s0);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is bool u1) await BaseStream.WriteAsync(new Memory<byte>(MemoryMarshal.AsBytes(new Span<bool>([u1])).ToArray()), cancellationToken);
            else if (data is byte u8) await BaseStream.WriteAsync(new Memory<byte>([u8]), cancellationToken);
            else if (data is sbyte s8) await BaseStream.WriteAsync(new Memory<byte>(MemoryMarshal.AsBytes(new Span<sbyte>([s8])).ToArray()), cancellationToken);
            else if (data is ushort u16)
            {
                byte[] buffer = new byte[sizeof(ushort)];
                if (LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(buffer, u16);
                else BinaryPrimitives.WriteUInt16BigEndian(buffer, u16);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is short s16)
            {
                byte[] buffer = new byte[sizeof(short)];
                if (LittleEndian) BinaryPrimitives.WriteInt16LittleEndian(buffer, s16);
                else BinaryPrimitives.WriteInt16BigEndian(buffer, s16);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is uint u32)
            {
                byte[] buffer = new byte[sizeof(uint)];
                if (LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(buffer, u32);
                else BinaryPrimitives.WriteUInt32BigEndian(buffer, u32);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is int s32)
            {
                byte[] buffer = new byte[sizeof(int)];
                if (LittleEndian) BinaryPrimitives.WriteInt32LittleEndian(buffer, s32);
                else BinaryPrimitives.WriteInt32BigEndian(buffer, s32);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is ulong u64)
            {
                byte[] buffer = new byte[sizeof(ulong)];
                if (LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(buffer, u64);
                else BinaryPrimitives.WriteUInt64BigEndian(buffer, u64);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is long s64)
            {
                byte[] buffer = new byte[sizeof(long)];
                if (LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(buffer, s64);
                else BinaryPrimitives.WriteInt64BigEndian(buffer, s64);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is float f32)
            {
                byte[] buffer = new byte[sizeof(float)];
                if (LittleEndian) BinaryPrimitives.WriteSingleLittleEndian(buffer, f32);
                else BinaryPrimitives.WriteSingleBigEndian(buffer, f32);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is double f64)
            {
                byte[] buffer = new byte[sizeof(double)];
                if (LittleEndian) BinaryPrimitives.WriteDoubleLittleEndian(buffer, f64);
                else BinaryPrimitives.WriteDoubleBigEndian(buffer, f64);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is decimal f128)
            {
                int[] buffer = decimal.GetBits(f128);
                for (int i = 0; i < sizeof(decimal) / sizeof(int); i++) await WriteAsync(buffer[i], cancellationToken);
            }
            else if (data is char tu16)
            {
                byte[] buffer = new byte[sizeof(ushort)];
                if (LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(buffer, tu16);
                else BinaryPrimitives.WriteUInt16BigEndian(buffer, tu16);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else if (data is string tu16a)
            {
                byte[] buffer = Encoding.Default.GetBytes(tu16a);
                await WriteLengthAsync(buffer.LongLength, cancellationToken);
                await BaseStream.WriteAsync(buffer, cancellationToken);
            }
            else
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    await WriteAsync(field.FieldType, field.GetValue(data), cancellationToken);
                }
            }
            async Task WriteLengthAsync(long length, CancellationToken cancellationToken)
            {
                if (!LongPrefix && length > int.MaxValue) throw new InvalidDataException();
                if (DynamicPrefix)
                {
                    ulong value = LongPrefix ? (ulong)length : (uint)length;
                    do
                    {
                        ulong current = value & 0x7F;
                        value >>= 7;
                        await BaseStream.WriteAsync(new Memory<byte>([(byte)(current | (value != 0UL ? 0x80UL : 0x00UL))]), cancellationToken);
                    }
                    while (value != 0);
                }
                else
                {
                    if (LongPrefix) await WriteAsync(length, cancellationToken);
                    else await WriteAsync((int)length, cancellationToken);
                }
            }
        }
        public void Dispose()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            if (AutoClose) BaseStream.Dispose();
        }
        public async ValueTask DisposeAsync()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            if (AutoClose) await BaseStream.DisposeAsync();
        }
    }
}
