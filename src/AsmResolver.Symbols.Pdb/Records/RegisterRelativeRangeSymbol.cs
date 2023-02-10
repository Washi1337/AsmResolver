using System.Collections.Generic;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Defines an address range in which a local variable or parameter is defined that is fully defined by a register and
/// a relative starting offset and length.
/// </summary>
public class RegisterRelativeRangeSymbol : DefinitionRangeSymbol
{
    /// <summary>
    /// Initializes an empty relative register range.
    /// </summary>
    protected RegisterRelativeRangeSymbol()
    {
    }

    /// <summary>
    /// Creates a new range.
    /// </summary>
    /// <param name="baseRegister">The base register.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="range">The address range this range is valid.</param>
    /// <param name="gaps">A collection of gaps within the range that the symbol is invalid.</param>
    public RegisterRelativeRangeSymbol(ushort baseRegister, int offset, LocalAddressRange range, IEnumerable<LocalAddressGap> gaps)
        : base(range, gaps)
    {
        BaseRegister = baseRegister;
        Offset = offset;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.DefRangeRegisterRel;

    /// <summary>
    /// Gets or sets the base register holding the pointer of the symbol.
    /// </summary>
    public ushort BaseRegister
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is spilled.
    /// </summary>
    public bool IsSpilledUdtMember
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the parent variable this symbol starts at.
    /// </summary>
    public int ParentOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset from the base register this symbol is defined at.
    /// </summary>
    public int Offset
    {
        get;
        set;
    }
}
