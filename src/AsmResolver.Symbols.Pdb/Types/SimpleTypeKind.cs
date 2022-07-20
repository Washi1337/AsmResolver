namespace AsmResolver.Symbols.Pdb.Types;

/// <summary>
/// Provides members defining all basic type kinds that can be used as a type index.
/// </summary>
/// <remarks>
/// Reference: https://llvm.org/docs/PDB/TpiStream.html
/// </remarks>
public enum SimpleTypeKind : uint
{
    /// <summary>
    /// Indicates the type index indicates the absence of a specific type or type category.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// Indicates the type index references the void type.
    /// </summary>
    Void = 0x0003,

    /// <summary>
    /// Indicates the type index references a type that is not translated by CVPack.
    /// </summary>
    NotTranslated = 0x0007,

    /// <summary>
    /// Indicates the type index references the OLE/COM HRESULT type.
    /// </summary>
    HResult = 0x0008,

    /// <summary>
    /// Indicates the type index references the 8 bit signed character type.
    /// </summary>
    SignedCharacter = 0x0010,

    /// <summary>
    /// Indicates the type index references the 8 bit unsigned character type.
    /// </summary>
    UnsignedCharacter = 0x0020,

    /// <summary>
    /// Indicates the type index references the narrow character type.
    /// </summary>
    NarrowCharacter = 0x0070,

    /// <summary>
    /// Indicates the type index references the wide character type.
    /// </summary>
    WideCharacter = 0x0071,

    /// <summary>
    /// Indicates the type index references the char16_t type.
    /// </summary>
    Character16 = 0x007a,

    /// <summary>
    /// Indicates the type index references the char32_t type.
    /// </summary>
    Character32 = 0x007b,

    /// <summary>
    /// Indicates the type index references the char8_t type.
    /// </summary>
    Character8 = 0x007c,

    /// <summary>
    /// Indicates the type index references the 8 bit signed int type.
    /// </summary>
    SByte = 0x0068,

    /// <summary>
    /// Indicates the type index references the 8 bit unsigned int type.
    /// </summary>
    Byte = 0x0069,

    /// <summary>
    /// Indicates the type index references the 16 bit signed type.
    /// </summary>
    Int16Short = 0x0011,

    /// <summary>
    /// Indicates the type index references the 16 bit unsigned type.
    /// </summary>
    UInt16Short = 0x0021,

    /// <summary>
    /// Indicates the type index references the 16 bit signed int type.
    /// </summary>
    Int16 = 0x0072,

    /// <summary>
    /// Indicates the type index references the 16 bit unsigned int type.
    /// </summary>
    UInt16 = 0x0073,

    /// <summary>
    /// Indicates the type index references the 32 bit signed type.
    /// </summary>
    Int32Long = 0x0012,

    /// <summary>
    /// Indicates the type index references the 32 bit unsigned type.
    /// </summary>
    UInt32Long = 0x0022,

    /// <summary>
    /// Indicates the type index references the 32 bit signed int type.
    /// </summary>
    Int32 = 0x0074,

    /// <summary>
    /// Indicates the type index references the 32 bit unsigned int type.
    /// </summary>
    UInt32 = 0x0075,

    /// <summary>
    /// Indicates the type index references the 64 bit signed type.
    /// </summary>
    Int64Quad = 0x0013,

    /// <summary>
    /// Indicates the type index references the 64 bit unsigned type.
    /// </summary>
    UInt64Quad = 0x0023,

    /// <summary>
    /// Indicates the type index references the 64 bit signed int type.
    /// </summary>
    Int64 = 0x0076,

    /// <summary>
    /// Indicates the type index references the 64 bit unsigned int type.
    /// </summary>
    UInt64 = 0x0077,

    /// <summary>
    /// Indicates the type index references the 128 bit signed int type.
    /// </summary>
    Int128Oct = 0x0014,

    /// <summary>
    /// Indicates the type index references the 128 bit unsigned int type.
    /// </summary>
    UInt128Oct = 0x0024,

    /// <summary>
    /// Indicates the type index references the 128 bit signed int type.
    /// </summary>
    Int128 = 0x0078,

    /// <summary>
    /// Indicates the type index references the 128 bit unsigned int type.
    /// </summary>
    UInt128 = 0x0079,

    /// <summary>
    /// Indicates the type index references the 16 bit real type.
    /// </summary>
    Float16 = 0x0046,

    /// <summary>
    /// Indicates the type index references the 32 bit real type.
    /// </summary>
    Float32 = 0x0040,

    /// <summary>
    /// Indicates the type index references the 32 bit PP real type.
    /// </summary>
    Float32PartialPrecision = 0x0045,

    /// <summary>
    /// Indicates the type index references the 48 bit real type.
    /// </summary>
    Float48 = 0x0044,

    /// <summary>
    /// Indicates the type index references the 64 bit real type.
    /// </summary>
    Float64 = 0x0041,

    /// <summary>
    /// Indicates the type index references the 80 bit real type.
    /// </summary>
    Float80 = 0x0042,

    /// <summary>
    /// Indicates the type index references the 128 bit real type.
    /// </summary>
    Float128 = 0x0043,

    /// <summary>
    /// Indicates the type index references the 16 bit complex type.
    /// </summary>
    Complex16 = 0x0056,

    /// <summary>
    /// Indicates the type index references the 32 bit complex type.
    /// </summary>
    Complex32 = 0x0050,

    /// <summary>
    /// Indicates the type index references the 32 bit PP complex type.
    /// </summary>
    Complex32PartialPrecision = 0x0055,

    /// <summary>
    /// Indicates the type index references the 48 bit complex type.
    /// </summary>
    Complex48 = 0x0054,

    /// <summary>
    /// Indicates the type index references the 64 bit complex type.
    /// </summary>
    Complex64 = 0x0051,

    /// <summary>
    /// Indicates the type index references the 80 bit complex type.
    /// </summary>
    Complex80 = 0x0052,

    /// <summary>
    /// Indicates the type index references the 128 bit complex type.
    /// </summary>
    Complex128 = 0x0053,

    /// <summary>
    /// Indicates the type index references the 8 bit boolean type.
    /// </summary>
    Boolean8 = 0x0030,

    /// <summary>
    /// Indicates the type index references the 16 bit boolean type.
    /// </summary>
    Boolean16 = 0x0031,

    /// <summary>
    /// Indicates the type index references the 32 bit boolean type.
    /// </summary>
    Boolean32 = 0x0032,

    /// <summary>
    /// Indicates the type index references the 64 bit boolean type.
    /// </summary>
    Boolean64 = 0x0033,

    /// <summary>
    /// Indicates the type index references the 128 bit boolean type.
    /// </summary>
    Boolean128 = 0x0034,
}
