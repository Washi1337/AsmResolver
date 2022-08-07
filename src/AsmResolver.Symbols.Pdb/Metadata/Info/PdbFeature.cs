namespace AsmResolver.Symbols.Pdb.Metadata.Info;

/// <summary>
/// Provides members defining all possible features that a PDB can have.
/// </summary>
public enum PdbFeature : uint
{
    /// <summary>
    /// Indicates no other feature flags are present, and that an IPI stream is present.
    /// </summary>
    VC110 = 20091201,

    /// <summary>
    /// Indicates that other feature flags may be present, and that an IPI stream is present.
    /// </summary>
    VC140 = 20140508,

    /// <summary>
    /// Indicates types can be duplicated in the TPI stream.
    /// </summary>
    NoTypeMerge = 0x4D544F4E,

    /// <summary>
    /// Indicates the program was linked with /DEBUG:FASTLINK, and all type information is contained in the original
    /// object files instead of TPI and IPI streams.
    /// </summary>
    MinimalDebugInfo = 0x494E494D,
}
