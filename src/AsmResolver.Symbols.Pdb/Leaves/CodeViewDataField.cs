namespace AsmResolver.Symbols.Pdb.Leaves;

/// <summary>
/// Represents a data member in a class or structure type.
/// </summary>
public abstract partial class CodeViewDataField : CodeViewNamedField
{
    /// <summary>
    /// Initializes an empty instance data member.
    /// </summary>
    /// <param name="typeIndex">The type index to assign to the member.</param>
    protected CodeViewDataField(uint typeIndex)
        : base(typeIndex)
    {
    }

    /// <summary>
    /// Creates a new data member.
    /// </summary>
    /// <param name="dataType">The data type of the member.</param>
    /// <param name="name">The name of the member.</param>
    protected CodeViewDataField(CodeViewTypeRecord dataType, Utf8String name)
        : base(0)
    {
        DataType = dataType;
        Name = name;
    }

    /// <summary>
    /// Gets or sets the data type of the member.
    /// </summary>
    [LazyProperty]
    public partial CodeViewTypeRecord DataType
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
    public override string ToString() => $"{DataType} {Name}";
}
