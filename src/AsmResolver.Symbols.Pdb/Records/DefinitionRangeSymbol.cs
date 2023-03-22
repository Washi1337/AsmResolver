using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a definition range for a symbol.
/// </summary>
public abstract class DefinitionRangeSymbol : CodeViewSymbol
{
    private IList<LocalAddressGap>? _gaps;

    /// <summary>
    /// Initializes an empty def-range.
    /// </summary>
    protected DefinitionRangeSymbol()
    {
    }

    /// <summary>
    /// Initializes the def-range with the provided address range and gaps.
    /// </summary>
    /// <param name="range">The address range.</param>
    /// <param name="gaps">The gaps within the address range the symbol is invalid at.</param>
    protected DefinitionRangeSymbol(LocalAddressRange range, IEnumerable<LocalAddressGap> gaps)
    {
        Range = range;
        _gaps = new List<LocalAddressGap>(gaps);
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
    /// Gets or sets the range of addresses within the program where this symbol is valid.
    /// </summary>
    public LocalAddressRange Range
    {
        get;
        set;
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
