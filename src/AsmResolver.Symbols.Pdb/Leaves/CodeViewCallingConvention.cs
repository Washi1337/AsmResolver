namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Provides members defining all calling conventions that can be specified in a PDB image.
/// </summary>
public enum CodeViewCallingConvention : byte
{
    /// <summary>
    /// near right to left push, caller pops stack
    /// </summary>
    NearC = 0x00,

    /// <summary>
    /// far right to left push, caller pops stack
    /// </summary>
    FarC = 0x01,

    /// <summary>
    /// near left to right push, callee pops stack
    /// </summary>
    NearPascal = 0x02,

    /// <summary>
    /// far left to right push, callee pops stack
    /// </summary>
    FarPascal = 0x03,

    /// <summary>
    /// near left to right push with regs, callee pops stack
    /// </summary>
    NearFast = 0x04,

    /// <summary>
    /// far left to right push with regs, callee pops stack
    /// </summary>
    FarFast = 0x05,

    /// <summary>
    /// skipped (unused) call index
    /// </summary>
    Skipped = 0x06,

    /// <summary>
    /// near standard call
    /// </summary>
    NearStd = 0x07,

    /// <summary>
    /// far standard call
    /// </summary>
    FarStd = 0x08,

    /// <summary>
    /// near sys call
    /// </summary>
    NearSys = 0x09,

    /// <summary>
    /// far sys call
    /// </summary>
    FarSys = 0x0a,

    /// <summary>
    /// this call (this passed in register)
    /// </summary>
    ThisCall = 0x0b,

    /// <summary>
    /// Mips call
    /// </summary>
    MipsCall = 0x0c,

    /// <summary>
    /// Generic call sequence
    /// </summary>
    Generic = 0x0d,

    /// <summary>
    /// Alpha call
    /// </summary>
    AlphaCall = 0x0e,

    /// <summary>
    /// PPC call
    /// </summary>
    PpcCall = 0x0f,

    /// <summary>
    /// Hitachi SuperH call
    /// </summary>
    ShCall = 0x10,

    /// <summary>
    /// ARM call
    /// </summary>
    ArmCall = 0x11,

    /// <summary>
    /// AM33 call
    /// </summary>
    Am33Call = 0x12,

    /// <summary>
    /// TriCore Call
    /// </summary>
    TriCall = 0x13,

    /// <summary>
    /// Hitachi SuperH-5 call
    /// </summary>
    Sh5Call = 0x14,

    /// <summary>
    /// M32R Call
    /// </summary>
    M32RCall = 0x15,

    /// <summary>
    /// clr call
    /// </summary>
    ClrCall = 0x16,

    /// <summary>
    /// Marker for routines always inlined and thus lacking a convention
    /// </summary>
    Inline = 0x17,

    /// <summary>
    /// near left to right push with regs, callee pops stack
    /// </summary>
    NearVector = 0x18,
}
