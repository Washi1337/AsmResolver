namespace AsmResolver.Net.Signatures
{
    public enum VariantType
    {
        I2 = 0x02,
        I4 = 0x03,
        R4 = 0x04,
        R8 = 0x05,
        Cy = 0x06,
        Date = 0x07,
        BStr = 0x08,
        Dispatch = 0x09,
        Error = 0x0A,
        Bool = 0x0B,
        Variant = 0x0C,
        Unknown = 0x0D,
        Decimal = 0x0E,
        I1 = 0x10,
        UI1 = 0x11,
        UI2 = 0x12,
        UI4 = 0x13,
        Int = 0x25,
        Uint = 0x26,
        Max = 0x50,
    }
}