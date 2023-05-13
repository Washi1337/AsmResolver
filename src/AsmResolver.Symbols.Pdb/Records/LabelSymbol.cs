namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a named label within an instruction stream.
/// </summary>
public class LabelSymbol : CodeViewSymbol
{
    private readonly LazyVariable<LabelSymbol, Utf8String?> _name;

    /// <summary>
    /// Initializes an empty label symbol.
    /// </summary>
    protected LabelSymbol()
    {
        _name = new LazyVariable<LabelSymbol, Utf8String?>(x => x.GetName());
    }

    /// <summary>
    /// Creates a new label symbol.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <param name="segmentIndex">The index of the segment the label is defined in.</param>
    /// <param name="offset">The offset within the segment the label is defined at.</param>
    /// <param name="attributes">The attributes describing the label.</param>
    public LabelSymbol(Utf8String name, ushort segmentIndex, uint offset, ProcedureAttributes attributes)
    {
        _name = new LazyVariable<LabelSymbol, Utf8String?>(name);
        SegmentIndex = segmentIndex;
        Offset = offset;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Label32;

    /// <summary>
    /// Gets or sets the index of the segment the label is defined in.
    /// </summary>
    public ushort SegmentIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the segment the label is defined at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the attributes describing the label.
    /// </summary>
    public ProcedureAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the name of the label.
    /// </summary>
    public Utf8String? Name
    {
        get => _name.GetValue(this);
        set => _name.SetValue(value);
    }

    /// <summary>
    /// Obtains the name of the label.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String? GetName() => null;

    /// <inheritdoc />
    public override string ToString() => $"S_LABEL32: [{SegmentIndex:X4}:{Offset:X8}] {Name}";
}
