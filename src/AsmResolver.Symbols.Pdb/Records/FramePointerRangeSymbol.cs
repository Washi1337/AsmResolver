using System.Linq;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a symbol def-range that is stored at an offset relative to the frame pointer.
/// </summary>
public class FramePointerRangeSymbol : DefinitionRangeSymbol
{
    private bool _isFullScope;

    /// <summary>
    /// Initializes an empty frame-pointer def-range.
    /// </summary>
    protected FramePointerRangeSymbol()
    {
    }

    /// <summary>
    /// Creates a new frame-pointer def-range.
    /// </summary>
    /// <param name="offset">The offset the symbol is declared at, relative to the frame pointer.</param>
    /// <param name="range">The address range the symbol is valid in.</param>
    public FramePointerRangeSymbol(int offset, LocalAddressRange range)
        : base(range, Enumerable.Empty<LocalAddressGap>())
    {
        Offset = offset;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => IsFullScope
        ? CodeViewSymbolType.DefRangeFramePointerRelFullScope
        : CodeViewSymbolType.DefRangeFramePointerRel;

    /// <summary>
    /// Gets or sets a value indicating whether the symbol spans the full scope of the function or not.
    /// </summary>
    /// <remarks>
    /// When this value is set to <c>true</c>, the values in <see cref="DefinitionRangeSymbol.Range"/> and
    /// <see cref="DefinitionRangeSymbol.Gaps"/> have no meaning.
    /// </remarks>
    public bool IsFullScope
    {
        get => _isFullScope;
        set
        {
            _isFullScope = value;

            if (value)
            {
                Range = default;
                Gaps.Clear();
            }
        }
    }

    /// <summary>
    /// Gets or sets the offset the symbol is declared at, relative to the frame pointer.
    /// </summary>
    public int Offset
    {
        get;
        set;
    }
}
