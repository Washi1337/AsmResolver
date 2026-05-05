namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a mapping from a user-defined type (UDT) to the source file and module where it is defined.
/// </summary>
public partial class UdtModuleSourceLineLeaf : CodeViewLeaf, IIpiLeaf
{
    /// <summary>
    /// Initializes an empty UDT module source line leaf.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    protected UdtModuleSourceLineLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new UDT module source line leaf.
    /// </summary>
    /// <param name="type">The UDT's type record.</param>
    /// <param name="sourceFileOffset">The offset into the /names string table for the source file name.</param>
    /// <param name="line">The line number in the source file.</param>
    /// <param name="module">The module index that contributes this UDT definition.</param>
    public UdtModuleSourceLineLeaf(CodeViewTypeRecord type, uint sourceFileOffset, uint line, ushort module)
        : base(0)
    {
        UdtType = type;
        SourceFileOffset = sourceFileOffset;
        Line = line;
        Module = module;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.UdtModSrcLine;

    /// <summary>
    /// Gets or sets the UDT's type record.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? UdtType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the offset into the /names string table for the source file name.
    /// </summary>
    public uint SourceFileOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the line number in the source file.
    /// </summary>
    public uint Line
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the module index that contributes this UDT definition.
    /// </summary>
    public ushort Module
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the UDT's type record.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="UdtType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetUdtType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{UdtType} @ line {Line} (module {Module})";
}
