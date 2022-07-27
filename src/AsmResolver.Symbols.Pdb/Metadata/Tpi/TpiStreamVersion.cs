namespace AsmResolver.Symbols.Pdb.Metadata.Tpi;

/// <summary>
/// Provides members defining all known file formats for the TPI stream.
/// </summary>
public enum TpiStreamVersion : uint
{
#pragma warning disable CS1591
    V40 = 19950410,
    V41 = 19951122,
    V50 = 19961031,
    V70 = 19990903,
    V80 = 20040203,
#pragma warning restore CS1591
}
