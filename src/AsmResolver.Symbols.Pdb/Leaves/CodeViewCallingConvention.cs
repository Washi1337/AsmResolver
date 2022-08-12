namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all calling conventions that can be specified in a PDB image.
/// </summary>
public enum CodeViewCallingConvention : byte
{
    /// <summary>
    /// Indicates a near call using the cdecl calling convention.
    /// </summary>
    NearC = 0x00,

    /// <summary>
    /// Indicates a far call using the cdecl calling convention.
    /// </summary>
    FarC = 0x01,

    /// <summary>
    /// Indicates a near call using the Pascal calling convention.
    /// </summary>
    NearPascal = 0x02,

    /// <summary>
    /// Indicates a far call using the Pascal calling convention.
    /// </summary>
    FarPascal = 0x03,

    /// <summary>
    /// Indicates a near call using the fastcall calling convention.
    /// </summary>
    NearFast = 0x04,

    /// <summary>
    /// Indicates a far call using the fastcall calling convention.
    /// </summary>
    FarFast = 0x05,

    /// <summary>
    /// Skipped (unused) call index
    /// </summary>
    Skipped = 0x06,

    /// <summary>
    /// Indicates a near call using the stdcall calling convention.
    /// </summary>
    NearStd = 0x07,

    /// <summary>
    /// Indicates a far call using the stdcall calling convention.
    /// </summary>
    FarStd = 0x08,

    /// <summary>
    /// Indicates a near call using the syscall calling convention.
    /// </summary>
    NearSys = 0x09,

    /// <summary>
    /// Indicates a far call using the syscall calling convention.
    /// </summary>
    FarSys = 0x0a,

    /// <summary>
    /// Indicates a call using the thiscall calling convention.
    /// </summary>
    ThisCall = 0x0b,

    /// <summary>
    /// Indicates a call using the MIPS calling convention.
    /// </summary>
    MipsCall = 0x0c,

    /// <summary>
    /// Indicates a generic calling sequence.
    /// </summary>
    Generic = 0x0d,

    /// <summary>
    /// Indicates a call using the Alpha calling convention.
    /// </summary>
    AlphaCall = 0x0e,

    /// <summary>
    /// Indicates a call using the PowerPC calling convention.
    /// </summary>
    PpcCall = 0x0f,

    /// <summary>
    /// Indicates a call using the Hitachi SuperH calling convention.
    /// </summary>
    ShCall = 0x10,

    /// <summary>
    /// Indicates a call using the ARM calling convention.
    /// </summary>
    ArmCall = 0x11,

    /// <summary>
    /// Indicates a call using the AM33  calling convention.
    /// </summary>
    Am33Call = 0x12,

    /// <summary>
    /// Indicates a call using the TriCore  calling convention.
    /// </summary>
    TriCall = 0x13,

    /// <summary>
    /// Indicates a call using the Hitachi SuperH-5  calling convention.
    /// </summary>
    Sh5Call = 0x14,

    /// <summary>
    /// Indicates a call using the M32R  calling convention.
    /// </summary>
    M32RCall = 0x15,

    /// <summary>
    /// Indicates a call using the clr  calling convention.
    /// </summary>
    ClrCall = 0x16,

    /// <summary>
    /// Marker for routines always inlined and thus lacking a convention.
    /// </summary>
    Inline = 0x17,

    /// <summary>
    /// Indicates a near call using the vectorcall calling convention.
    /// </summary>
    NearVector = 0x18,
}
