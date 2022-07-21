namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents an unknown or unsupported CodeView type record.
/// </summary>
public class UnknownCodeViewType : CodeViewLeaf
{
    /// <summary>
    /// Creates a new unknown type record.
    /// </summary>
    /// <param name="leafKind">The type of symbol.</param>
    /// <param name="data">The raw data stored in the record.</param>
    public UnknownCodeViewType(CodeViewLeafKind leafKind, byte[] data)
        : this(0, leafKind, data)
    {
    }

    /// <summary>
    /// Creates a new unknown type record.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the type</param>
    /// <param name="leafKind">The type of symbol.</param>
    /// <param name="data">The raw data stored in the record.</param>
    internal UnknownCodeViewType(uint typeIndex, CodeViewLeafKind leafKind, byte[] data)
        : base(typeIndex)
    {
        LeafKind = leafKind;
        Data = data;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind
    {
        get;
    }

    /// <summary>
    /// Gets the raw data stored in the record.
    /// </summary>
    public byte[] Data
    {
        get;
    }

    /// <inheritdoc />
    public override string ToString() => $"{LeafKind.ToString()} ({Data.Length.ToString()} bytes)";
}
