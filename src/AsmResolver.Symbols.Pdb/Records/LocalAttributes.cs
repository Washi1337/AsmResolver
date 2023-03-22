using System;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Provides members defining all possible flags that can be assigned to a <see cref="LocalSymbol"/>.
/// </summary>
/// <remarks>
/// This enum is in direct correspondence of the <c>CV_LVARFLAGS</c> enum in <c>cvinfo.h</c>.
/// </remarks>
[Flags]
public enum LocalAttributes : ushort
{
    /// <summary>
    /// Indicates the variable is a parameter.
    /// </summary>
    Parameter = 1 << 0,

    /// <summary>
    /// Indicates the address is taken.
    /// </summary>
    AddrTaken = 1 << 1,

    /// <summary>
    /// Indicates the variable is compiler generated.
    /// </summary>
    CompilerGenerated = 1 << 2,

    /// <summary>
    /// Indicates the the symbol is split in temporaries, which are treated by compiler as independent entities.
    /// </summary>
    Aggregate = 1 << 3,

    /// <summary>
    /// Indicates the Counterpart of <see cref="Aggregate" /> and tells that it is a part of an
    /// <see cref="Aggregate" /> symbol.
    /// </summary>
    Aggregated = 1 << 4,

    /// <summary>
    /// Indicates the variable has multiple simultaneous lifetimes.
    /// </summary>
    Aliased = 1 << 5,

    /// <summary>
    /// Indicates the represents one of the multiple simultaneous lifetimes.
    /// </summary>
    Alias = 1 << 6,

    /// <summary>
    /// Indicates the represents a function return value.
    /// </summary>
    ReturnValue = 1 << 7,

    /// <summary>
    /// Indicates the variable has no lifetimes.
    /// </summary>
    OptimizedOut = 1 << 8,

    /// <summary>
    /// Indicates the variable is an enregistered global.
    /// </summary>
    EnregisteredGlobal = 1 << 9,

    /// <summary>
    /// Indicates the variable is an enregistered static.
    /// </summary>
    EnregisteredStatic = 1 << 10,

}
