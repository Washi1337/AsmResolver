namespace AsmResolver.Symbols.Pdb.Records;

/// <summary>
/// Represents a public symbol stored in a PDB symbol stream.
/// </summary>
public partial class PublicSymbol : CodeViewSymbol
{
    /// <summary>
    /// Initializes a new empty public symbol.
    /// </summary>
    protected PublicSymbol()
    {
    }

    /// <summary>
    /// Creates a new public symbol.
    /// </summary>
    /// <param name="segmentIndex">The segment index.</param>
    /// <param name="offset">The offset within the segment the symbol starts at.</param>
    /// <param name="name">The name of the symbol.</param>
    /// <param name="attributes">The attributes associated to the symbol.</param>
    public PublicSymbol(ushort segmentIndex, uint offset, Utf8String name, PublicSymbolAttributes attributes)
    {
        SegmentIndex = segmentIndex;
        Offset = offset;
        Name = name;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public override CodeViewSymbolType CodeViewSymbolType => CodeViewSymbolType.Pub32;

    /// <summary>
    /// Gets or sets the file segment index this symbol is located in.
    /// </summary>
    public ushort SegmentIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset within the file that this symbol is defined at.
    /// </summary>
    public uint Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes associated to the public symbol.
    /// </summary>
    public PublicSymbolAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is a code symbol.
    /// </summary>
    public bool IsCode
    {
        get => (Attributes & PublicSymbolAttributes.Code) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Code)
                            | (value ? PublicSymbolAttributes.Code : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is a function symbol.
    /// </summary>
    public bool IsFunction
    {
        get => (Attributes & PublicSymbolAttributes.Function) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Function)
                            | (value ? PublicSymbolAttributes.Function : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol involves managed code.
    /// </summary>
    public bool IsManaged
    {
        get => (Attributes & PublicSymbolAttributes.Managed) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Managed)
                            | (value ? PublicSymbolAttributes.Managed : 0);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol involves MSIL code.
    /// </summary>
    public bool IsMsil
    {
        get => (Attributes & PublicSymbolAttributes.Msil) != 0;
        set => Attributes = (Attributes & ~PublicSymbolAttributes.Msil)
                            | (value ? PublicSymbolAttributes.Msil : 0);
    }

    /// <summary>
    /// Gets or sets the name of the symbol.
    /// </summary>
    [LazyProperty]
    public partial Utf8String Name
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the name of the public symbol.
    /// </summary>
    /// <returns>The name.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Name"/> property.
    /// </remarks>
    protected virtual Utf8String GetName() => Utf8String.Empty;

    /// <inheritdoc />
    public override string ToString() => $"S_PUB32: [{SegmentIndex:X4}:{Offset:X8}] {Name}";
}
