namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a bit field type.
/// </summary>
public partial class BitFieldTypeRecord : CodeViewTypeRecord
{
    /// <summary>
    /// Initializes an empty bit field record.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the bit field type.</param>
    protected BitFieldTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new bit field record.
    /// </summary>
    /// <param name="baseType">The type of the bit field.</param>
    /// <param name="position">The bit index the bit field starts at.</param>
    /// <param name="length">The number of bits the bit field spans.</param>
    public BitFieldTypeRecord(CodeViewTypeRecord baseType, byte position, byte length)
        : base(0)
    {
        BaseType = baseType;
        Position = position;
        Length = length;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BitField;

    /// <summary>
    /// Gets or sets the base type that this bit field is referencing.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord? BaseType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the bit index that this bit fields starts at.
    /// </summary>
    public byte Position
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bits that this bit fields spans.
    /// </summary>
    public byte Length
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the base type that the bit field is referencing.
    /// </summary>
    /// <returns>The base type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="BaseType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{BaseType} : {Length}";
}
