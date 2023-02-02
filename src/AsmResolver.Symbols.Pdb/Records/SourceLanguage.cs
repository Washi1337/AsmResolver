namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members describing all languages that the PDB file format supports.
/// </summary>
public enum SourceLanguage : byte
{
#pragma warning disable CS1591
    C = 0x00,
    Cpp = 0x01,
    Fortran = 0x02,
    Masm = 0x03,
    Pascal = 0x04,
    Basic = 0x05,
    Cobol = 0x06,
    Link = 0x07,
    Cvtres = 0x08,
    Cvtpgd = 0x09,
    CSharp = 0x0a,
    VB = 0x0b,
    ILAsm = 0x0c,
    Java = 0x0d,
    JScript = 0x0e,
    MSIL = 0x0f,
    HLSL = 0x10,

    Rust = 0x15,

    D = (byte)'D',
    Swift = (byte)'S',
#pragma warning restore CS1591
}
