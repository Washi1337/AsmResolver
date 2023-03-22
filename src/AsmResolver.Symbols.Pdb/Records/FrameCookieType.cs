namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members defining all possible stack frame cookie types.
/// </summary>
// NOTE: Even though cvinfo.h doesn't explicitly specify an element type (and thus it would be an int according to C
//       specification), in practice a uint8_t is written by compilers instead to represent the cookie type. CVDump.exe
//      actually is bugged and reads arbitrary memory when dumping symbol information from a PDB file.
public enum FrameCookieType : byte
{
    /// <summary>
    /// Indicates the cookie is stored as a simple plain copy.
    /// </summary>
    Copy = 0,

    /// <summary>
    /// Indicates the cookie is stored after XOR'ing it with the stack pointer.
    /// </summary>
    XorSP,

    /// <summary>
    /// Indicates the cookie is stored after XOR'ing it with the base pointer.
    /// </summary>
    XorBP,

    /// <summary>
    /// Indicates the cookie is stored after XOR'ing it with R13.
    /// </summary>
    XorR13
}
