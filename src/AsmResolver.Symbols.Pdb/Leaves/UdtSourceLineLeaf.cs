namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a mapping from a user-defined type (UDT) to the source file where it is defined.
/// </summary>
public partial class UdtSourceLineLeaf : CodeViewLeaf, IIpiLeaf
{
    /// <summary>
    /// Initializes an empty UDT source line leaf.
    /// </summary>
    /// <param name="typeIndex">The type index.</param>
    protected UdtSourceLineLeaf(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new UDT source line leaf.
    /// </summary>
    /// <param name="type">The UDT's type record.</param>
    /// <param name="sourceFile">The source file string identifier.</param>
    /// <param name="line">The line number in the source file.</param>
    public UdtSourceLineLeaf(CodeViewTypeRecord type, ICodeViewLeaf sourceFile, uint line)
        : base(0)
    {
        UdtType = type;
        SourceFile = sourceFile;
        Line = line;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.UdtSrcLine;

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
    /// Gets or sets the source file string identifier.
    /// </summary>
    [LazyProperty]
    public partial ICodeViewLeaf? SourceFile
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
    /// Obtains the UDT's type record.
    /// </summary>
    /// <returns>The type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="UdtType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetUdtType() => null;

    /// <summary>
    /// Obtains the source file string identifier.
    /// </summary>
    /// <returns>The source file.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="SourceFile"/> property.
    /// </remarks>
    protected virtual ICodeViewLeaf? GetSourceFile() => null;

    /// <inheritdoc />
    public override string ToString() => $"{UdtType} @ line {Line}";
}
