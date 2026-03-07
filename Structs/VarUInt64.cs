using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Me.Shiokawaii.IO.Structs
{
    public struct VarUInt64 : ISerializable, IConvertible
    {
        private ulong Value;
        private VarUInt64(ulong value)
        {
            Value = value;
        }
        public async Task SerializeAsync(SerialStream stream, CancellationToken cancellationToken = default)
        {
            ulong data = Value;
            do
            {
                byte current = (byte)data;
                data >>= 7;
                if (data != 0) current |= 128;
                else current &= 127;
                await stream.WriteAsync<byte>(current, cancellationToken);
            }
            while (data != 0);
        }
        public async Task DeserializeAsync(SerialStream stream, CancellationToken cancellationToken = default)
        {
            ulong data = 0;
            int position = 0;
            while (true)
            {
                byte current = await stream.ReadAsync<byte>(cancellationToken);
                data |= (current & 127U) << position;
                if ((current & 128) == 0) Value = data;
                position += 7;
                if (position >= sizeof(ulong) * 8) throw new ProtocolViolationException($"{nameof(VarUInt64)} size exceeded!");
            }
        }
        public TypeCode GetTypeCode()
        {
            return ((IConvertible)Value).GetTypeCode();
        }
        public bool ToBoolean(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToBoolean(provider);
        }
        public byte ToByte(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToByte(provider);
        }
        public char ToChar(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToChar(provider);
        }
        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToDateTime(provider);
        }
        public decimal ToDecimal(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToDecimal(provider);
        }
        public double ToDouble(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToDouble(provider);
        }
        public short ToInt16(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToInt16(provider);
        }
        public int ToInt32(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToInt32(provider);
        }
        public long ToInt64(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToInt64(provider);
        }
        public sbyte ToSByte(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToSByte(provider);
        }
        public float ToSingle(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToSingle(provider);
        }
        public string ToString(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToString(provider);
        }
        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToType(conversionType, provider);
        }
        public ushort ToUInt16(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToUInt16(provider);
        }
        public uint ToUInt32(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToUInt32(provider);
        }
        public ulong ToUInt64(IFormatProvider? provider)
        {
            return ((IConvertible)Value).ToUInt64(provider);
        }
        public static implicit operator ulong(VarUInt64 data)
        {
            return data.Value;
        }
        public static implicit operator VarUInt64(ulong data)
        {
            return new(data);
        }
    }
}