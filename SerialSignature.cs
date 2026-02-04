namespace Me.Shiokawaii.IO
{
    internal class SerialSignature
    {
        private enum Signature : byte
        {
            U1 = 0,
            U8 = 1,
            S8 = 2,
            U16 = 3,
            S16 = 4,
            U32 = 5,
            S32 = 6,
            U64 = 7,
            S64 = 8,
            U0 = 9,
            S0 = 10,
            F32 = 11,
            F64 = 12,
            F128 = 13,
            TU16 = 14,
            TU16A = 15,
            Class = 16,
            Array = 17,
            Enum = 18,
            Nullable = 19,
        }
    }
}
