using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a thunk symbol in a PDB module.
/// </summary>
public class ThunkSymbol : CodeViewSymbol, IScopeCodeViewSymbol
{
    private readonly LazyVariable<Utf8String?> _name;

    private IList<ICodeViewSymbol>? _symbols;

    /// <summary>
    /// Initializes an empty thunk symbol.
    /// </summary>
    protected ThunkSymbol()
    {
        _name = new LazyVariable<Utf8String?>(GetName);
    }

    /// <summary>
    /// Creates a new named thunk symbol.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="size">The size of the thunk in bytes.</param>
    public ThunkSymbol(Utf8String name, ushort size)
    {
        _name = new LazyVariable<Utf8String?>(name);
        Size = size;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Thunk32;

    /// <inheritdoc />
    public IList<ICodeViewSymbol> Symbols
    {
        get
        {
            if (_symbols is null)
                Interlocked.CompareExchange(ref _symbols, GetSymbols(), null);
            return _symbols;
        }
    }

    /// <summary>
    /// Gets or sets the index of the segment the thunk is defined in.
    /// </summary>
    public ushort SegmentIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the segment the thunk is defined in.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the size of the thunk in bytes.
    /// </summary>
    public ushort Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the ordinal of the thunk.
    /// </summary>
    public ThunkOrdinal Ordinal
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the thunk.
    /// </summary>
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Obtains the symbols defined within the thunk.
    /// </summary>
    /// <returns>The symbols.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Symbols"/> property.
    /// </remarks>
    protected virtual IList<ICodeViewSymbol> GetSymbols() => new List<ICodeViewSymbol>();

    /// <summary>
    /// Obtains the name of the thunk.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_THUNK32: [{SegmentIndex:X4}:{Offset:X8}] {Name} ({Ordinal})";
}
