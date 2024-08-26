namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents the type of contents a C13 sub section may contain.
/// </summary>
public enum C13SubSectionType
{
    /// <summary>
    /// Indicates the section contains symbol data.
    /// </summary>
    Symbols = 0xF1,

    /// <summary>
    /// Indicates the section contains line information.
    /// </summary>
    Lines,

    /// <summary>
    /// Indicates the section contains a string table.
    /// </summary>
    StringTable,

    /// <summary>
    /// Indicates the section contains file checksums.
    /// </summary>
    FileChecksums,

    /// <summary>
    /// Indicates the section contains frame data.
    /// </summary>
    FrameData,

    /// <summary>
    /// Indicates the section contains line information for inlinees.
    /// </summary>
    InlineeLines,

    /// <summary>
    /// Indicates the section contains cross scope imports.
    /// </summary>
    CrossScopeImports,

    /// <summary>
    /// Indicates the section contains cross scope exports.
    /// </summary>
    CrossScopeExports,

    /// <summary>
    /// Indicates the section contains line information for managed code (CIL).
    /// </summary>
    IlLines,

    /// <summary>
    /// Indicates the section contains metadata tokens of managed methods.
    /// </summary>
    FunctionMetadataTokenMap,

    /// <summary>
    /// Indicates the section contains metadata tokens of managed types.
    /// </summary>
    TypeMetadataTokenMap,

    /// <summary>
    /// Indicates the section contains merged assembly input information.
    /// </summary>
    MergedAssemblyInput,

    /// <summary>
    /// Indicates the section contains COFF symbol RVAs.
    /// </summary>
    CoffSymbolRva,
}
