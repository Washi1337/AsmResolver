namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a bit field type.
/// </summary>
public class BitFieldTypeRecord : CodeViewTypeRecord
{
    private readonly LazyVariable<CodeViewTypeRecord?> _type;

    /// <summary>
    /// Initializes an empty bit field record.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the bit field type.</param>
    protected BitFieldTypeRecord(uint typeIndex)
        : base(typeIndex)
    {
        _type = new LazyVariable<CodeViewTypeRecord?>(GetBaseType);
    }

    /// <summary>
    /// Creates a new bit field record.
    /// </summary>
    /// <param name="type">The type of the bit field.</param>
    /// <param name="position">The bit index the bit field starts at.</param>
    /// <param name="length">The number of bits the bit field spans.</param>
    public BitFieldTypeRecord(CodeViewTypeRecord type, byte position, byte length)
        : base(0)
    {
        _type = new LazyVariable<CodeViewTypeRecord?>(type);
        Position = position;
        Length = length;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.BitField;

    /// <summary>
    /// Gets or sets the base type that this bit field is referencing.
    /// </summary>
    public CodeViewTypeRecord? Type
    {
        get => _type.Value;
        set => _type.Value = value;
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
    /// This method is called upon initialization of the <see cref="Type"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetBaseType() => null;

    /// <inheritdoc />
    public override string ToString() => $"{Type} : {Length}";
}
