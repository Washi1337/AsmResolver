using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Defines an address range in which a local variable or parameter is defined that is fully defined by a register and
/// a relative starting offset and length.
/// </summary>
public class RelativeRegisterRange : CodeViewSymbol
{
    private IList<LocalAddressGap>? _gaps;

    /// <summary>
    /// Initializes an empty relative register range.
    /// </summary>
    protected RelativeRegisterRange()
    {
    }

    /// <summary>
    /// Creates a new range.
    /// </summary>
    /// <param name="baseRegister">The base register.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="range">The address range this range is valid.</param>
    /// <param name="gaps">A collection of gaps within the range that the symbol is invalid.</param>
    public RelativeRegisterRange(ushort baseRegister, int offset, LocalAddressRange range, IEnumerable<LocalAddressGap> gaps)
    {
        BaseRegister = baseRegister;
        Offset = offset;
        Range = range;
        _gaps = new List<LocalAddressGap>(gaps);
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

    /// <summary>
    /// Gets or sets the range of addresses within the program where this symbol is valid.
    /// </summary>
    public LocalAddressRange Range
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a collection of gaps in <see cref="Range"/> where this symbol is not available.
    /// </summary>
    public IList<LocalAddressGap> Gaps
    {
        get
        {
            if (_gaps is null)
                Interlocked.CompareExchange(ref _gaps, GetGaps(), null);
            return _gaps;
        }
    }

    /// <summary>
    /// Obtains the collection of ranges the symbol is unavailable.
    /// </summary>
    /// <returns>The gaps.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Gaps"/> property.
    /// </remarks>
    protected virtual IList<LocalAddressGap> GetGaps() => new List<LocalAddressGap>();
}
