#pragma warning disable 1591

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Provides members defining all valid element types for a safe array.
    /// </summary>
    public enum SafeArrayVariantType
    {
        Empty = 0,
        Null = 1,
        I2 = 2,
        I4 = 3,
        R4 = 4,
        R8 = 5,
        CY = 6,
        Date = 7,
        BStr = 8,
        Dispatch = 9,
        Error = 10,
        Bool = 11,
        Variant = 12,
        Unknown = 13,
        Decimal = 14,
        I1 = 16,
        UI1 = 17,
        UI2 = 18,
        UI4 = 19,
        I8 = 20,
        UI8 = 21,
        Int = 22,
        UInt = 23,
        Void = 24,
        HResult = 25,
        Ptr = 26,
        SafeArray = 27,
        CArray = 28,
        UserDefined = 29,
        LPStr = 30,
        LPWStr = 31,
        Record = 36,
        IntPtr = 37,
        UIntPtr = 38,
        FileTime = 64,
        Blob = 65,
        Stream = 66,
        Storage = 67,
        StreamedObject = 68,
        StoredObject = 69,
        BlobObject = 70,
        CF = 71,
        CLSID = 72,
        TypeMask = 0xfff,
        NotSet = 0xffff,
    }
}