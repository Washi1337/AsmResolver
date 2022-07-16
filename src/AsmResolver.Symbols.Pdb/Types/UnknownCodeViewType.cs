namespace AsmResolver.Symbols.Pdb.Types;

/// <summary>
/// Represents an unknown or unsupported CodeView type record.
/// </summary>
public class UnknownCodeViewType : CodeViewType
{
    /// <summary>
    /// Creates a new unknown type record.
    /// </summary>
    /// <param name="typeKind">The type of symbol.</param>
    /// <param name="data">The raw data stored in the record.</param>
    public UnknownCodeViewType(CodeViewTypeKind typeKind, byte[] data)
    {
        TypeKind = typeKind;
        Data = data;
    }

    /// <inheritdoc />
    public override CodeViewTypeKind TypeKind
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
    public override string ToString() => $"{TypeKind.ToString()} ({Data.Length.ToString()} bytes)";
}
