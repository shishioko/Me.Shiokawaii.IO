using System;
using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Me.Shiokawaii.IO.Structs;

namespace Me.Shiokawaii.IO
{
    public class SerialStream : IDisposable, IAsyncDisposable
    {
        public bool AutoClose { get; init; } = false;
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
            return ReadAsync<T>().Result;
        }
        public object? Read(Type type)
        {
            return ReadAsync(type).Result;
        }
        public void Write<T>(T data)
        {
            WriteAsync<T>(data).Wait();
        }
        public void Write(Type type, object? data)
        {
            WriteAsync(type, data).Wait();
        }
        public async Task<T> ReadAsync<T>(CancellationToken cancellationToken = default)
        {
            return (T)(await ReadAsync(typeof(T), cancellationToken))!;
        }
        public async Task<object?> ReadAsync(Type type, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            if (type.IsPointer) throw new SerializationException("Attempted to deserialize pointer type!");
            if (type.IsClass)
            {
                if (Nullable.GetUnderlyingType(type) is not null) if (!await ReadAsync<bool>(cancellationToken)) return null;
            }
            if (type.IsAssignableTo(typeof(ISerializable)))
            {
                ISerializable serializable = (ISerializable)RuntimeHelpers.GetUninitializedObject(type);
                await serializable.DeserializeAsync(this, cancellationToken);
                return serializable;
            }
            if (type.IsArray)
            {
                if (type == typeof(bool[]))
                {
                    return MemoryMarshal.Cast<byte, bool>(await ReadAsync<byte[]>(cancellationToken)).ToArray();
                }
                if (type == typeof(byte[]))
                {
                    return await readBufferAsync(await readLengthAsync());
                }
                if (type == typeof(sbyte[]))
                {
                    return MemoryMarshal.Cast<byte, sbyte>(await ReadAsync<byte[]>(cancellationToken)).ToArray();
                }
                else
                {
                    int dimensions = type.GetArrayRank();
                    long[] indices = new long[dimensions];
                    long[] lengths = new long[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        indices[i] = 0;
                        lengths[i] = await readLengthAsync();
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
                byte[] buffer = await readBufferAsync(sizeof(ulong));
                if (LittleEndian) return (nuint)BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                else return (nuint)BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else if (type == typeof(nint))
            {
                byte[] buffer = await readBufferAsync(sizeof(long));
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
                byte[] buffer = await readBufferAsync(sizeof(ushort));
                if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (type == typeof(short))
            {
                byte[] buffer = await readBufferAsync(sizeof(short));
                if (LittleEndian) return BinaryPrimitives.ReadInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt16BigEndian(buffer);
            }
            else if (type == typeof(uint))
            {
                byte[] buffer = await readBufferAsync(sizeof(uint));
                if (LittleEndian) return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else if (type == typeof(int))
            {
                byte[] buffer = await readBufferAsync(sizeof(int));
                if (LittleEndian) return BinaryPrimitives.ReadInt32LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt32BigEndian(buffer);
            }
            else if (type == typeof(ulong))
            {
                byte[] buffer = await readBufferAsync(sizeof(ulong));
                if (LittleEndian) return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else if (type == typeof(long))
            {
                byte[] buffer = await readBufferAsync(sizeof(long));
                if (LittleEndian) return BinaryPrimitives.ReadInt64LittleEndian(buffer);
                else return BinaryPrimitives.ReadInt64BigEndian(buffer);
            }
            else if (type == typeof(float))
            {
                byte[] buffer = await readBufferAsync(sizeof(float));
                if (LittleEndian) return BinaryPrimitives.ReadSingleLittleEndian(buffer);
                else return BinaryPrimitives.ReadSingleBigEndian(buffer);
            }
            else if (type == typeof(double))
            {
                byte[] buffer = await readBufferAsync(sizeof(double));
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
                byte[] buffer = await readBufferAsync(sizeof(ushort));
                if (LittleEndian) return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
                else return BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (type == typeof(string))
            {
                return Encoding.Default.GetString(await readBufferAsync(await readLengthAsync()));
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
            async Task<long> readLengthAsync()
            {
                if (DynamicPrefix)
                {
                    if (LongPrefix) return await ReadAsync<VarInt64>(cancellationToken);
                    else return await ReadAsync<VarInt32>(cancellationToken);
                }
                else
                {
                    if (LongPrefix) return await ReadAsync<long>(cancellationToken);
                    else return await ReadAsync<int>(cancellationToken);
                }
            }
            async Task<byte[]> readBufferAsync(long length)
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
        public async Task WriteAsync<T>(T data, CancellationToken cancellationToken = default)
        {
            await WriteAsync(typeof(T), data, cancellationToken);
        }
        public async Task WriteAsync(Type type, object? data, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            if (data is not null && !data.GetType().IsAssignableTo(type)) throw new SerializationException("Attempted to serialize data of mismatching type!");
            if (type.IsPointer) throw new SerializationException("Attempted to serialize pointer type!");
            if (type.IsClass)
            {
                bool isNull = data is null;
                if (isNull && Nullable.GetUnderlyingType(type) is null) throw new SerializationException("Attempted to serialize non-nullable type as null!");
                if (Nullable.GetUnderlyingType(type) is not null) await WriteAsync(!isNull, cancellationToken);
                if (isNull) return;
            }
            if (type.IsAssignableTo(typeof(ISerializable)))
            {
                ISerializable serializable = (data as ISerializable)!;
                await serializable.SerializeAsync(this, cancellationToken);
                return;
            }
            if (type.IsArray)
            {
                if (data is bool[] u1a)
                {
                    await writeLengthAsync(u1a.LongLength);
                    await BaseStream.WriteAsync(new Memory<byte>(MemoryMarshal.AsBytes(new Span<bool>(u1a)).ToArray()), cancellationToken);
                }
                else if (data is byte[] u8a)
                {
                    await writeLengthAsync(u8a.LongLength);
                    await BaseStream.WriteAsync(u8a, cancellationToken);
                }
                else if (data is sbyte[] s8a)
                {
                    await writeLengthAsync(s8a.LongLength);
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
                        await writeLengthAsync(lengths[i] = array.GetLongLength(i));
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
                await writeLengthAsync(buffer.LongLength);
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
            return;
            async Task writeLengthAsync(long length)
            {
                if (!LongPrefix && length > int.MaxValue) throw new InvalidDataException();
                if (DynamicPrefix)
                {
                    if (LongPrefix) await WriteAsync<VarInt64>(length, cancellationToken);
                    else await WriteAsync<VarInt32>((int)length, cancellationToken);
                }
                else
                {
                    if (LongPrefix) await WriteAsync<long>(length, cancellationToken);
                    else await WriteAsync<int>((int)length, cancellationToken);
                }
            }
        }
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
            GC.SuppressFinalize(this);
        }
        public async ValueTask DisposeAsync()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            Disposed = true;
            if (AutoClose) await BaseStream.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
