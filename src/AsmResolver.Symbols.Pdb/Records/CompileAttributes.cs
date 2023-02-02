using System;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members describing all possible flags that can be assigned to a <see cref="CompileSymbol"/>.
/// </summary>
[Flags]
public enum CompileAttributes
{
    /// <summary>
    /// Indicates the file is compiled for Edit-and-Continue.
    /// </summary>
    EC = 1 << 1,

    /// <summary>
    /// Indicates the file is not compiled with debug info.
    /// </summary>
    NoDbgInfo = 1 << 2,

    /// <summary>
    /// Indicates the file is compiled with LTCG.
    /// </summary>
    Ltcg = 1 << 3,

    /// <summary>
    /// Indicates the file is compiled with <c>-Bzalign</c>.
    /// </summary>
    NoDataAlign = 1 << 4,

    /// <summary>
    /// Indicates managed code/data is present in the file.
    /// </summary>
    ManagedPresent = 1 << 5,

    /// <summary>
    /// Indicates the file is compiled with <c>/GS</c>.
    /// </summary>
    SecurityChecks = 1 << 6,

    /// <summary>
    /// Indicates the file is compiled with <c>/hotpatch</c>.
    /// </summary>
    HotPatch = 1 << 7,

    /// <summary>
    /// Indicates thef ile is converted with CVTCIL.
    /// </summary>
    CvtCil = 1 << 8,

    /// <summary>
    /// Indicates the file is a MSIL netmodule.
    /// </summary>
    MsilModule = 1 << 9,

    /// <summary>
    /// Indicates thef ile is compiled with <c>/sdl</c>.
    /// </summary>
    Sdl = 1 << 10,

    /// <summary>
    /// Indicates the file is compiled with <c>/ltcg:pgo</c> or <c>pgu</c>.
    /// </summary>
    Pgo = 1 << 11,

    /// <summary>
    /// Indicates the file is a <c>.exp</c> module.
    /// </summary>
    Exp = 1 << 12,
}
