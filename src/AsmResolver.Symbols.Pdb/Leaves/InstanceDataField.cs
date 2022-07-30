namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents an instance data member in a class or structure type.
/// </summary>
public class InstanceDataField : CodeViewField
{
    private readonly LazyVariable<CodeViewTypeRecord> _dataType;

    /// <summary>
    /// Initializes an empty instance data member.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    protected InstanceDataField(uint typeIndex)
        : base(typeIndex)
    {
        _dataType = new LazyVariable<CodeViewTypeRecord>(GetDataType);
    }

    /// <summary>
    /// Creates a new instance data member.
    /// </summary>
    /// <param name="dataType">The data type of the member.</param>
    /// <param name="name">The name of the member.</param>
    /// <param name="offset">The byte offset within the class or structure that the member is stored at.</param>
    public InstanceDataField(CodeViewTypeRecord dataType, Utf8String name, ulong offset)
        : base(0)
    {
        _dataType = new LazyVariable<CodeViewTypeRecord>(dataType);
        Name = name;
        Offset = offset;
    }

    /// <inheritdoc />
    public override CodeViewLeafKind LeafKind => CodeViewLeafKind.Member;

    /// <summary>
    /// Gets or sets the data type of the member.
    /// </summary>
    public CodeViewTypeRecord DataType
    {
        get => _dataType.Value;
        set => _dataType.Value = value;
    }

    /// <summary>
    /// Gets or sets the byte offset within the class or structure that the member is stored at.
    /// </summary>
    public ulong Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Obtains the data type of the member.
    /// </summary>
    /// <returns>The data type.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="DataType"/> property.
    /// </remarks>
    protected virtual CodeViewTypeRecord? GetDataType() => null;

    /// <inheritdoc />
    public override string ToString() => $"[{Offset:X4}]: {DataType} {Name}";
}
