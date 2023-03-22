using AsmResolver.PE.File.Headers;

namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a single Common Object File Format (COFF) group symbol.
/// </summary>
public class CoffGroupSymbol : CodeViewSymbol
{
    private readonly LazyVariable<Utf8String?> _name;

    /// <summary>
    /// Initializes an empty COFF group symbol.
    /// </summary>
    protected CoffGroupSymbol()
    {
        _name = new LazyVariable<Utf8String?>(GetName);
    }

    /// <summary>
    /// Creates a new COFF group symbol.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <param name="segmentIndex">The index of the segment the group resides in.</param>
    /// <param name="offset">The offset within the segment.</param>
    /// <param name="size">The size of the group in bytes.</param>
    /// <param name="characteristics">The characteristics describing the group.</param>
    public CoffGroupSymbol(Utf8String name, ushort segmentIndex, uint offset, uint size, SectionFlags characteristics)
    {
        _name = new LazyVariable<Utf8String?>(name);
        SegmentIndex = segmentIndex;
        Offset = offset;
        Size = size;
        Characteristics = characteristics;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.CoffGroup;

    /// <summary>
    /// Gets or sets the size of the group in bytes.
    /// </summary>
    public uint Size
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the characteristics describing the group.
    /// </summary>
    public SectionFlags Characteristics
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index of the segment the group resides in.
    /// </summary>
    public ushort SegmentIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the segment the group starts at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the group.
    /// </summary>
    public Utf8String? Name
    {
        get => _name.Value;
        set => _name.Value = value;
    }

    /// <summary>
    /// Obtains the name of the group.
    /// </summary>
    /// <returns>The name</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_COFFGROUP: [{SegmentIndex:X4}:{Offset:X8}], Cb: {Size:X8}, Name: {Name}";
}
